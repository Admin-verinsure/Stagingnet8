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
    public class JobService : IJobService
    {
        IMapperSession<Job> _jobRepository;

        public JobService(IMapperSession<Job> jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<Job> GetJobById(Guid jobId)
        {
            return await _jobRepository.GetByIdAsync(jobId);
        }
        public async Task UpdateJob(Job job)
        {
            await _jobRepository.UpdateAsync(job);
        }
    }
}

