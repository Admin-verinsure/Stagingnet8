using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using FluentNHibernate.Conventions;
using NHibernate.Util;
using FluentNHibernate.Utils;
using ServiceStack;

namespace DealEngine.Services.Impl
{
    public class ProgrammeService : IProgrammeService
    {
        IMapperSession<Programme> _programmeRepository;
        IMapperSession<ClientProgramme> _clientProgrammeRepository;
        IClientInformationService _clientInformationService;
        IReferenceService _referenceService;
        ICloneService _cloneService;
        IMapperSession<Organisation> _organisationRepository;
        IMapperSession<Reference> _referenceRepository;

        public ProgrammeService(
            IMapperSession<Organisation> organisationRepository,
            IMapperSession<Programme> programmeRepository,
            IClientInformationService clientInformationService,
            IMapperSession<ClientProgramme> clientProgrammeRepository,
            IReferenceService referenceService,
            IMapperSession<Reference> referenceRepository,
            ICloneService cloneService
            )
        {
            _organisationRepository = organisationRepository;
            _cloneService = cloneService;
            _clientInformationService = clientInformationService;
            _programmeRepository = programmeRepository;
            _clientProgrammeRepository = clientProgrammeRepository;
            _referenceService = referenceService;
            _referenceRepository = referenceRepository;
        }

        public async Task<ClientProgramme> CreateClientProgrammeFor(Guid programmeId, User creatingUser, Organisation owner)
        {
            var programme = await GetProgramme(programmeId);
            return await CreateClientProgrammeFor(programme, creatingUser, owner);
        }

        public async Task<ClientProgramme> CreateClientProgrammeFor(Programme programme, User creatingUser, Organisation owner)
        {
            ClientProgramme clientProgramme = new ClientProgramme(creatingUser, owner, programme);
            clientProgramme.BrokerContactUser = programme.BrokerContactUser;
            await Update(clientProgramme);
            return clientProgramme;
        }

        public async Task<ClientProgramme> GetClientProgramme(Guid id)
        {
            return await _clientProgrammeRepository.GetByIdAsync(id);
        }

        public async Task<List<ClientProgramme>> GetClientProgrammesByProgramme(Guid programmeId)
        {
            var list = await _clientProgrammeRepository.FindAll().Where(cp => cp.BaseProgramme.Id == programmeId).ToListAsync();
            return list;
        }

        public async Task<List<ClientProgramme>> GetClientProgrammesByOwner(Guid ownerOrganisationId)
        {
            var list = await _clientProgrammeRepository.FindAll().Where(cp => cp.Owner.Id == ownerOrganisationId && cp.InformationSheet != null && cp.DateDeleted == null).ToListAsync();
            return list;
        }

        public async Task<List<ClientProgramme>> GetClientProgrammesForProgramme(Guid programmeId)
        {
            Programme programme = await GetProgramme(programmeId);
            var clientList = new List<ClientProgramme>();
            if (programme == null)
                return null;
            foreach (var client in programme.ClientProgrammes)
            {
                var isBaseClass = await IsBaseClass(client);
                if (isBaseClass)
                {
                    if (client.DateDeleted == null)
                    {
                        clientList.Add(client);
                    }
                }
            }

            return clientList;
        }

        public async Task<List<ClientProgramme>> GetSubClientProgrammesForProgramme(Guid programmeId)
        {
            Programme programme = await GetProgramme(programmeId);
            var clientList = new List<ClientProgramme>();
            if (programme == null)
                return null;
            foreach (var client in programme.ClientProgrammes)
            {
                var isBaseClass = await IsBaseClass(client);
                if (!isBaseClass)
                {
                    if (client.DateDeleted == null)
                    {
                        clientList.Add(client);
                    }
                }
            }

            return clientList;
        }


        public async Task<bool> IsBaseClass(ClientProgramme client)
        {
            var objectType = client.GetType();
            if (!objectType.IsSubclassOf(typeof(ClientProgramme)))
            {
                return true;
            }
            return false;
        }

        public async Task<Programme> GetProgramme(Guid id)
        {
            return await _programmeRepository.GetByIdAsync(id);
        }

        public async Task<List<Programme>> GetProgrammesByOwner(Guid ownerOrganisationId)
        {
            return await _programmeRepository.FindAll().Where(p => p.Owner.Id == ownerOrganisationId || p.IsPublic == true).ToListAsync();
        }

        public async Task<Programme> GetCoastGuardProgramme()
        {
            return await _programmeRepository.FindAll().FirstOrDefaultAsync(p => p.Name == "First Mate Cover");
        }

        public async Task Update(params ClientProgramme[] clientProgrammes)
        {
            foreach (ClientProgramme clientProgramme in clientProgrammes)
            {
                await _clientProgrammeRepository.AddAsync(clientProgramme);
            }

        }
        
        public async Task<ClientProgramme> CloneForRewenal (ClientProgramme clientProgramme, User cloningUser)
		{
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(_cloneService.GetCloneProfile());
            });

            var cloneMapper = mapperConfiguration.CreateMapper();
            ClientProgramme newClientProgramme = await CreateClientProgrammeFor(clientProgramme.BaseProgramme, cloningUser, clientProgramme.Owner);
			newClientProgramme.InformationSheet = clientProgramme.InformationSheet.CloneForRenewal (cloningUser, cloneMapper);
			newClientProgramme.InformationSheet.Programme = newClientProgramme;

            return newClientProgramme;
        }

        public async Task AttachProgrammeToActivities(Programme programme, BusinessActivityTemplate businessActivityTemplate)
        {
            programme.BusinessActivityTemplates.Add(businessActivityTemplate);
            await _programmeRepository.UpdateAsync(programme);
        }

        public async Task AttachProgrammeToTerritory(Programme programme, TerritoryTemplate territoryTemplate)
        {
            programme.TerritoryTemplates.Add(territoryTemplate);
            await _programmeRepository.UpdateAsync(programme);
        }

        public async Task<List<Programme>> GetAllProgrammes()
        {
            return await _programmeRepository.FindAll().ToListAsync();
        }

        public async Task<Programme> GetProgrammeById(Guid ProgrammeId)
        {
            return await _programmeRepository.GetByIdAsync(ProgrammeId);
        }

        public async Task AttachProgrammeToSharedRole(Programme programme, SharedDataRoleTemplate sharedRole)
        {
            programme.SharedDataRoleTemplates.Add(sharedRole);
            await _programmeRepository.UpdateAsync(programme);
        }

        public async Task AddClaimNotificationByMembership(ClaimNotification claimNotification)
        {
            var clientProgramme = await _clientProgrammeRepository.FindAll().FirstOrDefaultAsync(c => c.ClientProgrammeMembershipNumber == claimNotification.ClaimMembershipNumber);
            if (clientProgramme != null)
            {
                clientProgramme.InformationSheet.ClaimNotifications.Add(claimNotification);
                await _clientProgrammeRepository.UpdateAsync(clientProgramme);
            }
        }

        public async Task AddBusinessContractByMembership(BusinessContract businessContract)
        {
            var clientProgramme = await _clientProgrammeRepository.FindAll().FirstOrDefaultAsync(c => c.ClientProgrammeMembershipNumber == businessContract.MembershipNumber);
            if (clientProgramme != null)
            {
                clientProgramme.InformationSheet.BusinessContracts.Add(businessContract);
                await _clientProgrammeRepository.UpdateAsync(clientProgramme);
            }
        }

        public async Task AddPreRenewOrRefDataByMembership(PreRenewOrRefData preRenewOrRefData)
        {
            var clientProgramme = await _clientProgrammeRepository.FindAll().FirstOrDefaultAsync(c => c.ClientProgrammeMembershipNumber == preRenewOrRefData.RefField);
            if (clientProgramme != null)
            {
                clientProgramme.InformationSheet.PreRenewOrRefDatas.Add(preRenewOrRefData);
                await _clientProgrammeRepository.UpdateAsync(clientProgramme);
            }
        }

        public async Task<ClientProgramme> GetClientProgrammebyId(Guid clientProgrammeID)
        {
            return await _clientProgrammeRepository.GetByIdAsync(clientProgrammeID);
        }

        public async Task<SubClientProgramme> CreateSubClientProgrammeFor(Guid programmeId)
        {
            var programme = await GetClientProgrammebyId(programmeId);
            return await CreateSubClientProgrammeFor(programme);
        }

        public async Task<SubClientProgramme> CreateSubClientProgrammeFor(ClientProgramme programme)
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(_cloneService.GetCloneProfile());
            });

            var cloneMapper = mapperConfiguration.CreateMapper();
            SubClientProgramme subClientProgramme = cloneMapper.Map<SubClientProgramme>(programme);
            return subClientProgramme;
        }

        public async Task<bool> HasProgrammebyMembership(string membershipNumber)
        {
            var clientProgramme = await _clientProgrammeRepository.FindAll().FirstOrDefaultAsync(c => c.ClientProgrammeMembershipNumber == membershipNumber);
            if (clientProgramme == null)
            {
                return false;
            }
            return true;
        }

        public async Task<SubClientProgramme> GetSubClientProgrammebyId(Guid subClientProgrammeId)
        {
            return (SubClientProgramme)await _clientProgrammeRepository.GetByIdAsync(subClientProgrammeId);
        }

        public async Task<bool> SubsystemCompleted(ClientProgramme clientProgramme)
        {
            foreach (var subClient in clientProgramme.SubClientProgrammes)
            {
                if (subClient.InformationSheet.Status != "Submitted")
                {
                    return false;
                }
            }

            return true;
        }

        public async Task Update(Programme programmes)
        {
            await _programmeRepository.UpdateAsync(programmes);
        }

        public async Task AttachProgrammeToDataRole(Programme programme, SharedDataRoleTemplate template)
        {
            if (!programme.SharedDataRoleTemplates.Contains(template))
            {
                programme.SharedDataRoleTemplates.Add(template);
                await _programmeRepository.UpdateAsync(programme);
            }
        }

        public async Task<ClientInformationSheet> CreateUIS(Guid programmeId, User user, Organisation organisation)
        {
            var ClientProgramme = await CreateClientProgrammeFor(programmeId, user, organisation);
            var Reference = await _referenceService.GetLatestReferenceId();
            var Sheet = await _clientInformationService.IssueInformationFor(user, organisation, ClientProgramme, Reference);
            return Sheet;
        }

        public async Task<bool> AddOrganisationByMembership(Organisation organisation, string membership)
        {
            var clientProgramme = await _clientProgrammeRepository.FindAll().FirstOrDefaultAsync(c => c.ClientProgrammeMembershipNumber == membership);
            if (clientProgramme != null)
            {
                clientProgramme.InformationSheet.Organisation.Add(organisation);
                await _clientProgrammeRepository.UpdateAsync(clientProgramme);
                return true;
            }
            return false;
        }

        public async Task<SubClientProgramme> GetSubClientProgrammeFor(Organisation Owner)
        {
            var list = await _clientProgrammeRepository.FindAll().Where(c => c.Owner == Owner && c.DateDeleted == null).ToListAsync();
            var clientprogramme = list.LastOrDefault();
            return (SubClientProgramme)clientprogramme;
        }

        private async Task<List<Programme>> GetProgrammes(IFormCollection collection)
        {
            var programmes = new List<Programme>();
            foreach (var Key in collection.Keys)
            {
                Guid.TryParse(collection[Key], out Guid Id);
                if (Id != null)
                {
                    var Programme = await GetProgrammeById(Id);
                    if (Programme != null)
                    {
                        programmes.Add(Programme);
                    }
                }

            }
            return programmes;
        }

        public async Task<List<ClientInformationSheet>> SearchProgrammes(IFormCollection collection)
        {
            var Value = collection["Value"].ToString();
            var Term = collection["Term"].ToString();
            var programmes = await GetProgrammes(collection);

            if (Term == "Advisor")
            {
                return await AdvisorSearch(programmes, Value);
            }
            if (Term == "Boat")
            {
                return await BoatSearch(programmes, Value);
            }
            if (Term == "Name")
            {
                return await ClientNameSearch(programmes, Value);
            }
            if (Term == "Reference")
            {
                return await ReferenceSearch(programmes, Value);
            }
            return null;
        }

        private async Task<List<ClientInformationSheet>> ReferenceSearch(List<Programme> programmes, string value)
        {
            var Sheets = new List<ClientInformationSheet>();
            foreach (var Programme in programmes)
            {
                foreach (var ClientProgrammes in Programme.ClientProgrammes.Where(c => c.InformationSheet.ReferenceId == value && c.DateDeleted == null || c.Agreements.Any(a => a.ReferenceId == value) && c.DateDeleted == null))
                {
                    Sheets.Add(ClientProgrammes.InformationSheet);
                }
            }
            return Sheets;
        }

        private async Task<List<ClientInformationSheet>> ClientNameSearch(List<Programme> programmes, string value)
        {
            var Sheets = new List<ClientInformationSheet>();
            foreach (var Programme in programmes)
            {
                foreach (var ClientProgrammes in Programme.ClientProgrammes.Where(c => c.Owner.Name == value && c.DateDeleted == null))
                {
                    Sheets.Add(ClientProgrammes.InformationSheet);
                }
            }
            return Sheets;
        }

        private async Task<List<ClientInformationSheet>> BoatSearch(List<Programme> programmes, string value)
        {
            var Sheets = new List<ClientInformationSheet>();
            foreach (var Programme in programmes)
            {
                foreach (var ClientProgrammes in Programme.ClientProgrammes.Where(c => c.InformationSheet.Boats.Any(b => b.BoatName == value) && c.DateDeleted == null))
                {
                    Sheets.Add(ClientProgrammes.InformationSheet);
                }
            }
            return Sheets;
        }

        private async Task<List<ClientInformationSheet>> AdvisorSearch(List<Programme> programmes, string value)
        {
            var Sheets = new List<ClientInformationSheet>();
            foreach (var Programme in programmes)
            {
                foreach (var ClientProgrammes in Programme.ClientProgrammes.Where(c => c.InformationSheet.Organisation.Any(s => s.InsuranceAttributes.Any(i => i.Name == "Advisor")) && c.DateDeleted == null))
                {
                    var organisation = ClientProgrammes.InformationSheet.Organisation.FirstOrDefault(o => o.Name == value);
                    if (organisation != null)
                    {
                        Sheets.Add(ClientProgrammes.InformationSheet);
                    }
                }
            }
            return Sheets;
        }

        public async Task<ClientProgramme> CloneForUpdate(User createdBy, IFormCollection formCollection, Dictionary<string,string> collection)
        {            
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(_cloneService.GetCloneProfile());
            });

            var cloneMapper = mapperConfiguration.CreateMapper();

            ClientProgramme oldClientProgramme = null;
            ChangeReason changeReason = null;

            if (formCollection != null)
            {
                changeReason = new ChangeReason(createdBy, formCollection);
                oldClientProgramme = await GetClientProgramme(Guid.Parse(formCollection["DealId"]));
            }
            else
            {
                changeReason = new ChangeReason(createdBy);
                changeReason.ChangeType = collection["ChangeType"];
                changeReason.Reason = collection["Reason"];
                changeReason.Description = collection["ReasonDesc"];
                changeReason.EffectiveDate = DateTime.UtcNow;
                oldClientProgramme = await GetClientProgramme(Guid.Parse(collection["ClientProgrammeID"]));
            }
            
            ClientInformationSheet newClientInformationSheet = new ClientInformationSheet(createdBy, oldClientProgramme.Owner, null);
            newClientInformationSheet = cloneMapper.Map<ClientInformationSheet>(oldClientProgramme.InformationSheet);
            newClientInformationSheet.ReferenceId = await _referenceService.GetLatestReferenceId();
            newClientInformationSheet.IsChange = true;
            newClientInformationSheet.Status = "Not Started";
            newClientInformationSheet.DateCreated = DateTime.UtcNow;
            newClientInformationSheet.UnlockDate = DateTime.MinValue;
            newClientInformationSheet.PreviousInformationSheet = oldClientProgramme.InformationSheet;
            await _referenceRepository.AddAsync(new Reference(newClientInformationSheet.Id, newClientInformationSheet.ReferenceId));

            ClientProgramme newClientProgramme = new ClientProgramme(createdBy, oldClientProgramme.Owner, oldClientProgramme.BaseProgramme);
            newClientProgramme.BrokerContactUser = oldClientProgramme.BaseProgramme.BrokerContactUser;
            newClientProgramme.ChangeReason = changeReason;
            newClientProgramme.InformationSheet = newClientInformationSheet;
            newClientProgramme.InformationSheet.Programme = newClientProgramme;
            if (!string.IsNullOrEmpty(oldClientProgramme.EGlobalBranchCode))
                newClientProgramme.EGlobalBranchCode = oldClientProgramme.EGlobalBranchCode;
            if (!string.IsNullOrEmpty(oldClientProgramme.EGlobalClientNumber))
                newClientProgramme.EGlobalClientNumber = oldClientProgramme.EGlobalClientNumber;
            if (!string.IsNullOrEmpty(oldClientProgramme.EGlobalClientStatus))
                newClientProgramme.EGlobalClientStatus = oldClientProgramme.EGlobalClientStatus;
            newClientProgramme.HasEGlobalCustomDescription = oldClientProgramme.HasEGlobalCustomDescription;
            if (!string.IsNullOrEmpty(oldClientProgramme.EGlobalCustomDescription))
                newClientProgramme.EGlobalCustomDescription = oldClientProgramme.EGlobalCustomDescription;
            if (!string.IsNullOrEmpty(oldClientProgramme.ClientProgrammeMembershipNumber))
                newClientProgramme.ClientProgrammeMembershipNumber = oldClientProgramme.ClientProgrammeMembershipNumber;
            if (!string.IsNullOrEmpty(oldClientProgramme.Tier))
                newClientProgramme.Tier = oldClientProgramme.Tier;

            oldClientProgramme.InformationSheet.NextInformationSheet = newClientInformationSheet;
            if (oldClientProgramme.InformationSheet.Vehicles != null)
            {
                newClientInformationSheet.Vehicles.Clear();
                foreach (Vehicle vehicle in oldClientProgramme.InformationSheet.Vehicles)
                {
                    Vehicle newVehicle = vehicle.CloneForNewSheet(newClientInformationSheet);
                    newClientInformationSheet.Vehicles.Add(newVehicle);
                }
            }
            if (oldClientProgramme.InformationSheet.Boats != null)
            {
                newClientInformationSheet.Boats.Clear();
                foreach (Boat boat in oldClientProgramme.InformationSheet.Boats)
                {
                    Boat newBoat = boat.CloneForNewSheet(newClientInformationSheet);
                    newClientInformationSheet.Boats.Add(newBoat);
                }
            }
            if (oldClientProgramme.InformationSheet.RevenueData != null)
            {
                newClientInformationSheet.RevenueData = null;
                RevenueData newRevenueData = oldClientProgramme.InformationSheet.RevenueData.CloneForNewSheet(newClientInformationSheet);
                newClientInformationSheet.RevenueData = newRevenueData;
            }
            if (oldClientProgramme.InformationSheet.RoleData != null)
            {
                newClientInformationSheet.RoleData = null;
                RoleData newRoleData = oldClientProgramme.InformationSheet.RoleData.CloneForNewSheet(newClientInformationSheet);
                newClientInformationSheet.RoleData = newRoleData;
            }
            if (oldClientProgramme.InformationSheet.Answers != null)
            {
                newClientInformationSheet.Answers.Clear();
                foreach (ClientInformationAnswer answer in oldClientProgramme.InformationSheet.Answers)
                {
                    ClientInformationAnswer newClientInformationAnswer = answer.CloneForNewSheet(newClientInformationSheet);
                    newClientInformationSheet.Answers.Add(newClientInformationAnswer);
                }
            }
            if (oldClientProgramme.InformationSheet.BusinessInterruptions != null)
            {
                newClientInformationSheet.BusinessInterruptions.Clear();
                foreach (BusinessInterruption businessInterruption in oldClientProgramme.InformationSheet.BusinessInterruptions)
                {
                    BusinessInterruption newBusinessInterruption = businessInterruption.CloneForNewSheet(newClientInformationSheet);
                    newClientInformationSheet.BusinessInterruptions.Add(newBusinessInterruption);
                }
            }
            if (oldClientProgramme.InformationSheet.MaterialDamages != null)
            {
                newClientInformationSheet.MaterialDamages.Clear();
                foreach (MaterialDamage materialDamage in oldClientProgramme.InformationSheet.MaterialDamages)
                {
                    MaterialDamage newMaterialDamage = materialDamage.CloneForNewSheet(newClientInformationSheet);
                    newClientInformationSheet.MaterialDamages.Add(newMaterialDamage);
                }
            }
            if (oldClientProgramme.InformationSheet.BusinessContracts != null)
            {
                newClientInformationSheet.BusinessContracts.Clear();
                foreach (BusinessContract businessContract in oldClientProgramme.InformationSheet.BusinessContracts)
                {
                    BusinessContract newBusinessContract = businessContract.CloneForNewSheet(newClientInformationSheet);
                    newClientInformationSheet.BusinessContracts.Add(newBusinessContract);
                }
            }
            if (oldClientProgramme.InformationSheet.ResearchHouses != null)
            {
                newClientInformationSheet.ResearchHouses.Clear();
                foreach (ResearchHouse researchHouse in oldClientProgramme.InformationSheet.ResearchHouses)
                {
                    ResearchHouse newResearchHouse = researchHouse.CloneForNewSheet(newClientInformationSheet);
                    newClientInformationSheet.ResearchHouses.Add(newResearchHouse);
                }
            }
            //if (oldClientProgramme.InformationSheet.SubClientInformationSheets != null)
            //{
            //    newClientInformationSheet.SubClientInformationSheets.Clear();
            //    foreach (SubClientInformationSheet subClientInformationSheet in oldClientProgramme.InformationSheet.SubClientInformationSheets)
            //    {
            //        SubClientInformationSheet newSubClientInformationSheet = subClientInformationSheet.CloneForNewSheet(newClientInformationSheet);
            //        newClientInformationSheet.SubClientInformationSheets.Add(newSubClientInformationSheet);
            //    }
            //}

            await Update(newClientProgramme);
            return newClientProgramme;
        }

        public Task DeveloperTool()
        {
            throw new NotImplementedException();
        }

        public async Task<Programme> PostProgramme(User user, User broker, Programme Source, Programme Destination)
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(_cloneService.GetSerialiseProfile());
            });
            var cloneMapper = mapperConfiguration.CreateMapper();
            Destination = cloneMapper.Map(Source, Destination);

            Destination.LastModified(user);
            Destination.BrokerContactUser = broker;
            await Update(Destination);
            return Destination;
        }

        public async Task AttachOrganisationToClientProgramme(IFormCollection collection, ClientProgramme clientProgramme)
        {
            if (Guid.TryParse(collection["RemovedOrganisation.Id"], out Guid AttachOrganisationId))
            {
                if(AttachOrganisationId != Guid.Empty)
                {
                    var Organisation = await _organisationRepository.GetByIdAsync(AttachOrganisationId);
                    Organisation.Removed = false;
                    foreach (AdvisorUnit unit in Organisation.OrganisationalUnits.OfType<AdvisorUnit>())
                    {

                        if (unit.IsPrincipalAdvisor == true)
                        {
                            unit.IsPrincipalAdvisor = false;
                        }
                    }
                    if (Organisation != null)
                    {
                        if (clientProgramme != null)
                        {
                            if (!clientProgramme.InformationSheet.Organisation.Contains(Organisation))
                            {
                                clientProgramme.InformationSheet.Organisation.Add(Organisation);
                                await Update(clientProgramme);
                            }
                        }                        
                    }
                }                
            }
        }
    }
}

