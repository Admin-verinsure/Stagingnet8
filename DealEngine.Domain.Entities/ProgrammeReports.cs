using System;
using System.Collections.Generic;
using System.Text;
using DealEngine.Domain.Entities.Abstracts;

namespace DealEngine.Domain.Entities
{
    public class ProgrammeReports : EntityBase
    {
        public ProgrammeReports() : base(null)
        {

        }

        public ProgrammeReports(User createdBy) : base(createdBy)
        {
            
        }
        public virtual string progreportsName { get; set; }
        public virtual string progreportsValue { get; set; }
        public virtual string progreportsDescription { get; set; }
        public virtual IList<Programme> programme { get; set; }
        public virtual Boolean IsBoundField { get; set; }
        public virtual string ReportType { get; set; }


    }

}
