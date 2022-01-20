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

    public class ReportSchedular : IJob
    {

        //IUserService _userService;
        ////private readonly IServiceProvider _provider;
        //public ReportSchedular(IReportBuilderService ReportBuilderService)
        //{
        //    _ReportBuilderService = ReportBuilderService;
        //}
        //    {
        //        _ReportBuilderService = reportBuilderService;
        //    }
        //public ReportSchedular(IServiceProvider provider)
        //{
        //    _provider = provider;
        //}
        //public  Task Execute(IJobExecutionContext jobcontext)
        //{
        //   // Guid progid = Guid.Parse("62aea93b-8f7e-4554-b037-bb6726bc3c2d");
        //   // List<List<string>> Lreportset = new List<List<string>>();
        //    try
        //    {
        //        //using (var scope = _provider.CreateScope())
        //        //{
        //        //    var jobType = jobcontext.JobDetail.JobType;
        //        //    var job = scope.ServiceProvider.GetRequiredService(jobType) as IJob;

        //        //    var _ReportBuilderService = scope.ServiceProvider.GetService<ReportBuilderService>();
        //        //    await job.Execute(jobcontext);
        //        //    // fetch customers, send email, update DB
        //        //}
        //        //using (var scope = _ReportBuilderService.CreateScope())
        //        //{
        //        //    // Resolve the Scoped service
        //        //    var service = scope.ServiceProvider.GetService<IScopedService>();
        //        //    _logger.LogInformation("Hello world!");
        //        //}
        //        Debug.WriteLine("reports Available #############");
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex);
        //    }

        //    //await ReportBuilderService.GetReportView(progid, "false");
        //   return await Task.CompletedTask;
        //}


        //}

        private readonly IReportBuilderService _ReportBuilderService;

        private readonly ILogger<ReportSchedular> _logger;
        //private readonly  IProgrammeService _programmeService;
        Guid progid = Guid.Parse("62aea93b-8f7e-4554-b037-bb6726bc3c2d");
        public ReportSchedular(IReportBuilderService reportBuilderService)
        {
            _ReportBuilderService = reportBuilderService;
        }
        //public ReportSchedular(ILogger<ReportSchedular> logger)
        //{
        //    this._logger = logger;
        //}
            public async Task Execute(IJobExecutionContext context)
        {
            Debug.WriteLine("reports Available #############");
            // _logger.LogInformation($"Notification Job: Notify User at {DateTime.Now} and Jobtype: {context.JobDetail.JobType}");
           // await _ReportBuilderService.GetReportView(progid, "false");
            await Task.CompletedTask;
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
