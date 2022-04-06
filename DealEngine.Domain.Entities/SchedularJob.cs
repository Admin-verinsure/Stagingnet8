using System;
using System.Collections.Generic;
using System.Text;
using DealEngine.Domain.Entities.Abstracts;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DealEngine.Domain.Entities
{
    public class SchedularJob : EntityBase
    {
        public SchedularJob() : base(null) { }

        public SchedularJob(User createdBy) : base(createdBy) { }


        public SchedularJob( string JobName, string ProgrammeId, string JobDate, string JobTime, string JobFunctionNumber, 
                             string ScheduleFrequency, string JobStatus, Type jobType, string ReportName, String EmailIds,
                             String BoundDateFrom,String BoundDateTo,String ReportType, User createdBy = null)
        : base(createdBy)
        {
            this.JobName = JobName;
            this.ProgrammeId = ProgrammeId;
            this.JobDate = JobDate;
            this.JobTime = JobTime;
            this.JobFunctionName = JobFunctionNumber;
            this.ScheduleFrequency = ScheduleFrequency;
            this.JobStatus = JobStatus;
            this.JobType = jobType;
            this.ReportName = ReportName;
            this.EmailIds = EmailIds;
            this.BoundDateFrom = BoundDateFrom;
            this.BoundDateTo = BoundDateTo;
            this.ReportType = ReportType;
        }


        public virtual string JobName { get; set; }
        public virtual string ProgrammeId { get; set; }
        public virtual string JobDate { get; set; }
        public virtual string JobTime { get; set; }
        public virtual string JobFunctionName { get; set; }
        public virtual string ScheduleFrequency { get; set; }
        public virtual string JobStatus { get; set; }
        public virtual Type JobType { get;  }
        public virtual string ReportName { get; set; }
        public virtual string EmailIds { get; set; }
        public virtual string BoundDateFrom { get; set; }
        public virtual string BoundDateTo { get; set; }
        public virtual string ReportType { get; set; }


    }

}
