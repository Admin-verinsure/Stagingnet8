using DealEngine.Services.Interfaces;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Domain.Entities;
using System.Threading.Tasks;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl
{
    public class SchedularJobService : ISchedularJobService
    {
        IMapperSession<SchedularJob> _schedularjobRepository;

        public SchedularJobService(IMapperSession<SchedularJob> schedularjobRepository)
        {
            _schedularjobRepository = schedularjobRepository;
        }

        public async Task<SchedularJob> GetJobById(Guid jobId)
        {
            return await _schedularjobRepository.GetByIdAsync(jobId);
        }
        public async Task UpdateJob(SchedularJob job)
        {
            await _schedularjobRepository.UpdateAsync(job);
        }

        public async Task AddJob(SchedularJob schedularJob)
        {
            await _schedularjobRepository.AddAsync(schedularJob);
        }
        public List<SchedularJob> GetJob()

        { //var joblist = new List<SchedularJob>();

           var  joblist = _schedularjobRepository.FindAll().Where(job => job.JobStatus == "Active").ToList();
            return joblist;
        }
    }
}

