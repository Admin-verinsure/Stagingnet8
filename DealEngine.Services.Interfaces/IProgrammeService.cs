using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace DealEngine.Services.Interfaces
{
	public interface IProgrammeService
	{
        Task<Programme> GetProgramme (Guid id);
        Task<Programme> GetCoastGuardProgramme();
        Task<List<Programme>> GetProgrammesByOwner (Guid ownerOrganisationId);
		Task<ClientProgramme> GetClientProgramme (Guid id);
        Task<List<ClientProgramme>> GetClientProgrammesByOwner (Guid ownerOrganisationId);
        Task<List<ClientProgramme>> GetClientProgrammesByOwnerByProgramme(Guid ownerOrganisationId, Guid programmeId);
        Task<ClientProgramme> GetOriginalClientProgrammeByOwnerByProgramme(Guid ownerOrganisationId, Guid programmeId);
        Task<List<ClientProgramme>> GetClientProgrammesForProgramme (Guid programmeId);
        Task<List<ClientProgramme>> GetRenewBaseClientProgrammesForProgramme(Guid programmeId);
        Task<ClientProgramme> CreateClientProgrammeFor (Guid programmeId, User creatingUser, Organisation owner);
        Task<ClientProgramme> CreateClientProgrammeFor (Programme programme, User creatingUser, Organisation owner);
		Task Update (params ClientProgramme[] clientProgrammes);
        Task Update(Programme programmes);
        Task<ClientProgramme> CloneForRewenal (ClientProgramme clientProgramme, User cloningUser);
        Task AttachProgrammeToActivities(Programme programme, BusinessActivityTemplate businessActivityTemplate);
        Task<List<Programme>> GetAllProgrammes();
        Task<Programme> GetProgrammeById(Guid ProgrammeId);
        Task AttachProgrammeToTerritory(Programme programme, TerritoryTemplate territoryTemplate);
        Task AttachProgrammeToSharedRole(Programme programme, SharedDataRoleTemplate sharedRole);
        Task AddClaimNotificationByMembership(ClaimNotification claimNotification);
        Task AddBusinessContractByMembership(BusinessContract businessContract);
        Task AddPreRenewOrRefDataByMembership(PreRenewOrRefData preRenewOrRefData);
        Task AddPreRenewOrRefDataByMembershipAndProgramme(PreRenewOrRefData preRenewOrRefData, Programme programme);
        Task<ClientProgramme> GetClientProgrammebyId(Guid clientProgrammeID);
        Task<SubClientProgramme> CreateSubClientProgrammeFor(Guid programmeId);
        Task<bool> HasProgrammebyMembership(string membershipNumber);
        Task<SubClientProgramme> GetSubClientProgrammebyId(Guid subClientProgrammeId);
        Task<bool> IsBaseClass(ClientProgramme clientProgramme);
        Task<bool> SubsystemCompleted(ClientProgramme clientProgramme);
        Task<List<ClientProgramme>> GetSubClientProgrammesForProgramme(Guid programmeId);
        Task AttachProgrammeToDataRole(Programme programme, SharedDataRoleTemplate template);
        Task<ClientInformationSheet> CreateUIS(Guid programmeId, User user, Organisation organisation);
        Task<bool> AddOrganisationByMembership(Organisation organisation, string membership);
        Task<SubClientProgramme> GetSubClientProgrammeFor(Organisation org);
        Task<List<ClientInformationSheet>> SearchProgrammes(IFormCollection collection);
        Task<ClientProgramme> CloneForUpdate(User createdBy, IFormCollection formCollection, Dictionary<string,string> collection);
        Task DeveloperTool();
        Task<Programme> PostProgramme(User user, User brokerUser, Programme jsonProgramme, Programme programme);
        Task AttachOrganisationToClientProgramme(IFormCollection collection, ClientProgramme clientProgramme);
        Task MoveAdvisorsToClientProgramme(IList<string> advisors, ClientProgramme clientProgramme, ClientProgramme sourceClientProgramme, User user, string targetOwnerFAP);
        Task<List<ClientAgreement>> CloneAgreementsForUpdate(User createdBy, Guid oldProgrammeId, Guid currentProgrammeId);
        Task<ClientProgramme> CloneForRenew(User createdBy, Guid renewFromProgrammeBaseId, Guid currentProgrammeId);
        Task<ClientProgramme> GetClientProgrammebyOwnerName(String Programmename , String OwnerName);
        Task<ClientProgramme> GetOriginalClientProgrammeByReferenceNum(String RefrenceNum);
        Task<bool> AddOrganisationByMembershipByProgram(Organisation organisation, string membership,Guid Progid);
        
    }
}

