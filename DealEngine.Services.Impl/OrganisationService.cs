using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Infrastructure.Ldap.Interfaces;
using DealEngine.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AutoMapper;


namespace DealEngine.Services.Impl
{
    public class OrganisationService : IOrganisationService
    {
        ISerializerationService _serializerationService;
        IMapperSession<Organisation> _organisationRepository;
        IOrganisationTypeService _organisationTypeService;
        ILdapService _ldapService;
        IUserService _userService;
        ILogger<OrganisationService> _logger;
        IMapper _mapper;

        public OrganisationService(IMapperSession<Organisation> organisationRepository,
            ISerializerationService serializerationService,
            IMapper mapper,
            IUserService userService,
            IOrganisationTypeService organisationTypeService,
            ILdapService ldapService,
            ILogger<OrganisationService> logger
            )
        {
            _serializerationService = serializerationService;
            _mapper = mapper;
            _logger = logger;
            _userService = userService;
            _organisationTypeService = organisationTypeService;
            _organisationRepository = organisationRepository;
            _ldapService = ldapService;
        }

        public async Task<Organisation> CreateNewOrganisation(Organisation organisation)
        {
            try
            {
                await Update(organisation);
                _ldapService.Create(organisation);
            }
            catch (Exception ex)
            {
                //org exists in LDap but not in application
                if (ex.HResult == 68)
                {
                    //await Update(organisation);
                }
            }

            return organisation;
        }

        public Organisation CreateNewOrganisation(string organisationName, OrganisationType organisationType, string ownerFirstName, string ownerLastName, string ownerEmail)
        {
            Organisation organisation = new Organisation(null, Guid.NewGuid(), organisationName, organisationType);
            // TODO - finish this later since I need to figure out what calls the controller function that calls this service function
            throw new NotImplementedException();
        }

        public async Task DeleteOrganisation(User deletedBy, Organisation organisation)
        {
            organisation.Delete(deletedBy);
            await Update(organisation);
        }

        public async Task<List<Organisation>> GetAllOrganisations()
        {
            // we don't want to query ldap. That way lies timeouts. Or Dragons.
            return await _organisationRepository.FindAll().ToListAsync();
        }
        public async Task UpdateOrganisationsEmail(String Email , String NewEmail)
        {
            foreach (Organisation org in await GetAllOrganisationsByEmail(Email))
            {
                org.Email = NewEmail;


            }
            // we don't want to query ldap. That way lies timeouts. Or Dragons.
        }
        public async Task<Organisation> GetOrganisation(Guid organisationId)
        {
            if (organisationId != Guid.Empty)
            {
                Organisation organisation = await _organisationRepository.GetByIdAsync(organisationId);
                // have a repo organisation? Return it
                if (organisation != null)
                    return organisation;
                //organisation = _ldapService.GetOrganisation(organisationId);
                //// have a ldap organisation but no repo? Update NHibernate & return
                //if (organisation != null)
                //{
                //    await Update(organisation);
                //    return organisation;
                //}
                throw new Exception("Organisation with id [" + organisationId + "] does not exist in the system");
            }
            return null;
        }

        public async Task PostOrganisation(IFormCollection collection, Organisation organisation)
        {
            string TypeName = collection["OrganisationViewModel.InsuranceAttribute"].ToString();
            if (organisation.Email != collection["OrganisationViewModel.User.Email"])
            {
                UpdateOrganisationsEmail(organisation.Email, collection["OrganisationViewModel.User.Email"]);
               
            }
            organisation = await UpdateOrganisation(collection, organisation);

            if (!string.IsNullOrWhiteSpace(TypeName))
            {
                await UpdateOrganisationUnit(organisation, collection);
                await UpdateInsuranceAttribute(organisation, collection);
            }
            ///we are not updating org in ldap now
            //await Update(organisation);
        }

        private async Task UpdateInsuranceAttribute(Organisation organisation, IFormCollection collection)
        {
            string TypeName = collection["OrganisationViewModel.InsuranceAttribute"].ToString();
            var IA = organisation.InsuranceAttributes.FirstOrDefault(i => i.Name == TypeName);
            if (IA == null)
            {
                organisation.InsuranceAttributes.Clear();
                organisation.InsuranceAttributes.Add(
                    new InsuranceAttribute(null, TypeName)
                    );
            }
        }

        private async Task UpdateOrganisationUnit(Organisation organisation, IFormCollection collection)
        {
            var InsuranceAttribute = collection["OrganisationViewModel.InsuranceAttribute"].ToString();
            var UnitName = "";
            if (InsuranceAttribute == "Administrator")
            {
                 UnitName = collection["AdministratorUnit"].ToString();
            }
            else if(InsuranceAttribute == "Director")
            {
                UnitName = collection["DirectorUnit"].ToString();
            }
            else if(InsuranceAttribute == "EBarrister" || InsuranceAttribute == "ABusiness")
            {
                UnitName = collection["EBaristerUnit"].ToString();

            }else if (InsuranceAttribute == "JBarrister")
            {
                UnitName = collection["JBaristerUnit"].ToString();

            }
            else
            {
                 UnitName = collection["Unit"].ToString();

            }
            string TypeName = collection["OrganisationViewModel.InsuranceAttribute"].ToString();
            Type UnitType = Type.GetType(UnitName);
            try
            {
                var jsonUnit = (OrganisationalUnit) await _serializerationService.GetDeserializedObject(UnitType, collection);
                var unit = organisation.OrganisationalUnits.FirstOrDefault(ou => ou.GetType() == jsonUnit.GetType());
                if (unit != null)
                {
                    _mapper.Map(jsonUnit, unit);
                    unit.Name = TypeName;
                }
                else
                {
                    unit = (OrganisationalUnit)Activator.CreateInstance(UnitType);
                    _mapper.Map(jsonUnit, unit);
                    unit.Name = TypeName;
                    organisation.OrganisationalUnits.Add(unit);
                }
            }
            catch(Exception ex)
            {
                new Exception("Failed to add Organisational Unit " + ex.Message);
            }
        }

        private async Task<User> UpdateOrganisationUser(IFormCollection collection, Organisation organisation)
        {
            var jsonUser = (User) await _serializerationService.GetDeserializedObject(typeof(User), collection);

            Guid.TryParse(collection["OrganisationViewModel.User.Id"], out Guid UserId);
            if (UserId != Guid.Empty)
            {
                User user = await _userService.GetUserById(UserId);
                if (user != null)
                {
                    var bool1 = user.Organisations.Contains(organisation);
                    foreach (var org in user.Organisations)
                    {
                        if(org.Id == organisation.Id)
                        {
                            user = _mapper.Map(jsonUser, user);
                            await _userService.Update(user);
                            return user;
                        }

                    }
                    //if (user.Organisations.Contains(organisation))
                    //{
                    //    user = _mapper.Map(jsonUser, user);
                    //    await _userService.Update(user);
                    //    return user;
                    //}                    
                }
            }
            return null;
        }

        private async Task<Organisation> UpdateOrganisation(IFormCollection collection, Organisation organisation)
        {
            var jsonOrganisation = (Organisation)await _serializerationService.GetDeserializedObject(typeof(Organisation), collection);
            var OrganisationType = collection["OrganisationViewModel.OrganisationType"];
            string TypeName = collection["OrganisationViewModel.InsuranceAttribute"].ToString();
            var user = await UpdateOrganisationUser(collection, organisation);
            organisation = _mapper.Map(jsonOrganisation, organisation);

            if (user != null)
            {
                //if(organisation.Id != user.PrimaryOrganisation.Id && organisation.Email == user.Email)
                //{
                //    organisation.Name = user.FirstName + " " + user.LastName;
                //}
                //else
                //{
                //    organisation.Name = jsonOrganisation.Name;
                //}
                if (jsonOrganisation.Name != "")
                {
                    organisation.Name = jsonOrganisation.Name;
                }

                if ((user.FirstName + " " + user.LastName) != organisation.Name && jsonOrganisation.Name != "" && TypeName != "")
                {
                    organisation.Name = user.FirstName + " " + user.LastName;
                }

                //if ((user.FirstName + " " + user.LastName) != organisation.Name && TypeName == "Advisor")
                //{
                //    organisation.Name = user.FirstName + " " + user.LastName;
                //}
            }
            var isfap = collection["OrganisationViewModel.Organisation.isTheFAP"];
            organisation.Email = collection["OrganisationViewModel.User.Email"].ToString();
            if (isfap == "true")
            {
                organisation.isOrganisationTheFAP = true;

                organisation.OrganisationFAPLicenseNumber = collection["OrganisationViewModel.Organisation.FAPLicenseNumber"];
            }

            if (!string.IsNullOrWhiteSpace(OrganisationType))
            {
                organisation.OrganisationType.Name = OrganisationType;                
            }           

            return organisation;
        }

        public async Task<Organisation> GetOrganisationByName(string organisationName)
        {
            return await _organisationRepository.FindAll().FirstOrDefaultAsync(o => o.Name == organisationName);
        }

        public async Task<Organisation> GetOrganisationByOrganisationalUnitId(Guid organisationalUnitId)
        {
            var list = await _organisationRepository.FindAll().ToListAsync();
            Organisation organisation = new Organisation();

            foreach(Organisation org in list)
            {
                var foundIt = org.OrganisationalUnits.FirstOrDefault(u => u.Id == organisationalUnitId);
                if (foundIt != null){
                    organisation = org;
                    break;
                }
            }
            return organisation;
        }

        public async Task<Organisation> GetOrganisationByEmail(string organisationEmail)
        {
            var list = await GetAllOrganisationsByEmail(organisationEmail);
            return list.OrderByDescending(i => i.DateCreated).FirstOrDefault();
        }

        public async Task<List<Organisation>> GetAllOrganisationsByEmail(string email)
        {
            return await _organisationRepository.FindAll().Where(o => o.Email == email).ToListAsync();
        }

        public async Task<Organisation> GetOrganisationByEmailAndName(string organisationEmail, string organisationName)
        {
            var list = await GetAllOrganisationsByEmailAndName(organisationEmail, organisationName);
            return list.OrderByDescending(i => i.DateCreated).FirstOrDefault();
        }
        public async Task<List<Organisation>> GetAllOrganisationsByEmailAndName(string email, string name)
        {
            return await _organisationRepository.FindAll().Where(o => o.Email == email && o.Name == name).ToListAsync();
        }

        public async Task<List<Organisation>> GetNZFSGSubsystemAdvisors(ClientInformationSheet sheet)
        {
            var organisations = new List<Organisation>();
            foreach (var organisation in sheet.Organisation.Where(o => o.Removed != true && o.InsuranceAttributes.Any(i => i.Name == "Advisor")))
            {
                var unit = (AdvisorUnit)organisation.OrganisationalUnits.FirstOrDefault(u => u.Name == "Advisor");
                if (unit != null)
                {
                    if (!unit.IsPrincipalAdvisor)
                    {
                        organisations.Add(organisation);
                    }
                }
            }
            return organisations;
        }

        public async Task<List<Organisation>> GetTripleASubsystemAdvisors(ClientInformationSheet sheet)
        {
            var organisations = new List<Organisation>();
            foreach (var organisation in sheet.Organisation.Where(o => o.InsuranceAttributes.Any(i => i.Name == "Advisor" || i.Name == "Nominated Representative")))
            {
                var UnitName = organisation.InsuranceAttributes.FirstOrDefault().Name;
                var unit = (AdvisorUnit)organisation.OrganisationalUnits.FirstOrDefault(u => u.Name == UnitName);
                if (unit != null)
                {
                    if (!unit.IsPrincipalAdvisor && organisation.Removed != true)
                    {
                        organisations.Add(organisation);
                    }
                }
            }
            return organisations;
        }


        public async Task<Organisation> CreateOrganisation(string Email, string Type, string OrganisationName, string OrganisationTypeName, string FirstName, string LastName, User Creator, IFormCollection collection)
        {
            Organisation foundOrg = null;//= await GetOrganisationByEmail(Email);
            User User = null;
            if (foundOrg == null)
            {
                if (string.IsNullOrWhiteSpace(OrganisationName))
                {
                    OrganisationName = FirstName + " " + LastName;
                    OrganisationTypeName = "Person - Individual";

                }
                if (!string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(FirstName) && !string.IsNullOrWhiteSpace(LastName))
                {
                    User = await _userService.GetUserByEmail(Email);
                    if (User != null)
                    {
                        var SameUser = await _userService.GetUser(User.UserName);
                        if (User != SameUser)
                        {
                            User = new User(Creator, Guid.NewGuid(), collection);
                        }
                    }
                    else
                    {
                        if(!string.IsNullOrWhiteSpace(FirstName) || !string.IsNullOrWhiteSpace(LastName))
                            User = new User(Creator, Guid.NewGuid(), collection);
                    }
                }
                List<OrganisationalUnit> OrganisationalUnits = GetOrganisationCreateUnits(Type, Creator, collection);
                InsuranceAttribute InsuranceAttribute = new InsuranceAttribute(Creator, Type);
                OrganisationType OrganisationType = await _organisationTypeService.GetOrganisationTypeByName(OrganisationTypeName);
                foundOrg = CreateNewOrganisation(Creator, Email, OrganisationName, OrganisationType, OrganisationalUnits, InsuranceAttribute);
                if (User != null)
                {
                    if(!User.Organisations.Any(o=>o.InsuranceAttributes.Any(i=>i.Name == Type) && o.Name == OrganisationName))
                        User.Organisations.Add(foundOrg);

                    if(Type != "Administrator" && User.PrimaryOrganisation == null)
                    {
                        User.SetPrimaryOrganisation(foundOrg);
                    }
                    
                    await _userService.Create(User);
                }
            }
            return foundOrg;
        }

        private List<OrganisationalUnit> GetOrganisationCreateUnits(string Type, User User, IFormCollection collection)
        {
            List<OrganisationalUnit> OrganisationalUnits = new List<OrganisationalUnit>();
            string OrganisationTypeName;
            if (Type == "Corporation – Limited liability")
            {
                OrganisationTypeName = "Corporation – Limited liability";
                OrganisationalUnits.Add(new OrganisationalUnit(User, "Head Office", OrganisationTypeName, collection));
            }
            else if (Type == "Trust")
            {
                OrganisationTypeName = "Trust";
                OrganisationalUnits.Add(new OrganisationalUnit(User, "Head Office", OrganisationTypeName, collection));
            }
            else if (Type == "Partnership")
            {
                OrganisationTypeName = "Partnership";
                OrganisationalUnits.Add(new OrganisationalUnit(User, "Head Office", OrganisationTypeName, collection));
            }
            else
            {
                OrganisationTypeName = "Person - Individual";
                if (Type == "Person - Individual")
                {
                    OrganisationalUnits.Add(new OrganisationalUnit(User, Type, OrganisationTypeName, collection));
                }
                if (Type == "Advisor" ||
                    Type == "Nominated Representative" ||
                    Type == "Administration" ||
                    Type == "Other Consulting Business" ||
                    Type == "Mentored Advisor" ||
                    Type == "Director"
                    )
                {
                    OrganisationalUnits.Add(new OrganisationalUnit(User, "Person - Individual", OrganisationTypeName, collection));
                    OrganisationalUnits.Add(new AdvisorUnit(User, Type, OrganisationTypeName, collection));
                }
                if (Type == "Administrator"
                  )
                {
                    OrganisationalUnits.Add(new OrganisationalUnit(User, "Person - Individual", OrganisationTypeName, collection));
                    OrganisationalUnits.Add(new AdministratorUnit(User, Type, OrganisationTypeName, collection));
                }
                if (Type == "Personnel")
                {
                    OrganisationalUnits.Add(new OrganisationalUnit(User, "Person - Individual", OrganisationTypeName, collection));
                    OrganisationalUnits.Add(new PersonnelUnit(User, Type, OrganisationTypeName, collection));
                }
                if (Type == "Principal")
                {
                    OrganisationalUnits.Add(new OrganisationalUnit(User, "Person - Individual", OrganisationTypeName, collection));
                    OrganisationalUnits.Add(new PrincipalUnit(User, Type, OrganisationTypeName, collection));
                }
                if (Type == "Planner" || Type == "Contractor")
                {
                    OrganisationalUnits.Add(new OrganisationalUnit(User, "Person - Individual", OrganisationTypeName, collection));
                    OrganisationalUnits.Add(new PlannerUnit(User, Type, OrganisationTypeName, collection));
                }
                if ( Type == "ABusiness")
                {
                    OrganisationalUnits.Add(new OrganisationalUnit(User, "Person - Individual", OrganisationTypeName, collection));
                    OrganisationalUnits.Add(new EBaristerUnit(User, Type, OrganisationTypeName, collection));
                }
                if (Type == "Barrister" 
                   )
                {
                    OrganisationalUnits.Add(new OrganisationalUnit(User, "Person - Individual", OrganisationTypeName, collection));
                    OrganisationalUnits.Add(new BarristerUnit(User, Type, OrganisationTypeName, collection));
                }
            }

            return OrganisationalUnits;
        }

        private Organisation CreateNewOrganisation(User Creator, string email, string organisationName, OrganisationType organisationType, List<OrganisationalUnit> organisationalUnits, InsuranceAttribute insuranceAttribute)
        {
            var Organisation = new Organisation(Creator, Guid.NewGuid(), organisationName, organisationType, organisationalUnits, insuranceAttribute, email);
            return Organisation;
        }

        private void UpdateLDap(Organisation organisation)
        {
            _ldapService.Update(organisation);
        }

        public async Task Update(Organisation organisation)
        {
            try
            {
                UpdateLDap(organisation);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex.Message);
            }
            finally
            {
                await _organisationRepository.UpdateAsync(organisation);
            }
        }

        public async Task UpdateAdvisorDates(IFormCollection collection)
        {
            var guids = collection["OrganisationId"].ToString().Split(',');
            var DODates = collection["DORetroactiveDate"].ToString().Split(',');
            var PIDates = collection["PIRetroactiveDate"].ToString().Split(',');
            for (int i = 0; i < guids.Count(); i++) 
            {
                Guid.TryParse(guids[i], out Guid OrganisationId);
                if (OrganisationId != Guid.Empty)
                {
                    var organisation = await GetOrganisation(OrganisationId);
                    if(organisation != null)
                    {
                        var unit = (AdvisorUnit)organisation.OrganisationalUnits.FirstOrDefault(o => o.Name == "Advisor");
                        if(unit != null)
                        {
                            unit.DORetroactivedate = DODates[i];
                            unit.PIRetroactivedate = PIDates[i];
                            await Update(organisation);
                        }
                    }
                }
            }
        }

        public async  Task<List<Organisation>> GetPublicMarinas()
        {
            List<Organisation> organisations = new List<Organisation>();
            var marinas = await _organisationRepository.FindAll().Where(o => o.InsuranceAttributes.Any(i => i.Name == "Marina")).ToListAsync();
            foreach(var marina in marinas)
            {
                var unit = (MarinaUnit)marina.OrganisationalUnits.FirstOrDefault();
                if(unit != null)
                {
                    if (unit.WaterLocation != null)
                    {
                        if (unit.WaterLocation.IsPublic)
                        {
                            organisations.Add(marina);
                        }
                    }

                }
            }

            return organisations;
        }

        public async Task<List<Organisation>> GetPublicFinancialInstitutes()
        {
            List<Organisation> organisations = new List<Organisation>();
            var FinancialList = await GetFinancialInstitutes();
            foreach (var Financial in FinancialList)
            {
                var unit = (InterestedPartyUnit)Financial.OrganisationalUnits.Where(i => i.Name == "Financial").FirstOrDefault();
                if (unit != null)
                {
                    if (unit.Location != null)
                    {
                        if (unit.Location.IsPublic)
                        {
                            organisations.Add(Financial);
                        }
                    }
                }
            }

            return organisations;
        }

        public async Task<Organisation> GetMarina(WaterLocation waterLocation)
        {
            var marinas = await GetAllMarinas();
            foreach (var marina in marinas)
            {                
                var unit = (MarinaUnit)marina.OrganisationalUnits.FirstOrDefault();
                if (unit != null)
                {
                    if (unit.WaterLocation == waterLocation)
                    {
                        return marina;
                    }
                }
            }
            return null;
        }

        public async Task<List<Organisation>> GetAllMarinas()
        {
            return  await _organisationRepository.FindAll().Where(o => o.InsuranceAttributes.Any(i => i.Name == "Marina")).ToListAsync();
        }

        public async Task<List<Organisation>> GetFinancialInstitutes()
        {
            return await _organisationRepository.FindAll().Where(o => o.InsuranceAttributes.Any(i => i.Name == "Financial")).ToListAsync();
        }

        public async Task PostMarina(IFormCollection model)
        {
            Organisation organisation = null;
            if (Guid.TryParse(model["Organisation.Id"], out Guid OrganisationId))
            {
                organisation = await GetOrganisation(Guid.Parse(model["Organisation.Id"]));
                MarinaUnit marinaUnit = (MarinaUnit)organisation.OrganisationalUnits.FirstOrDefault();
                var jsonOrganisation = (Organisation)await _serializerationService.GetDeserializedObject(typeof(Organisation), model);
                var jsonWaterLocation = (WaterLocation)await _serializerationService.GetDeserializedObject(typeof(WaterLocation), model);
                jsonWaterLocation.IsPublic = true;
                organisation = _mapper.Map(jsonOrganisation, organisation);
                marinaUnit.WaterLocation = _mapper.Map(jsonWaterLocation, marinaUnit.WaterLocation);
            }
            else
            {

                organisation = new Organisation(null, Guid.NewGuid());
                organisation.Name = model["Organisation.Name"];
                organisation.Email = model["Organisation.Email"];
                OrganisationType organisationType = new OrganisationType("Corporation – Limited liability");
                InsuranceAttribute insuranceAttribute = new InsuranceAttribute(null, "Marina");
                MarinaUnit marinaUnit = new MarinaUnit(null, "Marina", "Corporation – Limited liability", null);
                WaterLocation DefaultMar = new WaterLocation(null);
                DefaultMar.MarinaName = model["WaterLocation.MarinaName"];
                DefaultMar.IsPublic = true;
                marinaUnit.WaterLocation = DefaultMar;
                organisation.OrganisationType = organisationType;
                organisation.InsuranceAttributes.Add(insuranceAttribute);
                organisation.OrganisationalUnits.Add(marinaUnit);
            }

            await Update(organisation);            
        }

        public async Task PostInstitute(IFormCollection model)
        {
            Organisation organisation = null;
            if (Guid.TryParse(model["Organisation.Id"], out Guid OrganisationId))
            {
                organisation = await GetOrganisation(Guid.Parse(model["Organisation.Id"]));
                InterestedPartyUnit unit = (InterestedPartyUnit)organisation.OrganisationalUnits.FirstOrDefault();
                var jsonOrganisation = (Organisation)await _serializerationService.GetDeserializedObject(typeof(Organisation), model);
                var jsonLocation = (Location)await _serializerationService.GetDeserializedObject(typeof(Location), model);
                organisation = _mapper.Map(jsonOrganisation, organisation);
                unit.Location = _mapper.Map(jsonLocation, unit.Location);
            }
            else
            {
                organisation = new Organisation(null, Guid.NewGuid());
                OrganisationType organisationType6 = new OrganisationType("Corporation – Limited liability");
                InsuranceAttribute insuranceAttribute6 = new InsuranceAttribute(null, "Financial");
                InterestedPartyUnit partyUnit = new InterestedPartyUnit(null, "Financial", "Corporation – Limited liability", null);
                partyUnit.Location = new Location(null);
                partyUnit.Location.IsPublic = true;
                partyUnit.Location.CommonName = model["Location.CommonName"];
                partyUnit.Location.Street = model["Location.Street"];
                partyUnit.Location.Suburb = model["Location.Suburb"];
                partyUnit.Location.City = model["Location.City"];
                organisation.Name = model["Institute.Name"];
                organisation.Email = model["Institute.Email"];
                organisation.OrganisationType = organisationType6;
                organisation.InsuranceAttributes.Add(insuranceAttribute6);
                organisation.OrganisationalUnits.Add(partyUnit);
            }
            await Update(organisation);
        }
    }

}

