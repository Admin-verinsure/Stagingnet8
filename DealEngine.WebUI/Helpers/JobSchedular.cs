//using DealEngine.Domain.Entities;
//using DealEngine.Services.Interfaces;
//using DealEngine.WebUI.Models;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Quartz;
//using Quartz.Spi;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;

//namespace DealEngine.WebUI.Helpers
//{
//    public class JobSchedular : IHostedService
//    {
//        public IScheduler Scheduler { get; set; }
//        private readonly IJobFactory jobFactory;
//        // private readonly JobMetadata jobMetadata;
//        private readonly ISchedulerFactory schedulerFactory;
//        //private readonly IEnumerable<JobMetadata> _jobMetadatas;
//        // private readonly ISchedularJobService _schedularjobService;
//        private readonly IServiceProvider _serviceProvider;
//        public JobSchedular(ISchedulerFactory schedulerFactory, IJobFactory jobFactory, IServiceProvider serviceProvider)
//        {
//            this.jobFactory = jobFactory;
//            this.schedulerFactory = schedulerFactory;
//            //this.jobMetadata = jobMetadata;
//            // this._jobMetadatas = JobMetadatas;
//            // this._schedularjobService = schedularjobService;
//            _serviceProvider = serviceProvider;
//        }
//        public async Task StartAsync(CancellationToken cancellationToken)
//        {
//            Scheduler = await schedulerFactory.GetScheduler(cancellationToken);
//            Scheduler.JobFactory = jobFactory;
//            //create job
//            //working IJobDetail jobDetails = CreateJob(jobMetadata);
//            //create trigger
//            //working ITrigger trigger = CreateTrigger(jobMetadata);
//            //schedule job
//            ///working await Scheduler.ScheduleJob(jobDetails, trigger);
//            //start schedular

//            //foreach (var jobSchedule in _jobMetadatas)
//            //{
//            using (var scope = _serviceProvider.CreateScope())
//            {
//                ISchedularJobService _schedularjobService = scope.ServiceProvider.GetService<ISchedularJobService>();   //  (ISchedularJobService)scope.ServiceProvider;
//                List<SchedularJob> LSchedularJob = _schedularjobService.GetJob();


//                foreach (var job in LSchedularJob)
//                {
//                    var scheduledjob = CreateJob(job);

//                    var trigger = CreateTrigger(job);
//                    await Scheduler.ScheduleJob(scheduledjob, trigger, cancellationToken);

//                }
//            }

//            // }
//            await Scheduler.Start(cancellationToken);
//        }

//        private static ITrigger CreateTrigger(SchedularJob schedularjob)
//        {
//            // ITrigger trigger = TriggerBuilder.Create()
//            //        .WithIdentity($"Check Availability - {DateTime.Now}")
//            //        .StartAt(new DateTimeOffset(DateTime.Now.AddSeconds(10)))
//            //        .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).WithRepeatCount(5))
//            //        .WithPriority(1)
//            //        .Build();

//            //List<SchedularJob> LSchedularJob =    _schedularjobService.GetJob();
//            //// foreach(var job in LSchedularJob)
//            //{

//           DateTime datetime = Convert.ToDateTime(schedularjob.JobDate + " " + "00:14");
//            // DateTime datetime2 = DateTime.Parse("16/02/2022 04:00");

//            //DateTime datetime = DateTime.Parse("02/16/2022 04:00", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"));
//            // DateTime datetime = DateTime.Now;


//            return TriggerBuilder.Create()
//            .WithIdentity(schedularjob.Id.ToString())
//           .StartAt(datetime)
//           //.StartAt(new DateTimeOffset(DateTime.Now.AddSeconds(10)))
//           //.WithSimpleSchedule(x => x.WithIntervalInSeconds(5).WithRepeatCount(1))
//           .WithDescription(schedularjob.ReportName)
//           .Build();
//            // }
//            //return ok;

//            //.WithCronSchedule(jobMetadata.CronExpression)

//        }

//        private static IJobDetail CreateJob(SchedularJob schedularjob)
//        {
//            return JobBuilder.Create<ReportSchedular>()
//                .WithIdentity(schedularjob.Id.ToString())
//                .WithDescription(schedularjob.ReportName)
//                .Build();
//        }

//        public async Task StopAsync(CancellationToken cancellationToken)
//        {
//            await Scheduler.Shutdown();
//        }
//    }

//}
