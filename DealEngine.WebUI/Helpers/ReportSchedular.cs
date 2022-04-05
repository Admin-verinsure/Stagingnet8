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
        private readonly ISessionFactory _sessionFactory;
        private NHibernate.ISession _session;
        ISchedularJobService _schedularjobService;
        //public ReportSchedular(IReportBuilderService reportBuilderService)
        //{
        //    _ReportBuilderService = reportBuilderService;
        //}public ReportSchedular(ILogger<ReportSchedular> logger, IReportBuilderService ReportBuilderService, IUserService userRepository, IProgrammeService programmeService, IEmailService emailService,
        //IServiceScopeFactory serviceScopeFactory) : base(userRepository)
        public ReportSchedular(ILogger<ReportSchedular> logger, IReportBuilderService ReportBuilderService, IUserService userRepository, IProgrammeService programmeService, IEmailService emailService,
             NHibernate.ISession session, ISessionFactory sessionFactory, ISchedularJobService schedularjobService) : base(userRepository)
        {
            this._logger = logger;
            _programmeService = programmeService;
            _ReportBuilderService = ReportBuilderService;
            _userService = userRepository;
            _emailService = emailService;
            _schedularjobService = schedularjobService;
            // _serviceScopeFactory = serviceScopeFactory;
            _sessionFactory = sessionFactory;
            _session = session;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var reportId = context.JobDetail.Description;
            SchedularJob schedularJob = await _schedularjobService.GetJobById(Guid.Parse(reportId));
            IQuery query;
        
            try
            {
                Programme prog = await _programmeService.GetProgrammeById(Guid.Parse(schedularJob.ProgrammeId));
                string fileName = schedularJob.ReportName + ".csv";
                string filepath = prog.Reportspath;
                //string filepath = "C:\\inetpub\\wwwroot\\dealengine\\DealEngine.WebUI\\cv\\";
                string file = filepath + fileName;

                using (var tx = _session.BeginTransaction())
                {
                    if (schedularJob.BoundDateFrom != "")
                    {
                        query = _session.CreateSQLQuery("   SELECT public." + schedularJob.JobFunctionName + "(  '''" + schedularJob.ProgrammeId + "''' ,'''" + schedularJob.BoundDateFrom + "'''   ,'''" + schedularJob.BoundDateTo + "''','''" + file + "'''  )   ");
                    }
                    if (schedularJob.ReportType == "Library")
                    {
                        query = _session.CreateSQLQuery("   SELECT public.LibraryReports"+ "(  '''" + schedularJob.ProgrammeId + "''' ,'''" + schedularJob.ReportName + "''','''" + file + "'''  )   ");
                    }
                    else
                    {
                        query = _session.CreateSQLQuery("   SELECT public." + schedularJob.JobFunctionName + "(  '''" + schedularJob.ProgrammeId + "''' ,'''" + file + "''' )   ");
                    }

                    
                    query.ExecuteUpdate();
                  

                    MemoryStream stream = new MemoryStream();
                   string ContentType = "text/csv";
                    stream.Position = 0;
                    EmailTemplate emailTemplate = null;
                    if(schedularJob.EmailIds != null)
                    {
                        await _emailService.SendReportsViaEmail(schedularJob.EmailIds, file);
                    }
                    else
                    {
                        await _emailService.SendReportsViaEmail("staff@techcertain.com", file);
                    }


                    if (emailTemplate != null)
                    {

                        await _emailService.SendReportsViaEmail(schedularJob.EmailIds, file);

                    }

                }
            }
            catch (Exception ex)
            {

            }
            await Task.CompletedTask;


        }


    }
}
