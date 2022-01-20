using DealEngine.WebUI.Models;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DealEngine.WebUI.Helpers
{
    public class JobSchedular : IHostedService
    {
        public IScheduler Scheduler { get; set; }
        private readonly IJobFactory jobFactory;
        private readonly JobMetadata jobMetadata;
        private readonly ISchedulerFactory schedulerFactory;
        private readonly IEnumerable<JobMetadata> _jobMetadatas;

        public JobSchedular(ISchedulerFactory schedulerFactory, JobMetadata jobMetadata, IJobFactory jobFactory, IEnumerable<JobMetadata> JobMetadatas)
        {
            this.jobFactory = jobFactory;
            this.schedulerFactory = schedulerFactory;
            this.jobMetadata = jobMetadata;
            this._jobMetadatas = JobMetadatas;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Scheduler = await schedulerFactory.GetScheduler(cancellationToken);
            Scheduler.JobFactory = jobFactory;
            //create job
            //working IJobDetail jobDetails = CreateJob(jobMetadata);
            //create trigger
            //working ITrigger trigger = CreateTrigger(jobMetadata);
            //schedule job
            ///working await Scheduler.ScheduleJob(jobDetails, trigger);
            //start schedular

            foreach (var jobSchedule in _jobMetadatas)
            {
                var job = CreateJob(jobSchedule);
                var trigger = CreateTrigger(jobSchedule);
                await Scheduler.ScheduleJob(job, trigger, cancellationToken);
            }
            await Scheduler.Start(cancellationToken);
        }

        private ITrigger CreateTrigger(JobMetadata jobMetadata)
        {
            return TriggerBuilder.Create()
                .WithIdentity(jobMetadata.JobId.ToString())
                .WithCronSchedule(jobMetadata.CronExpression)
                .WithDescription(jobMetadata.JobName)
                .Build();
        }

        private IJobDetail CreateJob(JobMetadata jobMetadata)
        {
            return JobBuilder.Create(jobMetadata.JobType)
                .WithIdentity(jobMetadata.JobId.ToString())
                .WithDescription(jobMetadata.JobName)
                .Build();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Scheduler.Shutdown();
        }
    }
}
