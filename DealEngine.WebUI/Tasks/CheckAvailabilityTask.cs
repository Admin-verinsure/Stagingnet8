
//using AutoMapper;
//using ClosedXML.Excel;
//using DealEngine.Domain.Entities;
//using DealEngine.Infrastructure.FluentNHibernate;
//using DealEngine.Services.Interfaces;
//using DealEngine.WebUI.Models;
//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Identity;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Logging;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using IdentityUser = NHibernate.AspNetCore.Identity.IdentityUser;
//using SystemDocument = DealEngine.Domain.Entities.Document;
//using Document = DealEngine.Domain.Entities.Document;
//using NReco.PdfGenerator;
//using Quartz;
//using System.Diagnostics;
//using DealEngine.WebUI.Controllers;
//using Quartz.Spi;
//using DealEngine.Services.Impl;

//namespace DealEngine.WebUI.Tasks
//{

//    public class CheckAvailabilityTask : IJob
//    {

//       // private readonly ReportBuilderService ReportBuilderService;

//        //public CheckAvailabilityTask(ReportBuilderService reportBuilderService)
//        //{
//        //    this.ReportBuilderService = reportBuilderService;
//        //}

//        public  async Task Execute(IJobExecutionContext jobcontext)
//        {
//            Guid progid = Guid.Parse("62aea93b-8f7e-4554-b037-bb6726bc3c2d");
//            List<List<string>> Lreportset = new List<List<string>>();
//            try
//            {
//                Debug.WriteLine("reports Available");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex);
//            }

//            // await ReportBuilderService.GetReportView(progid,"false");
//        }
       
//    }


//    //public class DemoJobFactory : IJobFactory
//    //{
//    //    private readonly IServiceProvider _serviceProvider;

//    //    public DemoJobFactory(IServiceProvider serviceProvider)
//    //    {
//    //        _serviceProvider = serviceProvider;
//    //    }

//    //    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
//    //    {
//    //        return _serviceProvider.GetService<CheckAvailabilityTask>();
//    //    }

//    //    public void ReturnJob(IJob job)
//    //    {
//    //        var disposable = job as IDisposable;
//    //        disposable?.Dispose();
//    //    }
//    //}
//}
