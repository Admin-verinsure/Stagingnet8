using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace DealEngine.Services.Interfaces
{
    public interface IOrganisationService
    {
		Task<Organisation> CreateNewOrganisation (Organisation organisation);
		Organisation CreateNewOrganisation (string p1, OrganisationType organisationType, string ownerFirstName, string ownerLastName, string ownerEmail);
		Task DeleteOrganisation (User deletedBy, Organisation organisation);
        Task<List<Organisation>> GetAllOrganisations ();
        Task<Organisation> GetOrganisation(Guid organisationId);
        Task<Organisation> GetOrganisationByName(string organisationName);
        Task<Organisation> GetOrganisationByEmail(string organisationEmail);
        Task<Organisation> GetOrganisationByOrganisationalUnitId(Guid organisationalUnitId);
        Task PostOrganisation(IFormCollection collection, Organisation organistaion);
        Task<List<Organisation>> GetNZFSGSubsystemAdvisors(ClientInformationSheet sheet); 
        Task<List<Organisation>> GetTripleASubsystemAdvisors(ClientInformationSheet sheet);
        Task<List<Organisation>> GetAllOrganisationsByEmail(string email);
        Task<Organisation> CreateOrganisation(string Email, string Type, string OrganisationName, string OrganisationTypeName, string FirstName, string LastName, User Creator, IFormCollection collection);
        Task Update(Organisation organisation);
        Task UpdateAdvisorDates(IFormCollection collection);
        Task<List<Organisation>> GetPublicMarinas();
        Task<List<Organisation>> GetPublicFinancialInstitutes();
        Task<Organisation> GetMarina(WaterLocation waterLocation);
        Task<List<Organisation>> GetAllMarinas();
        Task<List<Organisation>> GetFinancialInstitutes();
        Task PostMarina(IFormCollection model);
        Task PostInstitute(IFormCollection model);
    }
}

