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

namespace DealEngine.WebUI.Helpers
{

    public class ReportSchedular : BaseController, IJob
    {

        IUserService _userService;
        IEmailService _emailService;

      

        private readonly IReportBuilderService _ReportBuilderService;

        private readonly ILogger<ReportSchedular> _logger;
        private readonly  IProgrammeService _programmeService;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        //Guid progid = Guid.Parse("62aea93b-8f7e-4554-b037-bb6726bc3c2d");
         Guid progid = Guid.Parse("bbcd7ef3-64c3-4759-9144-49cac816f425");

        //public ReportSchedular(IReportBuilderService reportBuilderService)
        //{
        //    _ReportBuilderService = reportBuilderService;
        //}
        public ReportSchedular(ILogger<ReportSchedular> logger, IReportBuilderService ReportBuilderService, IUserService userRepository, IProgrammeService programmeService, IEmailService emailService,
        IServiceScopeFactory serviceScopeFactory) : base(userRepository)
        {
            this._logger = logger;
            _programmeService = programmeService;
            _ReportBuilderService = ReportBuilderService;
            _userService = userRepository;
            _emailService = emailService;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            Debug.WriteLine("reports Available #############");
            //String user = _httpContextAccessor.HttpContext.User.Identity.Name;
            // _logger.LogInformation($"Notification Job: Notify User at {DateTime.Now} and Jobtype: {context.JobDetail.JobType}");
            // await _ReportBuilderService.GetReportView(progid, "false");
            List<List<string>> Lreportset = new List<List<string>>();
            DataTable table = new DataTable();
            
            Programme programme = await _programmeService.GetProgrammeById(progid);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var myScopedService = scope.ServiceProvider.GetService<IReportBuilderService>();
                Lreportset = await myScopedService.GetReportView(progid, "false");



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
               
                    XLWorkbook workbook = new XLWorkbook();
                    workbook.Worksheets.Add(table, "WorksheetName");
                    // wb.SaveAs(@"C:\\Users\\Public\\DataImport\\Students1.xlsx");

                    //Defining the ContentType for excel file.
                    string ContentType = "Application/msexcel";

            //Define the file name.
            string file = " / tmp / Report.xlsx";
           // string fileName = "Report.xlsx";

                    //Creating stream object.
                    MemoryStream stream = new MemoryStream();

                    //Saving the workbook to stream in XLSX format
                    workbook.SaveAs(file);

                    stream.Position = 0;

            //EmailTemplate emailTemplate = null;
            await _emailService.SendReportsViaEmail("Ashu@techcertain.com", file);

            //if (emailTemplate != null)
            //{
               
            //        await _emailService.SendReportsViaEmail("Ashu@techcertain.com", file);
              

            //}
            //return File(stream, ContentType, fileName);

            // Domain.Entities.File file = File(stream, ContentType, fileName).;

            // return File(stream, ContentType, fileName);



            await Task.CompletedTask;


        }


        public async Task<User> getuser()
        {
            User user = await CurrentUser();
            return user;
        }

        //public async Task Execute(IJobExecutionContext context)
        //{

        //    try
        //    {
        //        Debug.WriteLine("reports Available #############");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }
        //    await Task.CompletedTask;
        //}


        //public class DemoJobFactory : IJobFactory
        //{
        //    private readonly IServiceProvider _serviceProvider;

        //    public DemoJobFactory(IServiceProvider serviceProvider)
        //    {
        //        _serviceProvider = serviceProvider;
        //    }

        //    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        //    {
        //        return _serviceProvider.GetService<CheckAvailabilityTask>();
        //    }

        //    public void ReturnJob(IJob job)
        //    {
        //        var disposable = job as IDisposable;
        //        disposable?.Dispose();
        //    }
        //}
    }
}
