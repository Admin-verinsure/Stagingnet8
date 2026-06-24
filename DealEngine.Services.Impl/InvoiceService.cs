using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Services.Interfaces.Models;
namespace DealEngine.Services.Impl
{
     public class InvoiceService : IInvoiceService
     {
        private const string MATERIAL_DAMAGE = "Rotary Material Damage";
        private const string GLOBAL_GUARD = " Rotary Association-Multinational Liability (Global Guard GL)";


        IAppSettingService _appSettingService;
        public InvoiceService(IAppSettingService appSettingService)
        {
            _appSettingService = appSettingService;
        }
        public async Task<InvoiceGenerationResult> GenerateInvoiceAsync(
           ClientInformationSheet sheet, ClientProgramme programme) {
            int quantity = CalculateQuantity(sheet);

            decimal globalGuardPremium = programme.Agreements
                .Where(a => a.DateDeleted == null
                         && a.Product?.Name == GLOBAL_GUARD)
                .Sum(a =>
                    (a.ClientAgreementTerms ?? Enumerable.Empty<ClientAgreementTerm>())
                    .Where(t => t.DateDeleted == null && t.Bound)
                    .Sum(t => t.Premium));

            decimal adminFeeQty = quantity + 1;

            if (programme.BaseProgramme.SendInvoiceToOdoo)
            {
               return  await SendInvoicePayloadPOC(
                    programme.InformationSheet,
                    programme,
                    quantity,
                    globalGuardPremium,
                    adminFeeQty);
            }
            else 
            {
                return new InvoiceGenerationResult
                {
                    Success = false,
                    Message = "Programme not configured to send invoice to Odoo."
                };
            }
        }

        public async Task<InvoiceGenerationResult> SendInvoicePayloadPOC(
          ClientInformationSheet sheet,
          ClientProgramme programme,
          decimal materialDamageQty,
          decimal globalGuardPremium,
          decimal adminFeeQty)
        {
            if (sheet is null || programme is null)
            {
                return new InvoiceGenerationResult
                {
                    Success = false,
                    Message = "Invalid input."
                };
            }


            var totalAmount = materialDamageQty + globalGuardPremium + adminFeeQty;

            if (totalAmount <= 0) return new InvoiceGenerationResult
            {
                Success = false,
                Message = "Invoice amount must be > 0."
            };

            try
            {
                var api = _appSettingService.OdooServerworkingendpoint.TrimEnd('/');
                var db = _appSettingService.OdooServerDB;
                var login = _appSettingService.LoginID;
                var key = _appSettingService.LoginKey;

                const int COMPANY_ID = 82;
                const string MATERIAL_DAMAGE = "Rotary Material Damage";
                const string GLOBAL_GUARD = "Rotary Multinational Liability (Global Guard GL)";

                using var http = new HttpClient
                {
                    Timeout = TimeSpan.FromMinutes(5)
                };

                // 🔐 LOGIN
                var uid = await RpcAsync<int>(http, api, new
                {
                    jsonrpc = "2.0",
                    method = "call",
                    id = 1,
                    @params = new
                    {
                        service = "common",
                        method = "login",
                        args = new object[] { db, login, key }
                    }
                });

                if (uid <= 0)
                    throw new UnauthorizedAccessException("Odoo login failed.");

                // ============================================================
                // 🧾 BUILD LINES
                // ============================================================

                var lines = new List<object>();

                if (materialDamageQty > 0)
                {
                    lines.Add(new
                    {
                        name = MATERIAL_DAMAGE,
                        qty = materialDamageQty,
                        product_guid = "bbfc4377-af90-41ae-a69b-e7d23caf1284"
                    });
                }

                if (globalGuardPremium > 0)
                {
                    lines.Add(new
                    {
                        name = GLOBAL_GUARD,
                        qty = globalGuardPremium,
                        product_guid = "fe4852f3-de8f-442f-8fd9-60defb9a9d3e"
                    });
                }

                if (adminFeeQty > 0)
                {
                    lines.Add(new
                    {
                        name = "Administrator Fee",
                        qty = adminFeeQty,
                        product_guid = "0592a35a-4e8c-4139-804f-de4686e691e0"
                    });
                }

                var extRef = $"EXT-POLICY-{DateTime.UtcNow:yyyyMMddHHmmss}";
                var policyNum = long.Parse("1" + new Random().Next(0, 999_999_999).ToString("D9"));

                // ============================================================
                // 📦 BUILD PAYLOAD
                // ============================================================

                var payload = new
                {
                    ext_ref = extRef,

                    customer = new
                    {
                        name = sheet.Owner?.Name ?? sheet.Owner?.Email ?? "Customer",
                        email = sheet.Owner?.Email ?? "admin@verinsure.online",
                        external_guid = sheet.Owner?.External_guid

                    },

                    currency = "NZD",
                    invoice_date = DateTime.UtcNow.ToString("yyyy-MM-dd"),
                    due_date = DateTime.UtcNow.AddDays(14).ToString("yyyy-MM-dd"),

                    salesperson = new
                    {
                        login = login
                    },

                    policy = new
                    {
                        type_name = programme?.BaseProgramme?.Name ?? "Policy",
                        name = programme?.BaseProgramme?.Name ?? "Policy",
                        amount = totalAmount,
                        policy_number = policyNum,
                        policy_duration = 12,
                        payment_type = "fixed",

                        agent = new
                        {
                            name = "Craig Horrocks",
                            email = "craigehorrocksnz@gmail.com",
                            phone = "6421683800"
                        }
                    },

                    lines = lines.ToArray()
                };

                var payloadJson = JsonConvert.SerializeObject(payload, Newtonsoft.Json.Formatting.None);

                // ============================================================
                // 📝 CREATE invoice.poc.payload
                // ============================================================

                var createPayloadRec = ExecKwEnvelope(
                    db,
                    uid,
                    key,
                    "invoice.poc.payload",
                    "create",
                    new object[]
                    {
                new object[]
                {
                    new Dictionary<string, object?>
                    {
                        ["ext_id"] = extRef,
                        ["payload_json"] = payloadJson
                    }
                }
                    },
                    new Dictionary<string, object>
                    {
                        ["context"] = new Dictionary<string, object>
                        {
                            ["allowed_company_ids"] = new int[] { COMPANY_ID },
                            ["force_company"] = COMPANY_ID
                        }
                    }
                );

                var recId = await RpcAsync<int>(http, api, createPayloadRec);

                // ============================================================
                // 🚀 RUN FLOW
                // ============================================================

                var runFlow = ExecKwEnvelope(
                    db,
                    uid,
                    key,
                    "invoice.poc.payload",
                    "action_create_policy_and_invoice",
                    new object[] { new object[] { recId } },
                    new Dictionary<string, object>
                    {
                        ["context"] = new Dictionary<string, object>
                        {
                            ["allowed_company_ids"] = new int[] { COMPANY_ID },
                            ["force_company"] = COMPANY_ID
                        }
                    }
                );

                await RpcAsync<object>(http, api, runFlow);

                return new InvoiceGenerationResult
                {
                    Success = true,
                    Message = "Invoice generated successfully."
                };
            }
            catch (Exception ex)
            {
                return new InvoiceGenerationResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        private object ExecKwEnvelope(
       string db,
       int uid,
       string key,
       string model,
       string method,
       object[] args,
       Dictionary<string, object>? kwargs = null)
        {
            return new
            {
                jsonrpc = "2.0",
                method = "call",
                id = 1,
                @params = new
                {
                    service = "object",
                    method = "execute_kw",
                    args = new object[]
                    {
                db,
                uid,
                key,
                model,
                method,
                args,
                kwargs ?? new Dictionary<string, object>()
                    }
                }
            };
        }

        private static async Task<T> RpcAsync<T>(HttpClient http, string api, object body)
        {
            var json = JsonConvert.SerializeObject(body);
            using var res = await http.PostAsync(
                api,
                new StringContent(json, Encoding.UTF8, "application/json")
            );

            var raw = await res.Content.ReadAsStringAsync();
            res.EnsureSuccessStatusCode();

            var jo = JObject.Parse(raw);

            // 🔴 Odoo-side error
            if (jo["error"] != null)
                throw new InvalidOperationException(jo["error"]!.ToString());

            var result = jo["result"];
            if (result == null)
                throw new InvalidOperationException("Odoo RPC returned null result.");

            // 🔥 Handle [id] → id
            if (result.Type == JTokenType.Array &&
                result.Count() == 1 &&
                typeof(T) != typeof(JArray))
            {
                return result.First!.ToObject<T>()!;
            }

            // 🔥 Handle null for void-like calls
            if (result.Type == JTokenType.Null)
            {
                return default!;
            }

            return result.ToObject<T>()!;
        }

        private int CalculateQuantity(ClientInformationSheet sheet)
        {
            int quantity = 0;
            int clubtrust1onlyCount = 0;

            foreach (var organisation in sheet.Organisation
                .Where(org => !org.Removed &&
                              org.OrganisationType?.Name != "Private"))
            {
                var validUnits = organisation.OrganisationalUnits
                    .Where(u =>
                        u.DateDeleted == null &&
                        u.Name != "Corporation – Limited liability" &&
                        u.Name != "Administrator")
                    .ToList();

                clubtrust1onlyCount +=
                    validUnits.Count(u => u.Name == "RotaryClubTrustOneOnly");

                quantity +=
                    validUnits.Count(u => u.Name != "RotaryClubTrustOneOnly");
            }

            if (clubtrust1onlyCount > 1)
            {
                quantity += (clubtrust1onlyCount - 1);
            }

            return quantity;
        }

        // -
    }

   


}


    

      

