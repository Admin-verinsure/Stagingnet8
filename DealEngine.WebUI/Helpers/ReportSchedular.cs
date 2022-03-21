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
       IEmailService _emailService;

        private readonly IReportBuilderService _ReportBuilderService;

        private readonly ILogger<ReportSchedular> _logger;
        private readonly  IProgrammeService _programmeService;
       // private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ISessionFactory _sessionFactory;
        private NHibernate.ISession _session;
        //Guid progid = Guid.Parse("62aea93b-8f7e-4554-b037-bb6726bc3c2d");
       // Guid progid = Guid.Parse("bbcd7ef3-64c3-4759-9144-49cac816f425");

        //public ReportSchedular(IReportBuilderService reportBuilderService)
        //{
        //    _ReportBuilderService = reportBuilderService;
        //}public ReportSchedular(ILogger<ReportSchedular> logger, IReportBuilderService ReportBuilderService, IUserService userRepository, IProgrammeService programmeService, IEmailService emailService,
        //IServiceScopeFactory serviceScopeFactory) : base(userRepository)
        public ReportSchedular(ILogger<ReportSchedular> logger, IReportBuilderService ReportBuilderService, IUserService userRepository, IProgrammeService programmeService, IEmailService emailService,
             NHibernate.ISession session, ISessionFactory sessionFactory) : base(userRepository)
        {
            this._logger = logger;
            _programmeService = programmeService;
            _ReportBuilderService = ReportBuilderService;
            _userService = userRepository;
            _emailService = emailService;
           // _serviceScopeFactory = serviceScopeFactory;
            _sessionFactory = sessionFactory;
            _session = session;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var reportname = context.JobDetail.Description;
            string dbfunctionname = reportname.Substring(0, reportname.IndexOf(","));
            string progid = reportname.Substring(reportname.IndexOf(",")+1);
            //  var progid= context.JobDetail.Key
           // string programme = _programmeService
            IQuery query;
        
            try
            {
                using (var tx = _session.BeginTransaction())
                {
                    //query = _session.CreateSQLQuery("CALL public."+ reportname)
                    query = _session.CreateSQLQuery("SELECT public."+ dbfunctionname+"('''"+ progid + "''')");

                    //query.SetString("progid","a808421f-59ff-436c-9250-ae49008bdc4a");
                     query.ExecuteUpdate();


                    /// here filename should be out parameters fr
                    string fileName = dbfunctionname+DateTime.Now+".csv";
                    // string filepath = "/ tmp /";
                    //string filepath = "C:\\inetpub\\wwwroot\\dealengine\\DealEngine.WebUI\\cv";
                    string filepath = "/home/ubuntu/projects/dealengine/publish/wwwroot/Documents/Reports";
                     //// here file should target to place we want to save file on server
                     string file = filepath + fileName;

                    //Creating stream object.
                    MemoryStream stream = new MemoryStream();

                    //Saving the workbook to stream in XLSX format
                    //workbook.SaveAs(file);
                  //  workbook.Worksheets.Add(table, "WorksheetName");
                    //         // wb.SaveAs(@"C:\\Users\\Public\\DataImport\\Students1.xlsx");

                    //         //Defining the ContentType for excel file.
                   string ContentType = "text/csv";

                    // //Define the file name.
                    // string file = " / tmp / Report.xlsx";
                    ////
                    stream.Position = 0;

                    EmailTemplate emailTemplate = null;
                    await _emailService.SendReportsViaEmail("staff@techcertain.com", file);

                    if (emailTemplate != null)
                    {

                        await _emailService.SendReportsViaEmail("staff@techcertain.com", file);


                    }

                    //return File(stream, ContentType, fileName);

                    //Domain.Entities.File file = File(stream, ContentType, fileName);

                    //return File(stream, ContentType, fileName);




                }
            }
            catch (Exception ex)
            {

            }
            await Task.CompletedTask;


        }


    }
}
