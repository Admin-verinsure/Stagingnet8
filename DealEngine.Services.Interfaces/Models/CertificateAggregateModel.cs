using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealEngine.Services.Interfaces.Models
{
    public class CertificateAggregateModel
    {
        public string CertificateTitle { get; set; }

        public string PolicyNumber { get; set; }
        public string PolicyType { get; set; }

        public string NamedParties { get; set; }
        public string BusinessDescription { get; set; }

        public string Insurer { get; set; }

        public string InceptionDate { get; set; }
        public string ExpiryDate { get; set; }

        public string InterestInsured { get; set; }
        public string LimitsOfLiability { get; set; }
        public string Deductible { get; set; }

        public string JurisdictionalLimit { get; set; }
        public string GeographicalLimit { get; set; }

        public string Endorsements { get; set; }

        public string IssueDate { get; set; }

        public string LogoUrl { get; set; }
    }
}
