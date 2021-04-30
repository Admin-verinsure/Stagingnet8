using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using System.Threading.Tasks;
using System;
using System.Linq;
using System.Collections.Generic;
using AutoMapper;
using DealEngine.Infrastructure.FluentNHibernate;

namespace DealEngine.Services.Impl
{
    public class SubsystemService : ISubsystemService
    {
        IOrganisationService _organisationService;
        IProgrammeService _programmeService;
        IProductService _productService;
        IClientInformationService _clientInformationService;
        IInformationTemplateService _informationTemplateService;
        IInformationSectionService _informationSectionService;
        IReferenceService _referenceService;
        ICloneService _cloneService;
        IMapper _mapper;
        IEmailService _emailService;
        IMapperSession<Reference> _referenceRepository;

        public SubsystemService(
            IEmailService emailService,
            ICloneService cloneService,
            IMapper mapper,
            IProductService productService,
            IInformationSectionService informationSectionService,
            IOrganisationService organisationService,
            IProgrammeService programmeService,            
            IClientInformationService clientInformationService,
            IInformationTemplateService informationTemplateService,
            IReferenceService referenceService,
            IMapperSession<Reference> referenceRepository
            )
        {
            _emailService = emailService;
            _cloneService = cloneService;
            _referenceService = referenceService;
            _mapper = mapper;
            _productService = productService;
            _informationSectionService = informationSectionService;
            _informationTemplateService = informationTemplateService;
            _programmeService = programmeService;
            _organisationService = organisationService;        
            _clientInformationService = clientInformationService;
            _referenceRepository = referenceRepository;
        }

        public async Task<bool> CreateSubObjects(Guid clientProgrammeId, ClientInformationSheet sheet, User user)
        {
            List<Organisation> principalOrganisations = null;
            if (sheet.Programme.BaseProgramme.NamedPartyUnitName == "TripleA Programme")
            {
                principalOrganisations = await _organisationService.GetTripleASubsystemAdvisors(sheet);
            }
            else
            {
                principalOrganisations = await _organisationService.GetNZFSGSubsystemAdvisors(sheet);
            }
           
            var clientProgramme = await _programmeService.GetClientProgrammebyId(clientProgrammeId);
            try
            {
                //await SubInformationTemplate(clientProgramme);
                foreach (var org in principalOrganisations)
                {
                    var subClientSheet = await CreateSubObjectProcess(clientProgramme, sheet, org);
                    sheet.SubClientInformationSheets.Add(subClientSheet);
                }
                if (principalOrganisations.Count != 0)
                {
                    sheet.submitted(user);                    
                }                
                await _clientInformationService.UpdateInformation(sheet);
                return true;
            }
            catch(Exception ex)
            {
                Exception subSystem = new Exception("Subsystem Failed", ex);                
                throw subSystem;
            }
        }

        private async Task<SubClientInformationSheet> CreateSubObjectProcess(ClientProgramme clientProgramme, ClientInformationSheet sheet, Organisation org)
        {
            SubClientInformationSheet subClientSheet;
            try
            {
                // compare if subclient sheet is attached to previous.sheet
                subClientSheet = await _clientInformationService.GetSubInformationSheetFor(org);
                if (subClientSheet == null)
                {
                    var subClientProgramme = await _programmeService.GetSubClientProgrammeFor(org);
                    if (subClientProgramme == null)
                    {
                        subClientProgramme = await CreateSubClientProgramme(clientProgramme, sheet, org);
                    }
                    subClientSheet = await CreateSubInformationSheet(subClientProgramme, sheet, org);
                }
                else if (subClientSheet != null && !sheet.SubClientInformationSheets.Contains(subClientSheet))
                {
                    if (subClientSheet.DateDeleted.HasValue || subClientSheet.DeletedBy != null)
                    {
                        SubClientProgramme subProg = (SubClientProgramme)subClientSheet.Programme;
                        subClientSheet.DateDeleted = null;
                        subClientSheet.Programme.DateDeleted = null;
                        subClientSheet.DeletedBy = null;
                        subClientSheet.Programme.DeletedBy = null;

                        clientProgramme.SubClientProgrammes.Add(subProg);
                    }
                    else
                    {
                        var subClientProgramme = await CreateSubClientProgramme(clientProgramme, sheet, org);
                        var createdSubSheet = await CreateSubInformationSheet(subClientProgramme, sheet, org);
                        foreach (var answer in subClientSheet.Answers)
                        {
                            createdSubSheet.AddAnswer(answer.ItemName, answer.Value);
                        }
                        foreach (var claim in subClientSheet.ClaimNotifications)
                        {
                            createdSubSheet.AddClaim(claim);
                        }
                        createdSubSheet.Status = subClientSheet.Status;
                        createdSubSheet.SubmittedBy = subClientSheet.SubmittedBy;
                        createdSubSheet.SubmitDate = DateTime.UtcNow;
                        subClientSheet = createdSubSheet;
                    }
                }
                else
                {
                    if (subClientSheet.DateDeleted.HasValue || subClientSheet.DeletedBy != null)
                    {
                        SubClientProgramme subProg = (SubClientProgramme)subClientSheet.Programme;
                        subClientSheet.DateDeleted = null;
                        subClientSheet.Programme.DateDeleted = null;
                        subClientSheet.DeletedBy = null;
                        subClientSheet.Programme.DeletedBy = null;

                        clientProgramme.SubClientProgrammes.Add(subProg);
                    }
                }

                //send out sub UIS invitation email
                if (subClientSheet.Status == "Not Started")
                {
                    await _emailService.SendSystemEmailLogin(org.Email);
                    await _emailService.SendSystemEmailAllSubUISInstruction(org, subClientSheet.Programme.BaseProgramme, subClientSheet);
                }

            }
            catch(Exception ex)
            {
                Exception subSystem = new Exception("Create Sub process Failed", ex);
                throw subSystem;
            }


            return subClientSheet;
        }

        private async Task<SubClientInformationSheet> CreateSubInformationSheet(SubClientProgramme subClientProgramme, ClientInformationSheet sheet, Organisation organisation)
        {
            try
            {
                var subSheet = await _clientInformationService.IssueSubInformationFor();
                subSheet.BaseClientInformationSheet = sheet;
                subSheet.Programme = subClientProgramme;
                subSheet.Status = "Not Started";
                subSheet.Owner = organisation;
                subSheet.ReferenceId = await _referenceService.GetLatestReferenceId();
                subClientProgramme.InformationSheet = subSheet;
                await _referenceRepository.AddAsync(new Reference(subSheet.Id, subSheet.ReferenceId));

                return subSheet;

            }
            catch (Exception ex)
            {
                Exception subSystem = new Exception("Create Sub Information Failed", ex);
                throw subSystem;
            }
        }

        private async Task<SubClientProgramme> CreateSubClientProgramme(ClientProgramme clientProgramme, ClientInformationSheet sheet, Organisation org)
        {
            try
            {
                var subClientProgramme = await _programmeService.CreateSubClientProgrammeFor(clientProgramme.Id);
                subClientProgramme.InformationSheet = sheet;
                subClientProgramme.Owner = org;
                clientProgramme.SubClientProgrammes.Add(subClientProgramme);                

                return subClientProgramme;
            }
            catch (Exception ex)
            {
                Exception subSystem = new Exception("Create Sub ClientProgramme Failed", ex);
                throw subSystem;
            }
        }

        private async Task SubInformationTemplate(ClientProgramme clientProgramme)
        {
            SubInformationTemplate subInformationTemplate = null;
            Product prod = null;
            try
            {                
                if (clientProgramme.BaseProgramme.Products.Count > 1)
                {
                    foreach (var prodMaster in clientProgramme.BaseProgramme.Products.Where(progp => progp.IsMasterProduct))
                    {
                        prod = prodMaster;
                        break;
                    }
                }
                else
                {
                    prod = clientProgramme.BaseProgramme.Products.FirstOrDefault();
                }
                
                SubInformationTemplate subInfomationTemplate = _mapper.Map<SubInformationTemplate>(prod.InformationTemplate);                
                subInfomationTemplate.BaseInformationTemplate = prod.InformationTemplate;
                List<InformationSection> sections = new List<InformationSection>();                 
                
                sections = await _informationSectionService.GetInformationSectionsbyTemplateId(subInfomationTemplate.BaseInformationTemplate.Id);
                
                foreach (var section in sections)
                {
                    section.Items = section.Items.OrderBy(i => i.ItemOrder).ToList();
                }
                //search any section using a where statement     
                int count = 1;
                var selectSections = sections.Where(s => s.Position == 1 || s.Position == 9 || s.Position == 11).ToList();
                foreach(var section in selectSections)
                {
                    var newSection = _mapper.Map<InformationSection>(section);
                    newSection.Position = count;
                    subInfomationTemplate.Sections.Add(newSection);
                }
                
                prod.SubInformationTemplate = subInfomationTemplate;
                await _productService.UpdateProduct(prod);
            }
            catch (Exception ex)
            {
                Exception subSystem = new Exception("Create Sub Template Failed", ex);
                throw subSystem;
            }
        }

        public async Task ValidateSubObjects(ClientInformationSheet informationSheet,  User user, List<Organisation> Advisors)
        {
            for (var i = informationSheet.SubClientInformationSheets.Count - 1; i >= 0; i--)
            {
                RemoveSubObjects(informationSheet, user, informationSheet.SubClientInformationSheets[i]);
            }

            if (Advisors.Any())
            {
                List<SubClientInformationSheet> subSheets = new List<SubClientInformationSheet>();

                foreach (var principal in Advisors)
                {
                    var subSheet = await CreateSubObjectProcess(informationSheet.Programme, informationSheet, principal);
                    subSheets.Add(subSheet);
                }

                foreach (var subsheet in subSheets)
                {
                    informationSheet.SubClientInformationSheets.Add(subsheet);
                }
            }

            informationSheet.submitted(user);
            await _clientInformationService.UpdateInformation(informationSheet);
        }

        public async Task<bool> ValidateProgramme(ClientInformationSheet informationSheet, User user)
        {
            if (informationSheet.Programme.BaseProgramme.NamedPartyUnitName == "NZFSG Programme")
            {
                var advisors = await _organisationService.GetNZFSGSubsystemAdvisors(informationSheet);
                await ValidateSubObjects(informationSheet, user, advisors);
            }
            else
            {
                var advisors = await _organisationService.GetTripleASubsystemAdvisors(informationSheet);
                await ValidateSubObjects(informationSheet, user, advisors);
            }

            return true;
        }

        private void RemoveSubObjects(ClientInformationSheet informationSheet, User user, SubClientInformationSheet subsheet)
        {
            try
            {                
                subsheet.Delete(user, DateTime.UtcNow);                
                informationSheet.Programme.SubClientProgrammes.Clear();
                informationSheet.SubClientInformationSheets.Remove(subsheet);
            }
            catch(Exception ex)
            {
                Exception subSystem = new Exception("Remove Subobjects Failed", ex);
                throw subSystem;
            }
        }

    }
}

