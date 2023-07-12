using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;
using System.Data;
using System.IO;
using ClosedXML.Excel;
using System.Diagnostics;
using NHibernate.SqlCommand;
using Microsoft.AspNetCore.Mvc;

namespace DealEngine.Services.Impl
{
    public class ReportBuilderService : IReportBuilderService
    {
        IProgrammeService _programmeService;
        IUserService _userService;

        public ReportBuilderService(IProgrammeService programmeServicey, IUserService userservice)
        {
            _programmeService = programmeServicey;
            _userService = userservice;
        }

        public async Task<DataTable> GetReportView(Guid programmeId, string reportName)
        {

            Programme programme = await _programmeService.GetProgrammeById(programmeId);
            List<List<dynamic>> ListReportSet = new List<List<dynamic>>();
            List<dynamic> ListReport = new List<dynamic>();
            DataTable dt = new DataTable();
            decimal PIFullPremiumTotal = 0M;
            decimal PIGrossPremiumTotal = 0M;
            decimal PINetPremiumToInsurerTotal = 0M;

            //var ReportingDay = DateTime.Parse("9/10/2022 12:00:00 AM");
            //var ReportingMonth = new DateTime(ReportingDay.Year, ReportingDay.Month, 1);
            //var ReportingFirstDay = ReportingMonth.AddMonths(-1);
            //var ReportingLastDay = ReportingMonth.AddDays(-1);

            var ReportingDay = DateTime.Today;
            var ReportingMonth = new DateTime(ReportingDay.Year, ReportingDay.Month, 1);
            var ReportingFirstDay = ReportingMonth.AddMonths(-1);
            var ReportingLastDay = ReportingMonth.AddDays(-1);

            DataColumn column1 = new DataColumn();
            DataColumn column2 = new DataColumn();
            DataColumn column3 = new DataColumn();
            DataColumn column4 = new DataColumn();
            DataColumn column5 = new DataColumn();
            DataColumn column6 = new DataColumn();
            DataColumn column7 = new DataColumn();
            DataColumn column8 = new DataColumn();
            DataColumn column9 = new DataColumn();
            DataColumn column10 = new DataColumn();
            DataColumn column11 = new DataColumn();

            column1 = new DataColumn("Client Number", typeof(string));
            column2 = new DataColumn("Invoice number%", typeof(string));
            column3 = new DataColumn("Client", typeof(string));
            column4 = new DataColumn("Limit", typeof(decimal));
            column5 = new DataColumn("Excess", typeof(decimal));

            var reportNameformal = reportName;
            if (reportName.Contains("lumely"))
            {

                if (reportName.Contains("ML"))
                {
                    column6 = new DataColumn("Gross Premium 12.5%", typeof(decimal));
                    column7 = new DataColumn("Brokerage%", typeof(decimal));
                    column8 = new DataColumn("Brokerage", typeof(decimal));
                    column9 = new DataColumn("GST", typeof(decimal));
                    column10 = new DataColumn("Brokerage GST", typeof(decimal));
                    column11 = new DataColumn("Net Premium to insurer", typeof(decimal));

                }
                else if (reportName.Contains("PI"))
                {
                    column6 = new DataColumn("Gross Premium 25%", typeof(decimal));
                    column7 = new DataColumn("Brokerage%", typeof(decimal));
                    column8 = new DataColumn("Brokerage", typeof(decimal));
                    column9 = new DataColumn("GST", typeof(decimal));
                    column10 = new DataColumn("Brokerage GST", typeof(decimal));
                    column11 = new DataColumn("Net Premium to insurer", typeof(decimal));

                }


            }
            else if (reportName.Contains("AIG"))
            {
                if (reportName.Contains("ML"))
                {
                    column6 = new DataColumn("Gross Premium 87.5%", typeof(decimal));
                    column7 = new DataColumn("Brokerage%", typeof(decimal));
                    column8 = new DataColumn("Brokerage", typeof(decimal));
                    column9 = new DataColumn("GST", typeof(decimal));
                    column10 = new DataColumn("Brokerage GST", typeof(decimal));
                    column11 = new DataColumn("Net Premium to insurer", typeof(decimal));

                }
                else if (reportName.Contains("PI"))
                {
                    column6 = new DataColumn("Gross Premium 75%", typeof(decimal));
                    column7 = new DataColumn("Brokerage%", typeof(decimal));
                    column8 = new DataColumn("Brokerage", typeof(decimal));
                    column9 = new DataColumn("GST", typeof(decimal));
                    column10 = new DataColumn("Brokerage GST", typeof(decimal));
                    column11 = new DataColumn("Net Premium to insurer", typeof(decimal));


                }
                else if (reportName.Contains("CL"))
                {
                    column6 = new DataColumn("Gross Premium 100%", typeof(decimal));
                    column7 = new DataColumn("Brokerage%", typeof(decimal));
                    column8 = new DataColumn("Brokerage", typeof(decimal));
                    column9 = new DataColumn("GST", typeof(decimal));
                    column10 = new DataColumn("Brokerage GST", typeof(decimal));
                    column11 = new DataColumn("Net Premium to insurer", typeof(decimal));

                }

            }

            dt.Columns.Add(column1);
            dt.Columns.Add(column2);
            dt.Columns.Add(column3);
            dt.Columns.Add(column4);
            dt.Columns.Add(column5);
            dt.Columns.Add(column6);
            dt.Columns.Add(column7);
            dt.Columns.Add(column8);
            dt.Columns.Add(column9);
            dt.Columns.Add(column10);
            dt.Columns.Add(column11);




            if (reportName.Contains("PI"))
            {
                reportName = "PI";
            }
            else if (reportName.Contains("ML"))
            {
                reportName = "ML";
            }
            else if (reportName.Contains("CL"))
            {
                reportName = "CL";
            }

            foreach (ClientProgramme cp in programme.ClientProgrammes.Where(o => o.InformationSheet.DateDeleted == null &&
                                                                                 o.InformationSheet.Status == "Bound and invoiced"))
            {
                try
                {
                    Guid clientInformationSheetID = Guid.NewGuid();
                    if (cp.BaseProgramme.Id == programme.Id)
                    {
                        clientInformationSheetID = cp.InformationSheet.Id;
                    }
                    List<dynamic> tempListReport = new List<dynamic>();
                    decimal PIFullPremium = 0M;
                    decimal PIGrossPremium = 0M;
                    decimal PINetPremiumToInsurer = 0M;
                    decimal PIFullPremiumtotal = 0M;
                    decimal Brokerageperc = 0M;
                    decimal Brokerage = 0M;
                    decimal GST = 0M;
                    decimal BrokerageGST = 0M;

                    Organisation organisation = cp.InformationSheet.Owner;
                    User user = await _userService.GetApplicationUserByEmail(organisation.Email);
                    if (cp.Agreements.Count > 0)
                    {
                        foreach (ClientAgreement agreement in cp.Agreements.Where(agree => agree.BoundDate >= ReportingFirstDay && agree.BoundDate <= ReportingLastDay && agree.InceptionDate < ReportingFirstDay
                                                                                              || agree.InceptionDate >= ReportingFirstDay && agree.InceptionDate <= ReportingLastDay && agree.BoundDate <= ReportingLastDay))
                        {
                            var term = agreement.ClientAgreementTerms.FirstOrDefault(ter => ter.SubTermType == reportName && ter.Bound == true);
                            if (term != null)
                            {
                                DataRow newRow = dt.NewRow();

                                tempListReport = new List<dynamic>();
                                newRow[0] = cp.EGlobalClientNumber.ToString();  //Add((cp.EGlobalClientNumber).ToString());
                                EGlobalResponse eGlobalResponse = cp.ClientAgreementEGlobalResponses.Where(er => er.DateDeleted == null && er.ResponseType == "update").OrderByDescending(er => er.VersionNumber).FirstOrDefault();
                                if (eGlobalResponse != null)
                                {
                                    newRow[1] = "I" + eGlobalResponse.InvoiceNumber.ToString();
                                }
                                newRow[2] = cp.InformationSheet.Owner.Name;
                                newRow[3] = term.TermLimit;
                                newRow[4] = term.Excess;
                                int ceextensionlimit = 0;
                                decimal ceextensionexcess = 0M;
                                decimal ceextensionpremium = 0M;
                                if (agreement.ClientAgreementTermExtensions.Count > 0)
                                {
                                    foreach (var termExtension in agreement.ClientAgreementTermExtensions.Where(ae => ae.DateDeleted == null))
                                    {
                                        if (termExtension.Bound && termExtension.ExtentionName == "Professional Indemnity – Costs & Expenses")
                                        {
                                            ceextensionlimit = termExtension.TermLimit;
                                            ceextensionexcess = termExtension.Excess;
                                            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
                                            {
                                                ceextensionpremium += termExtension.PremiumDiffer;
                                            }
                                            else
                                            {
                                                ceextensionpremium += termExtension.Premium;
                                            }
                                        }

                                    }
                                }
                                if (reportNameformal.Contains("lumely"))
                                {
                                    if (reportName == "ML")
                                    {
                                        PIGrossPremium = (term.Premium * 0.125M);
                                        Brokerageperc = (term.BrokerageRate) / 100;
                                        Brokerage = PIGrossPremium * Brokerageperc;
                                        GST = PIGrossPremium * 0.15M;
                                        BrokerageGST = (Brokerage * 0.15M);
                                        PINetPremiumToInsurer = (PIGrossPremium - Brokerage) + (GST - BrokerageGST);

                                    }
                                    else if (reportName == "PI")
                                    {

                                        PIGrossPremium = (term.Premium * 0.25M);
                                        Brokerageperc = (term.BrokerageRate) / 100;
                                        Brokerage = PIGrossPremium * Brokerageperc;
                                        GST = term.Premium * 0.15M;
                                        BrokerageGST = (Brokerage * 0.15M);
                                        PINetPremiumToInsurer = (PIGrossPremium - Brokerage) + (GST - BrokerageGST);

                                    }

                                }
                                else if (reportNameformal.Contains("AIG"))
                                {
                                    if (reportName == "ML")
                                    {
                                        PIGrossPremium = (term.Premium * 0.875M);
                                        Brokerageperc = (term.BrokerageRate) / 100;
                                        Brokerage = PIGrossPremium * Brokerageperc;
                                        GST = PIGrossPremium * 0.15M;
                                        BrokerageGST = (Brokerage * 0.15M);
                                        PINetPremiumToInsurer = (PIGrossPremium - Brokerage) + (GST - BrokerageGST);

                                    }
                                    else if (reportName == "PI")
                                    {
                                        PIGrossPremium = (term.Premium * 0.75M);
                                        Brokerageperc = (term.BrokerageRate) / 100;
                                        Brokerage = PIGrossPremium * Brokerageperc;
                                        GST = PIGrossPremium * 0.15M;
                                        BrokerageGST = (Brokerage * 0.15M);
                                        PINetPremiumToInsurer = (PIGrossPremium - Brokerage) + (GST - BrokerageGST);

                                    }
                                    else if (reportName == "CL")
                                    {
                                        PIGrossPremium = term.Premium;
                                        Brokerageperc = (term.BrokerageRate) / 100;
                                        Brokerage = PIGrossPremium * Brokerageperc;
                                        GST = PIGrossPremium * 0.15M;
                                        BrokerageGST = (Brokerage * 0.15M);
                                        PINetPremiumToInsurer = (PIGrossPremium - Brokerage) + (GST - BrokerageGST);

                                    }
                                }


                                PIFullPremiumTotal += (term.Premium + ceextensionpremium);
                                PIGrossPremiumTotal += PIGrossPremium;
                                PINetPremiumToInsurerTotal += PINetPremiumToInsurer;
                                newRow[5] = Math.Round(PIGrossPremium, 2);
                                newRow[6] = Brokerageperc;
                                newRow[7] = Brokerage;
                                newRow[8] = Math.Round(GST, 2);
                                newRow[9] = Math.Round(BrokerageGST, 2);
                                newRow[10] = Math.Round(PINetPremiumToInsurer, 2);
                                dt.Rows.Add(newRow);

                                break;
                            }
                        }
                    }
                    if (tempListReport.Count > 0)
                        ListReportSet.Add(tempListReport);

                }
                catch (Exception ex)
                { }


            }

            if (dt.Columns.Contains("Id"))
                dt.Columns.Remove("Id");
            string ContentType = "Application/msexcel";
            string fileName = reportName + "Report.xlsx";
            MemoryStream stream = new MemoryStream();

            dt.TableName = "MyDt";
            try
            {
                XLWorkbook workbook = new XLWorkbook();
                workbook.Worksheets.Add(dt, "WorksheetName");


                workbook.SaveAs(stream);

                stream.Position = 0;



            }
            catch (Exception ex)
            {

            }

            return ControllerBase.File(stream, ContentType, fileName);




        }



        public async Task<List<List<string>>> GetPremiumLimitReportSet(Guid programmeId, string reportName)
        {
            List<List<string>> ListReportSet = new List<List<string>>();
            List<String> ListReport = new List<String>();
            try
            {
                Programme programme = await _programmeService.GetProgrammeById(programmeId);
                ListReport.Add("Insured");
                ListReport.Add("Is Change");
                ListReport.Add("Reference Id");
                ListReport.Add("Email");
                ListReport.Add("Agreement Status");
                //ListReport.Add("Advisor Names");
                ListReport.Add("Limit");
                ListReport.Add("Excess");
                ListReport.Add("Premium");
                ListReport.Add("Premium Difference");
                //List<ClientProgramme> lClientProgrammes = programme.ClientProgrammes.ToList();
                //var clients = programme.ClientProgrammes;
                List<ClientProgramme> clientProgrammes = await _programmeService.GetClientProgrammesForProgramme(programmeId);

                ListReportSet.Add(ListReport);

                foreach (var cp in programme.ClientProgrammes.Where(o => o.InformationSheet.DateDeleted == null && o.InformationSheet.Status == "Submitted"))
                {
                    try
                    {
                        Guid clientInformationSheetID = Guid.NewGuid();
                        if (cp.BaseProgramme.Id == programme.Id)
                        {
                            clientInformationSheetID = cp.InformationSheet.Id;

                        }
                        List<String> reportlistcount = await CreatePremiumLimitReport(cp, clientInformationSheetID, true, false, reportName);
                        if (reportlistcount.Count > 0)
                            ListReportSet.Add(await CreatePremiumLimitReport(cp, clientInformationSheetID, true, false, reportName));

                    }
                    catch (Exception ex)
                    { }
                }
            }
            catch (Exception ex)
            { }

            return ListReportSet;
        }

        public async Task<List<string>> CreatePremiumLimitReport(ClientProgramme cp, Guid clientInformationSheetID, Boolean IsprincipalAdvisor, Boolean isSubClient, string reportName)
        {

            List<String> ListReport = new List<String>();




            Organisation organisation = cp.InformationSheet.Owner;
            ////adding collumns to ListReport

            //ListReport.Add(cp.InformationSheet.Owner.Name);
            //ListReport.Add((cp.InformationSheet.IsChange).ToString());
            //ListReport.Add(cp.InformationSheet.ReferenceId);

            //ListReport.Add(organisation.Email);
            User user = await _userService.GetApplicationUserByEmail(organisation.Email);

            if (cp.Agreements.Count > 0)
            {
                foreach (ClientAgreement agreement in cp.Agreements)
                {
                    var term = agreement.ClientAgreementTerms.FirstOrDefault(ter => ter.SubTermType == reportName && ter.Bound == true);
                    if (term != null)
                    {
                        ListReport = new List<String>();
                        ListReport.Add(cp.InformationSheet.Owner.Name);
                        ListReport.Add((cp.InformationSheet.IsChange).ToString());
                        ListReport.Add(cp.InformationSheet.ReferenceId);

                        ListReport.Add(organisation.Email);
                        ListReport.Add(agreement.Status);
                        ListReport.Add(term.TermLimit.ToString());
                        ListReport.Add(term.Excess.ToString("N0"));
                        ListReport.Add(term.Premium.ToString("N2"));
                        ListReport.Add(term.PremiumDiffer.ToString("N2"));

                        break;
                    }

                }

            }
            return ListReport;

        }

    }
}

