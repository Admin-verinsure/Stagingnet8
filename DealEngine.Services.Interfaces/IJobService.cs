using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface IJobService
    {
        public Task<Job> GetJobById(Guid jobId);
        public Task UpdateJob(Job job);
    }
}
