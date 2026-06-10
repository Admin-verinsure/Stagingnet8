using DealEngine.Domain.Entities;
using DealEngine.Services;
using DealEngine.Services.Interfaces.Enums;
using DealEngine.Services.Interfaces.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace DealEngine.Services.Interfaces
{
	public interface ICertificateBuilderService
    {
        Task<CertificateAggregateModel> BuildAsync(
         ClientAgreement agreement,
         ClientProgramme programme,
         CertificateType? certificateType);
    }

}

