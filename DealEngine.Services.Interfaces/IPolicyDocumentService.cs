using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using SystemDocument = DealEngine.Domain.Entities.Document;

namespace DealEngine.Services.Interfaces
{
    public interface IPolicyDocumentService
    {
        Task<List<SystemDocument>> GetAllPolicyDocumentsForProgrammeAsync(
            ClientProgramme programme,
            User user,
            bool regenerateDocuments = true);

        Task<List<SystemDocument>> GetAgreementDocumentsAsync(
            ClientAgreement agreement,
            ClientProgramme programme,
            User user,
            bool regenerateDocuments = true);
    }

}
