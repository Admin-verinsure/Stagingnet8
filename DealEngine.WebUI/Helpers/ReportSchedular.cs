using AutoMapper;
using ClosedXML.Excel;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;
using DealEngine.WebUI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IdentityUser = NHibernate.AspNetCore.Identity.IdentityUser;
using SystemDocument = DealEngine.Domain.Entities.Document;
using Document = DealEngine.Domain.Entities.Document;
using NReco.PdfGenerator;
using Quartz;
using System.Diagnostics;
using DealEngine.WebUI.Controllers;
using Quartz.Spi;
using DealEngine.Services.Impl;
using Microsoft.Extensions.DependencyInjection;
using NHibernate;
using System.Collections;
using NHibernate.Mapping;

namespace DealEngine.WebUI.Helpers
{

    public class ReportSchedular : BaseController, IJob
    {

        IUserService _userService;
      //  IEmailService _emailService;

      

        private readonly IReportBuilderService _ReportBuilderService;

        private readonly ILogger<ReportSchedular> _logger;
        private readonly  IProgrammeService _programmeService;
       // private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ISessionFactory _sessionFactory;
        private NHibernate.ISession _session;
        //Guid progid = Guid.Parse("62aea93b-8f7e-4554-b037-bb6726bc3c2d");
        Guid progid = Guid.Parse("bbcd7ef3-64c3-4759-9144-49cac816f425");

        //public ReportSchedular(IReportBuilderService reportBuilderService)
        //{
        //    _ReportBuilderService = reportBuilderService;
        //}public ReportSchedular(ILogger<ReportSchedular> logger, IReportBuilderService ReportBuilderService, IUserService userRepository, IProgrammeService programmeService, IEmailService emailService,
        //IServiceScopeFactory serviceScopeFactory) : base(userRepository)
        public ReportSchedular(ILogger<ReportSchedular> logger, IReportBuilderService ReportBuilderService, IUserService userRepository, IProgrammeService programmeService,
             NHibernate.ISession session, ISessionFactory sessionFactory) : base(userRepository)
        {
            this._logger = logger;
            _programmeService = programmeService;
            _ReportBuilderService = ReportBuilderService;
            _userService = userRepository;
           // _emailService = emailService;
           // _serviceScopeFactory = serviceScopeFactory;
            _sessionFactory = sessionFactory;
            _session = session;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            Debug.WriteLine("reports Available #############");
            //EntityManager em 
            //String user = _httpContextAccessor.HttpContext.User.Identity.Name;
            // _logger.LogInformation($"Notification Job: Notify User at {DateTime.Now} and Jobtype: {context.JobDetail.JobType}");
            // await _ReportBuilderService.GetReportView(progid, "false");
          //  List<List<string>> Lreportset = new List<List<string>>();
            DataTable table = new DataTable();
             object[] values1 = new object[table.Columns.Count];

            //Programme programme = await _programmeService.GetProgrammeById(progid);

            //using (var scope = _serviceScopeFactory.CreateScope())
            //{
            //    var myScopedService = scope.ServiceProvider.GetService<IReportBuilderService>();
            //    Lreportset = await myScopedService.GetReportView(progid, "false");

            //using (var command = Database.GetDbConnection().CreateCommand())
            //{
            //    command.CommandText = "sp_AverageTemperaturesReport";
            //    command.CommandType = CommandType.StoredProcedure;

            //    using (var reader = await command.ExecuteReaderAsync())
            //    {
            //        var table = new DataTable();
            //        table.Load(reader);

            //        return table;
            //    }
            //}



            if (!_session.IsOpen)
            {
                //StoredProcedureQuery query
                _session = _sessionFactory.OpenSession();
            }


            List<Object> objects = new List<Object>();
            IQuery query;
            //try{
            //    using (var tx = _session.BeginTransaction())
            //    {
            //        query = _session.CreateSQLQuery("SELECT * FROM public.\"SchedularJob\" as list");
            //        var result = query.List();
            //    }
            //}catch (Exception ex)
            //{

            //}
            try
            {
                //string table = "\"GetUsernameOrg2\"()";
                using (var tx = _session.BeginTransaction())
                {
                    //var sql = String.Format("SELECT  {0}", table);
                    query = _session.CreateSQLQuery("CALL public.proc_GetPIDataClass1277()");
                    //var result = query.List();
                    IList results = query.List();
                    // var datatable = new DataTable();
                    // datatable.Load((IDataReader)query.List());

                    //SqlDataAdapter da1 = new SqlDataAdapter(query);
                    //var query = _session.CreateQuery("FROM public.'Programme'");
                    //table.TableName = "MyDt";
                    //XLWorkbook workbook = new XLWorkbook();
                    //string ContentType = "Application/msexcel";
                    //string file = "cv//Report111222.xlsx";
                    //MemoryStream stream = new MemoryStream();

                    //string cellName;
                    //List<String> ListReport = new List<String>();

                    //table.Columns.Add("Status");
                    //table.Columns.Add("Reference Number");
                    //table.Columns.Add("Is Change");
                    //table.Columns.Add("Insured Name");
                    //table.Columns.Add("Principal Advisor");
                    //table.Columns.Add("Policy Name");
                    //table.Columns.Add("Limit");
                    //table.Columns.Add("Excess");
                    //table.Columns.Add("Premium");
                    //table.Columns.Add("Premium Difference");
                    //table.Columns.Add("Bind Date (UTC)");
                    //table.Columns.Add("Street");
                    //table.Columns.Add("Suburb");
                    //table.Columns.Add("Postcode");
                    //table.Columns.Add("City");
                    //table.Columns.Add("Location");

                    //for (int i = 1; i <= results.Count - 1; i++)
                    //{
                    //    foreach (var item in (IEnumerable)results[i])
                    //    {
                    //        if (item != null)
                    //        {
                    //            table.Columns.Add(item.ToString());

                    //        }
                    //        else
                    //        {
                    //            table.Columns.Add("");

                    //        }
                    //        //row[0] = i;
                    //        //row[counter] = item.ToString();
                    //        //counter++;

                    //    }
                    //    DataRow dtRow;
                    //    dtRow = table.NewRow();

                    //    table.Rows.Add(dtRow);
                    //    //for (int k = 0; k < results[i].Le; k++)
                    //    //{

                    //    //    row[k] = info[i][k];

                    //    //}
                    //}
                    //object[] rowdata = new object[table.Columns.Count];

                    //foreach (var item in results)
                    //{
                    //    //DataRow row = table.NewRow();
                    //    int counter = 0;

                    //    List<String> lrow = new List<String>();
                    //    foreach (var item1 in (IEnumerable)item)
                    //    {
                    //        //var val = item1.ToString();
                    //        if (item1 != null)
                    //        {
                    //            //lrow.Add(item1.ToString());
                    //            rowdata[counter] = item1;

                    //        }
                    //        else
                    //        {
                    //            // lrow.Add("");
                    //            rowdata[counter] = item1;

                    //        }
                    //        counter++;

                    //    }
                    //    table.Rows.Add(rowdata);
                    //    //   table.Columns.Add(item);
                    //}
                    //workbook.Worksheets.Add(table, "WorksheetName");


                    //for (int i = 1; i <= results.Count - 1; i++)
                    //{
                    //    try
                    //    {

                    //        var count = 0;


                    //        for (int j = 0; j < results[i].Count; j++)
                    //        {
                    //            try
                    //            {
                    //                var val = Lreportset[i].ElementAt(j);

                    //                if (val != null)
                    //                {
                    //                    values1[count] = val;
                    //                    count++;
                    //                }
                    //            }
                    //            catch (Exception ex)
                    //            {
                    //            }
                    //        }
                    //        table.Rows.Add(values1);

                    //    }
                    //    catch (Exception ex)
                    //    {
                    //    }
                    //}



                  //  workbook.SaveAs(file);

                    // stream.Position = 0;

                }


            }
            catch (Exception ex)
            {

            }

            //}

            //try
            //{
            //    for (int i = 0; i < results.Count; i++)
            //    {
            //        table.Columns.Add(Lreportset[0][i]);
            //    }

            //}
            //catch (Exception ex)
            //{
            //    if (table.Columns.Contains("Id"))
            //        table.Columns.Remove("Id");
            //}

            // //object[] values = new object[props.Count];
            // object[] values1 = new object[table.Columns.Count];

            // for (int i = 1; i <= Lreportset.Count - 1; i++)
            // {
            //     try
            //     {

            //         var count = 0;
            //         for (int j = 0; j < Lreportset[i].Count; j++)
            //         {
            //             try
            //             {
            //                 var val = Lreportset[i].ElementAt(j);

            //                 if (val != null)
            //                 {
            //                     values1[count] = val;
            //                     count++;
            //                 }
            //             }
            //             catch (Exception ex)
            //             {
            //             }
            //         }
            //         table.Rows.Add(values1);

            //     }
            //     catch (Exception ex)
            //     {
            //     }
            // }


            //     table.TableName = "MyDt";

            //         XLWorkbook workbook = new XLWorkbook();
            //         workbook.Worksheets.Add(table, "WorksheetName");
            //         // wb.SaveAs(@"C:\\Users\\Public\\DataImport\\Students1.xlsx");

            //         //Defining the ContentType for excel file.
            //         string ContentType = "Application/msexcel";

            // //Define the file name.
            // string file = " / tmp / Report.xlsx";
            //// string fileName = "Report.xlsx";

            //         //Creating stream object.
            //         MemoryStream stream = new MemoryStream();

            //         //Saving the workbook to stream in XLSX format
            //         workbook.SaveAs(file);

            //         stream.Position = 0;

            //EmailTemplate emailTemplate = null;
            // await _emailService.SendReportsViaEmail("Ashu@techcertain.com", file);

            //if (emailTemplate != null)
            //{

            //        await _emailService.SendReportsViaEmail("Ashu@techcertain.com", file);


            //}
            //return File(stream, ContentType, fileName);

            // Domain.Entities.File file = File(stream, ContentType, fileName).;

            // return File(stream, ContentType, fileName);



            await Task.CompletedTask;


        }


    }
}
