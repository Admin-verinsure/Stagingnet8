using DealEngine.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity;

namespace DealEngine.WebUI.Helpers
{
    //internal sealed class IntegrationJobFactory : IJobFactory
    //{
    //    private readonly IUnityContainer _container;

    //    public IntegrationJobFactory(IUnityContainer container)
    //    {
    //        _container = container;
    //    }

    //    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    //    {
    //        var jobDetail = bundle.JobDetail;
    //        //var _scheduler = schedulerFactory.GetScheduler();
    //        //var _scheduler.JobFactory = new IntegrationJobFactory(container);
    //        var job = (IJob)_container.Resolve(jobDetail.JobType);
    //        return job;
    //    }

    //    public void ReturnJob(IJob job)
    //    {
    //    }
    //}
    public class JobFactory: IJobFactory
    {

        private readonly IServiceProvider service;
        //private readonly IReportBuilderService _ReportBuilderService;

        public JobFactory(IServiceProvider serviceprovider)
    {
        service = serviceprovider;
    }

    public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
    {

        IJobDetail jobDetail = bundle.JobDetail;
            // Type jobType = jobDetail.JobType;
            // return service.GetRequiredService<QuartzJobRunner>();

            return (IJob)service.GetService(jobDetail.JobType);
        //return service.GetService(jobType) as IJob;
        //return service.GetService(ReportSchedular) as IJob;
    }

    public void ReturnJob(IJob job)
    {
        throw new NotImplementedException();
    }
}
}
