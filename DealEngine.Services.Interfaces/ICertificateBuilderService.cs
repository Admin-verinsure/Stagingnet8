using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using DealEngine.Services;
using DealEngine.Services.Interfaces.Models;
namespace DealEngine.Services.Interfaces
{
	public interface ICertificateBuilderService
    {
        Task<CertificateAggregateModel> BuildAsync(
         ClientAgreement agreement,
         ClientProgramme programme);
    }
}

