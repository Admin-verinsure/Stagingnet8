using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface IFullProposalReportService
    {
        //FullProposalReport CreateFullProposalReport(string programmeId, string programmeName, string reportName);
        //void SaveFullProposalReport(FullProposalReport report);
        //FullProposalReport GetFullProposalReportById(string reportId);

        //Task<Domain.Entities.FullProposalReport> GetTemplateByName(string claimName);
        Task getFullProposalReport(string programmeId);
        //Task AddClaim(Domain.Entities.Claim claim);
        //Task RemoveClaim(string claim);


    }

}