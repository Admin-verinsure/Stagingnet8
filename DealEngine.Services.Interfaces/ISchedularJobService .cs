using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface ISchedularJobService
    {
        public Task<SchedularJob> GetJobById(Guid jobId);
        public Task UpdateJob(SchedularJob job);
        public Task AddJob(SchedularJob schedularJob);
        public  List<SchedularJob> GetJob();

    }
}
