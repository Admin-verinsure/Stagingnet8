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

namespace DealEngine.Services.Impl
{
    public class ReportBuilderService :IReportBuilderService
    {
        IProgrammeService _programmeService;
        IUserService _userService;

        public ReportBuilderService(IProgrammeService programmeServicey, IUserService userservice)
        {
            _programmeService = programmeServicey;
            _userService = userservice;
        }

        public async Task<List<List<string>>> GetReportView(Guid ProgrammeId, string IsReport)
        {

            Debug.WriteLine("in report view Available");

            List<PIReport> reportset = new List<PIReport>();
            DataTable table = new DataTable();
            List<List<string>> Lreportset = new List<List<string>>();
            
               
                try
                {
                    Programme programme = await _programmeService.GetProgrammeById(ProgrammeId);
                    string queryselect = "PI";
                    
                    if (programme.NamedPartyUnitName == "NZFSG Programme" && queryselect == "FAP")
                    {
                        //ViewBag.Title = "Financial Advice Provider(FAP)";

                      //  Lreportset = await GetNZFGReportSet(ProgrammeId, queryselect);

                    }
                    else if ((programme.NamedPartyUnitName == "Apollo Programme" || programme.NamedPartyUnitName == "TripleA Programme" || programme.NamedPartyUnitName == "Abbott Financial Advisor Liability Programme") && queryselect == "FAP")
                    {
                        //ViewBag.Title = "Financial Advice Provider(FAP)";
//
                       // Lreportset = await GetAAAReportSet(ProgrammeId, queryselect);

                    }
                    else if (queryselect == "RevenueActivity")
                    {//
                       // Lreportset = await GetRevenueReportSet(ProgrammeId, queryselect);
                    }
                    else
                    {
                       // ViewBag.Title = "Bound " + queryselect + " Premium and Limits";

                        Lreportset = await GetPremiumLimitReportSet(ProgrammeId, queryselect);

                    }


                    try
                    {
                        for (int i = 0; i < Lreportset[0].Count; i++)
                        {
                            table.Columns.Add(Lreportset[0][i]);
                        }

                    }
                    catch (Exception ex)
                    {
                        if (table.Columns.Contains("Id"))
                            table.Columns.Remove("Id");
                    }

                    //object[] values = new object[props.Count];
                    object[] values1 = new object[table.Columns.Count];

                    for (int i = 1; i <= Lreportset.Count - 1; i++)
                    {
                        try
                        {

                            var count = 0;
                            for (int j = 0; j < Lreportset[i].Count; j++)
                            {
                                try
                                {
                                    var val = Lreportset[i].ElementAt(j);

                                    if (val != null)
                                    {
                                        values1[count] = val;
                                        count++;
                                    }
                                }
                                catch (Exception ex)
                                {
                                }
                            }
                            table.Rows.Add(values1);

                        }
                        catch (Exception ex)
                        {
                        }
                    }


                        table.TableName = "MyDt";
                        try
                        {
                            XLWorkbook workbook = new XLWorkbook();
                            workbook.Worksheets.Add(table, "WorksheetName");
                            // wb.SaveAs(@"C:\\Users\\Public\\DataImport\\Students1.xlsx");

                            //Defining the ContentType for excel file.
                            string ContentType = "Application/msexcel";

                            //Define the file name.
                            string fileName = queryselect + "Report.xlsx";

                            //Creating stream object.
                            MemoryStream stream = new MemoryStream();

                            //Saving the workbook to stream in XLSX format
                            workbook.SaveAs(stream);

                            stream.Position = 0;

                          //  return File(stream, ContentType, fileName);

                        }
                        catch (Exception ex)
                        {

                        }

                        //return View(table);

                  

                }
                catch (Exception ex)
                {

                    //await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                    //return RedirectToAction("Error500", "Error");
                }

            

            return Lreportset;

        }



        public async Task<List<List<string>>> GetPremiumLimitReportSet(Guid programmeId, string reportName)
        {
            Programme programme = await _programmeService.GetProgrammeById(programmeId);
            List<List<string>> ListReportSet = new List<List<string>>();
            List<String> ListReport = new List<String>();

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



            ListReportSet.Add(ListReport);

            foreach (ClientProgramme cp in programme.ClientProgrammes.Where(o => o.InformationSheet.DateDeleted == null && o.InformationSheet.Status == "Bound"))
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

