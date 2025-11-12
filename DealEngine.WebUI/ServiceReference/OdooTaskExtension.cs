using Google.Protobuf.WellKnownTypes;
using ServiceStack;
using System.Collections.Generic;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace DealEngine.WebUI.ServiceReference
{
    public sealed class OdooTaskExtension : IDisposable
    {
        private readonly HttpClient _http;
        private readonly string _api;    // e.g. https://not4profit.online/jsonrpc
        private readonly string _db;     // not4profitodoo18
        private readonly string _login;  // ashuchauhan@verinsure.online
        private readonly string _key;    // API key
        private int _uid;

        public OdooTaskExtension(string api, string db, string login, string key, HttpClient? http = null)
        { _http = http ?? new HttpClient(); _api = api.TrimEnd('/'); _db = db; _login = login; _key = key; }

        public void Dispose() => _http.Dispose();

        private async Task<T> RpcAsync<T>(object payload)
        {
            var req = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var res = await _http.PostAsync(_api, req);
            res.EnsureSuccessStatusCode();
            using var doc = JsonDocument.Parse(await res.Content.ReadAsStringAsync());
            if (doc.RootElement.TryGetProperty("error", out var err)) throw new InvalidOperationException(err.ToString());
            return JsonSerializer.Deserialize<T>(doc.RootElement.GetProperty("result").GetRawText())!;
        }

        public async Task LoginAsync()
        {
            var payload = new
            {
                jsonrpc = "2.0",
                method = "call",
                id = 1,
                @params = new { service = "common", method = "login", args = new object[] { _db, _login, _key } }
            };

            var req = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var res = await _http.PostAsync(_api, req);
            res.EnsureSuccessStatusCode();

            var raw = await res.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(raw);

            // If Odoo returned an RPC error, bubble it up with details
            if (doc.RootElement.TryGetProperty("error", out var err))
                throw new InvalidOperationException($"Odoo RPC error: {err}");

            if (!doc.RootElement.TryGetProperty("result", out var result))
                throw new InvalidOperationException($"Odoo RPC: missing 'result'. Raw: {raw}");

            // Successful login returns an integer uid; failed login returns JSON boolean false
            if (result.ValueKind == JsonValueKind.Number && result.TryGetInt32(out var uid))
            {
                _uid = uid;
                if (_uid <= 0) throw new UnauthorizedAccessException("Odoo login failed (uid <= 0).");
            }
            else if (result.ValueKind == JsonValueKind.False)
            {
                throw new UnauthorizedAccessException("Odoo login failed: check DB/login/API key.");
            }
            else
            {
                throw new InvalidOperationException($"Unexpected login result: {result}. Raw: {raw}");
            }
        }


        public async Task<int?> GetUserIdByEmailAsync(string email)
        {
            var payload = new
            {
                jsonrpc = "2.0",
                method = "call",
                id = 2,
                @params = new
                {
                    service = "object",
                    method = "execute_kw",
                    args = new object[] {
                    _db, _uid, _key,
                    "res.users", "search",
                    new object[] { new object[] { new object[] { "login", "=", email } } },
                    new Dictionary<string,object> { ["limit"] = 1 }
                }
                }
            };
            var ids = await RpcAsync<int[]>(payload);
            return ids.Length > 0 ? ids[0] : null;
        }

        public async Task<int> CreateTaskAsync(string title, int projectId, string? notes = null,
                                               DateTime? deadline = null, int? assigneeUserId = null,
                                               IEnumerable<int>? tagIds = null)
        {
            var vals = new Dictionary<string, object?>
            {
                ["name"] = title,
                ["project_id"] = projectId
            };
            if (!string.IsNullOrWhiteSpace(notes)) vals["description"] = notes;
          //  if (deadline is DateTime d) vals["date_deadline"] = d.ToString("yyyy-MM-dd");
           // if (assigneeUserId is int u) vals["user_id"] = u;
           // if (tagIds is not null) vals["tag_ids"] = new object[] { new object[] { 6, 0, tagIds.ToArray() } };

            var payload = new
            {
                jsonrpc = "2.0",
                method = "call",
                id = 3,
                @params = new
                {
                    service = "object",
                    method = "execute_kw",
                    args = new object[] { _db, _uid, _key, "project.task", "create", new object[] { vals } }
                }
            };
            return await RpcAsync<int>(payload);
        }
    }
}
