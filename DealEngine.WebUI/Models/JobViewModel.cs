using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.WebUI.Models
{
    public class JobViewModel : BaseViewModel
    {
        public JobViewModel(ClientInformationSheet ClientInformationSheet)
        {
            Jobs = GetJobs(ClientInformationSheet);
            if (IssueDate == DateTime.MinValue)
            {
                IssueDate = DateTime.Now;
            }
            if (StartDate == DateTime.MinValue)
            {
                StartDate = DateTime.Now;
            }
            if (EndDate == DateTime.MinValue)
            {
                EndDate = DateTime.Now;
            }
            if (CertRequiredBy == DateTime.MinValue)
            {
                CertRequiredBy = DateTime.Now;
            }
        }

        private IList<Job> GetJobs(ClientInformationSheet ClientInformationSheet)
        {
            Jobs = new List<Job>();
            foreach (var Job in ClientInformationSheet.Jobs)
            {
                Jobs.Add(Job);
            }
            return Jobs;
        }

		
		public IList<Job> Jobs { get; set; }
        public string JobDescription { get; set; }
        public string ContractorEmail { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string JobNumber { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CertRequiredBy { get; set; }
        public string RACLientNamedParty { get; set; }


    }
}
