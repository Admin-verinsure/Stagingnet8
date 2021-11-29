using DealEngine.Services.Interfaces;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Domain.Entities;
using System.Threading.Tasks;
using NHibernate.Linq;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl
{
    public class ImportService : IImportService
    {
        IOrganisationService _organisationService;
        IUserService _userService;
        IProgrammeService _programmeService;
        IReferenceService _referenceService;
        IClientInformationService _clientInformationService;
        IUnitOfWork _unitOfWork;
        IOrganisationTypeService _organisationTypeService;
        IBusinessActivityService _businessActivityService;
        IInsuranceAttributeService _InsuranceAttributeService;
        private readonly string WorkingDirectory;
        IAppSettingService _appSettingService;

        public ImportService(
            IOrganisationService organisationService,
            IUserService userService,
            IProgrammeService programmeService,
            IReferenceService referenceService,
            IClientInformationService clientInformationService,
            IUnitOfWork unitOfWork,
            IOrganisationTypeService organisationTypeService,
            IInsuranceAttributeService insuranceAttributeService,
            IMapperSession<Organisation> organisationRepository,
            IBusinessActivityService businessActivityService,
            IAppSettingService appSettingService)
        {

            _businessActivityService = businessActivityService;
            _InsuranceAttributeService = insuranceAttributeService;
            _organisationTypeService = organisationTypeService;
            _organisationService = organisationService;
            _userService = userService;
            _programmeService = programmeService;
            _referenceService = referenceService;
            _clientInformationService = clientInformationService;
            _unitOfWork = unitOfWork;
            _appSettingService = appSettingService;

            if (_appSettingService.IsLinuxEnv == "True")
            {
                WorkingDirectory = "/tmp/";
            }
            else
            {
                WorkingDirectory = "C:\\Users\\Public\\";
            }

        }

        public async Task ImportAOEServiceIndividuals(User CreatedUser)
        {
            //addresses need to be on one line
            //var userFileName = "C:\\tmp\\testclientdata\\NZACSUsers2018.csv";
            //var userFileName = "/tmp/nzfsg test spreadsheet.csv";
            var userFileName = "/tmp/NZACSUsers2018.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("214efe24-552d-46a3-a666-6bede7c88ca1");
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = false;
            string line;
            using (reader = new StreamReader(userFileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    //if (!readFirstLine)
                    //{
                    //    line = reader.ReadLine();
                    //    readFirstLine = true;
                    //}
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    try
                    {
                        if (parts[0] == "f")
                        {
                            if (!string.IsNullOrWhiteSpace(parts[4]))
                            {
                                organisation = await _organisationService.GetOrganisationByEmail(parts[4]);
                            }
                            if (organisation == null)
                            {
                                var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Person - Individual");
                                organisation = new Organisation(currentUser, Guid.NewGuid(), parts[2] + " " + parts[3], organisationType, parts[4]);
                                await _organisationService.CreateNewOrganisation(organisation);
                            }
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(parts[4]))
                            {
                                organisation = await _organisationService.GetOrganisationByEmail(parts[4]);
                            }
                            if (organisation == null)
                            {
                                var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Corporation – Limited liability");
                                organisation = new Organisation(currentUser, Guid.NewGuid(), parts[1], organisationType, parts[4]);
                                await _organisationService.CreateNewOrganisation(organisation);
                            }
                        }

                        if (!string.IsNullOrWhiteSpace(parts[4]))
                        {
                            var email = parts[7] + "@techcertain.com";
                            user = await _userService.GetUserByEmail(email);
                        }
                        if (user == null)
                        {
                            user = new User(currentUser, Guid.NewGuid(), parts[7]);
                        }

                        user.FirstName = parts[2];
                        user.LastName = parts[3];
                        user.FullName = parts[2] + " " + parts[3];
                        user.Email = parts[4];
                        user.Address = parts[5];
                        user.Phone = "12345";

                        if (!user.Organisations.Contains(organisation))
                            user.Organisations.Add(organisation);
                        user.SetPrimaryOrganisation(organisation);

                        await _userService.ApplicationCreateUser(user);

                        var programme = await _programmeService.GetProgramme(programmeID);
                        var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                        var reference = await _referenceService.GetLatestReferenceId();
                        var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                        await _referenceService.CreateClientInformationReference(sheet);

                        using (var uow = _unitOfWork.BeginUnitOfWork())
                        {

                            clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                            clientProgramme.ClientProgrammeMembershipNumber = parts[6];
                            sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                            try
                            {
                                await uow.Commit();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportAOEServicePrincipals(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = false;
            string line;
            var email = "";

            //addresses need to be on one line
            //var principalsFileName = "C:\\tmp\\testclientdata\\NZACSPrincipals2018.csv";
            var principalsFileName = WorkingDirectory + "NZACSPrincipals2018.csv";
            //var insuranceAttribute = await _InsuranceAttributeService.GetInsuranceAttributeByName("Principal");
            var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Person - Individual");
            if (organisationType == null)
            {
                organisationType = await _organisationTypeService.CreateNewOrganisationType(null, "Person - Individual");
            }
            using (reader = new StreamReader(principalsFileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    //if (!readFirstLine)
                    //{
                    //    line = reader.ReadLine();
                    //    readFirstLine = true;
                    //}
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    try
                    {
                        var hasProgramme = await _programmeService.HasProgrammebyMembership(parts[1]);
                        if (hasProgramme)
                        {
                            string userName = parts[4] + "_" + parts[3];

                            if (string.IsNullOrWhiteSpace(parts[5]))
                            {
                                email = parts[2] + "@techcertain.com";
                            }
                            else
                            {
                                email = parts[5];
                            }

                            organisation = new Organisation(currentUser, Guid.NewGuid(), parts[2], organisationType, email);
                            //organisation.InsuranceAttributes.Add(insuranceAttribute);
                            //organisation.NZIAmembership = parts[1];
                            organisation.Email = email;
                            organisation.Phone = "12345";

                            if (!string.IsNullOrEmpty(parts[6]))
                            {
                                //organisation.Qualifications = parts[6];
                            }
                            if (!string.IsNullOrEmpty(parts[7]))
                            {
                                //if (parts[7] == "1")
                                //{
                                //    organisation.IsNZIAmember = true;
                                //}
                                //else
                                //    organisation.IsNZIAmember = false;
                            }
                            if (!string.IsNullOrEmpty(parts[8]))
                            {
                                //if (parts[8] == "1")
                                //{
                                //    organisation.IsADNZmember = true;
                                //}
                                //else
                                //    organisation.IsADNZmember = false;
                            }
                            //clarify correct field
                            if (!string.IsNullOrEmpty(parts[9]))
                            {
                                //if (parts[9] == "1")
                                //{
                                //    organisation.IsOtherdirectorship = true;
                                //}
                                //else
                                //    organisation.IsOtherdirectorship = false;
                            }

                            using (var uom = _unitOfWork.BeginUnitOfWork())
                            {
                                //insuranceAttribute.IAOrganisations.Add(organisation);
                                try
                                {
                                    await uom.Commit();
                                }
                                catch (Exception ex)
                                {
                                    await uom.Rollback();
                                }
                            }

                            await _organisationService.CreateNewOrganisation(organisation);
                            //await _programmeService.AddOrganisationByMembership(organisation);

                            user = await _userService.GetUserByEmail(email);

                            if (user == null)
                            {
                                userName = parts[4] + "_" + parts[3];
                                try
                                {
                                    user = await _userService.GetUser(userName);
                                }
                                catch (Exception ex)
                                {
                                    Random random = new Random();
                                    int randomNumber = random.Next(10, 99);
                                    userName = userName + randomNumber.ToString();
                                }
                                user = new User(currentUser, Guid.NewGuid(), userName);
                                user.FirstName = parts[4];
                                user.LastName = parts[3];
                                user.FullName = parts[4] + " " + parts[3];
                                user.Email = email;
                                user.Address = "Import Address";
                                user.Phone = "12345";


                                if (!user.Organisations.Contains(organisation))
                                    user.Organisations.Add(organisation);

                                user.SetPrimaryOrganisation(organisation);
                                await _userService.ApplicationCreateUser(user);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }
        }
        public async Task ImportAOEServiceClaims(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            ClaimNotification claimNotification;
            bool readFirstLine = false;
            string line;

            //var claimFileName = "C:\\tmp\\testclientdata\\NZACSClaimsData2018.csv";
            var claimFileName = WorkingDirectory + "nzacs//NZACSClaimsData2018.csv";
            using (reader = new StreamReader(claimFileName))
            {
                while (!reader.EndOfStream)
                {
                    //if (!readFirstLine)
                    //{
                    //    line = reader.ReadLine();
                    //    readFirstLine = true;
                    //}
                    try
                    {
                        line = reader.ReadLine();
                        string[] parts = line.Split(',');
                        claimNotification = new ClaimNotification(currentUser);
                        claimNotification.ClaimMembershipNumber = parts[0];
                        claimNotification.ClaimTitle = parts[1];
                        claimNotification.ClaimReference = parts[2];
                        claimNotification.ClaimNotifiedDate = DateTime.Parse(parts[3]);
                        claimNotification.Claimant = parts[4];
                        claimNotification.ClaimStatus = parts[5];

                        await _programmeService.AddClaimNotificationByMembership(claimNotification);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportAOEServiceContract(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            BusinessContract businessContract;
            bool readFirstLine = false;
            string line;
            //special characters /,/
            //var contractFileName = "C:\\tmp\\testclientdata\\NZACSContractorsPrincipals2018.csv";
            var contractFileName = WorkingDirectory + "nzacs//NZACSContractorsPrincipals2018.csv";

            using (reader = new StreamReader(contractFileName))
            {
                while (!reader.EndOfStream)
                {
                    //if (!readFirstLine)
                    //{
                    //    line = reader.ReadLine();
                    //    readFirstLine = true;
                    //}
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        businessContract = new BusinessContract(currentUser);
                        businessContract.MembershipNumber = parts[4];
                        businessContract.ContractTitle = parts[1];
                        businessContract.Year = parts[0];
                        businessContract.ConstructionValue = parts[2];
                        businessContract.Fees = parts[3];

                        await _programmeService.AddBusinessContractByMembership(businessContract);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportActivities(User user)
        {
            var fileName = WorkingDirectory + "anzsic06completeclassification.csv";
            var currentTemplateList = await _businessActivityService.GetBusinessActivitiesTemplates();
            List<BusinessActivityTemplate> BAList = new List<BusinessActivityTemplate>();

            using (StreamReader reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    string[] parts = line.Split(';');
                    BusinessActivityTemplate ba = new BusinessActivityTemplate(user);

                    if (!string.IsNullOrEmpty(parts[0]) && !string.IsNullOrEmpty(parts[1]))
                    {
                        //classification 1
                        ba.Classification = 1;
                        ba.AnzsciCode = parts[0];
                        ba.Description = parts[1];
                    }
                    if (!string.IsNullOrEmpty(parts[1]) && !string.IsNullOrEmpty(parts[2]))
                    {
                        //classification 2
                        ba.Classification = 2;
                        ba.AnzsciCode = parts[1];
                        ba.Description = parts[2];
                    }
                    if (!string.IsNullOrEmpty(parts[2]) && !string.IsNullOrEmpty(parts[3]))
                    {
                        //classification 3
                        ba.Classification = 3;
                        ba.AnzsciCode = parts[2];
                        ba.Description = parts[3];

                    }
                    if (!string.IsNullOrEmpty(parts[3]) && !string.IsNullOrEmpty(parts[4]))
                    {
                        //classification 4
                        ba.Classification = 4;
                        ba.AnzsciCode = parts[3];
                        ba.Description = parts[4];
                    }

                    if (ba.AnzsciCode != null)
                    {
                        var test = currentTemplateList.Where(bat => bat.AnzsciCode == ba.AnzsciCode).ToList();
                        if (test.Count == 0)
                        {
                            BAList.Add(ba);
                        }
                    }
                }
            }

            foreach (BusinessActivityTemplate businessActivity in BAList)
            {
                await _businessActivityService.CreateBusinessActivityTemplate(businessActivity);
            }
        }
        public async Task ImportCEASServiceIndividuals(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "CEASClients2020.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("48ce028d-1fcb-4f3b-881b-9fd769b87643");
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = false;
            string line;
            string email;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    //if (!readFirstLine)
                    //{
                    //    line = reader.ReadLine();
                    //    readFirstLine = true;
                    //}
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    try
                    {
                        if (string.IsNullOrWhiteSpace(parts[4]))
                        {
                            email = parts[4] + "_" + parts[3] + "@techcertain.com";
                            user = await _userService.GetUserByEmail(email);
                        }
                        else
                        {
                            email = parts[4];
                        }

                        organisation = await _organisationService.GetOrganisationByEmail(email);

                        if (user == null)
                        {
                            user = new User(currentUser, Guid.NewGuid(), parts[2] + "_" + parts[3]);
                        }
                        organisation = await _organisationService.GetOrganisationByEmail(email);
                        if (parts[0] == "f")
                        {
                            if (organisation == null)
                            {
                                var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Person - Individual");
                                organisation = new Organisation(currentUser, Guid.NewGuid(), parts[2] + " " + parts[3], organisationType, email);
                                await _organisationService.CreateNewOrganisation(organisation);
                            }
                        }
                        else
                        {
                            if (organisation == null)
                            {
                                var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Corporation – Limited liability");
                                organisation = new Organisation(currentUser, Guid.NewGuid(), parts[1], organisationType, email);
                                await _organisationService.CreateNewOrganisation(organisation);
                            }
                        }

                        user.FirstName = parts[2];
                        user.LastName = parts[3];
                        user.FullName = parts[2] + " " + parts[3];
                        user.Email = email;
                        user.Address = "";
                        user.Phone = "12345";

                        if (!user.Organisations.Contains(organisation))
                            user.Organisations.Add(organisation);
                        user.SetPrimaryOrganisation(organisation);

                        await _userService.ApplicationCreateUser(user);

                        var programme = await _programmeService.GetProgramme(programmeID);
                        var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                        var reference = await _referenceService.GetLatestReferenceId();
                        var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                        await _referenceService.CreateClientInformationReference(sheet);

                        using (var uow = _unitOfWork.BeginUnitOfWork())
                        {
                            //check with ray

                            clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                            //check with ray
                            clientProgramme.ClientProgrammeMembershipNumber = parts[7];
                            sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                            try
                            {
                                await uow.Commit();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportCEASServiceUpdateUsers(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "CEASClients2019.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("48ce028d-1fcb-4f3b-881b-9fd769b87643");
            StreamReader reader;
            User user = null;
            bool readFirstLine = false;
            string line;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;

                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[8]))
                        {
                            user = await _userService.GetUserByUserName(parts[8]);
                        }

                        if (user != null)
                        {
                            using (var uow = _unitOfWork.BeginUnitOfWork())
                            {

                                user.FirstName = parts[2];
                                user.LastName = parts[3];
                                user.FullName = parts[2] + " " + parts[3];
                                try
                                {
                                    await uow.Commit();
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(ex.Message);
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportPMINZServiceIndividuals(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "PMINZClients2019Final.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("6a82f324-964f-47a6-b1cd-78848c62f616"); //PMINZ Programme ID
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    try
                    {
                        if (string.IsNullOrWhiteSpace(parts[4]))
                        {
                            email = parts[8] + "@techcertain.com";
                            user = await _userService.GetUserByEmail(email);
                        }
                        else
                        {
                            email = parts[4];
                        }

                        organisation = await _organisationService.GetOrganisationByEmail(email);

                        if (user == null)
                        {
                            user = new User(currentUser, Guid.NewGuid(), parts[8]);
                        }
                        organisation = await _organisationService.GetOrganisationByEmail(email);
                        if (parts[0] == "f")
                        {
                            if (organisation == null)
                            {
                                var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Person - Individual");
                                organisation = new Organisation(currentUser, Guid.NewGuid(), parts[2] + " " + parts[3], organisationType, email);
                                await _organisationService.CreateNewOrganisation(organisation);
                            }
                        }
                        else
                        {
                            if (organisation == null)
                            {
                                var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Corporation – Limited liability");
                                organisation = new Organisation(currentUser, Guid.NewGuid(), parts[1], organisationType, parts[4]);
                                await _organisationService.CreateNewOrganisation(organisation);
                            }
                        }

                        user.FirstName = parts[2];
                        user.LastName = parts[3];
                        user.FullName = parts[2] + " " + parts[3];
                        user.Email = email;
                        user.Address = "";
                        user.Phone = "12345";

                        if (!user.Organisations.Contains(organisation))
                            user.Organisations.Add(organisation);
                        user.SetPrimaryOrganisation(organisation);

                        await _userService.ApplicationCreateUser(user);

                        var programme = await _programmeService.GetProgramme(programmeID);
                        var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                        var reference = await _referenceService.GetLatestReferenceId();
                        var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                        await _referenceService.CreateClientInformationReference(sheet);

                        using (var uow = _unitOfWork.BeginUnitOfWork())
                        {

                            clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                            clientProgramme.ClientProgrammeMembershipNumber = parts[7];
                            sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                            try
                            {
                                await uow.Commit();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task NZFSGImportPInewUsers(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "MLPrincipalList.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("62ee21a0-802f-4c22-a46d-d677c212ba92"); //NZFSG PI FAP Programme ID
            //Guid renewFromProgrammeID = Guid.Parse("a073a11f-c0e2-4ef6-b7c9-2b3db04a6017"); //NZFSG Programme 2020 ID
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            email = parts[4];
                            orgname = parts[0];

                            organisation = await _organisationService.GetOrganisationByEmailAndName(email, orgname);

                            user = await _userService.GetUserByEmail(email);

                            if (organisation == null)
                            {
                                ClientProgramme ClientProgrammeBase = await _programmeService.GetClientProgrammebyId(Guid.Parse("a69e2609-b95d-44f8-aef5-503d1497549f"));
                                if (ClientProgrammeBase != null)
                                {
                                    var unit = (AdvisorUnit)organisation.OrganisationalUnits.FirstOrDefault(u => u.Name == "Advisor");
                                    if (unit == null)
                                    {
                                        OrganisationType advisorType = new OrganisationType("Person - Individual");
                                        InsuranceAttribute advisorAttribute = new InsuranceAttribute(currentUser, "Advisor");
                                        OrganisationalUnit defaultUnit = new OrganisationalUnit(currentUser, "Person - Individual", "Person - Individual", null);
                                        AdvisorUnit AdvisorUnit = new AdvisorUnit(currentUser, "Advisor", "Person - Individual", null)
                                        {
                                            IsPrincipalAdvisor = true
                                        };
                                        Organisation Advisor = new Organisation(currentUser, Guid.NewGuid())
                                        {
                                            OrganisationType = advisorType,
                                            Email = user.Email,
                                            Name = user.FullName
                                        };

                                        Advisor.InsuranceAttributes.Add(advisorAttribute);
                                        Advisor.OrganisationalUnits.Add(defaultUnit);
                                        Advisor.OrganisationalUnits.Add(AdvisorUnit);
                                        var sheet = await _clientInformationService.GetInformation(Guid.Parse("a69e2609-b95d-44f8-aef5-503d1497549f"));
                                        sheet.Organisation.Add(Advisor);
                                    }
                                    //Create a renew
                                }
                            }
                            else
                            {
                                if (organisation != null)
                                {
                                    var sheet = await _clientInformationService.GetInformation(Guid.Parse("a69e2609-b95d-44f8-aef5-503d1497549f"));
                                    sheet.Organisation.Add(organisation);
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportNZFSGServicePI(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "piupload.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("624399aa-6366-4165-a470-289fead38563"); //NZFSG PI FAP Programme ID
            Guid renewFromProgrammeID = Guid.Parse("a073a11f-c0e2-4ef6-b7c9-2b3db04a6017"); //NZFSG Programme 2020 ID
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            email = parts[1];
                            orgname = parts[0];

                            organisation = await _organisationService.GetOrganisationByEmailAndName(email, orgname);

                            user = await _userService.GetUserByEmail(email);

                            if (user != null && organisation != null)
                            {
                                ClientProgramme renewFromClientProgrammeBase = await _programmeService.GetOriginalClientProgrammeByOwnerByProgramme(organisation.Id, renewFromProgrammeID);
                                if (renewFromClientProgrammeBase != null)
                                {
                                    //Create a renew
                                    ClientProgramme CloneProgramme = await _programmeService.CloneForRenew(user, renewFromClientProgrammeBase.Id, programmeID);
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }


        public async Task ImportApolloServicePI(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "apollomemberupload2021pi.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("949b5d0b-2d7c-491a-b1a5-70bee2b5bebd"); //Apollo PI FAP Programme ID
            Guid renewFromProgrammeID = Guid.Parse("633b32f7-93bd-4ed1-9f7e-088ae5312b98"); //Apollo Programme 2020 ID
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            string referenceNum;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    referenceNum = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[0]))
                        {
                            referenceNum = parts[0];
                            if (referenceNum != null)
                            {
                                ClientProgramme renewFromClientProgrammeBase = await _programmeService.GetOriginalClientProgrammeByReferenceNum(referenceNum);
                                if (renewFromClientProgrammeBase != null)
                                {
                                    ClientProgramme CloneProgramme = await _programmeService.CloneForRenew(user, renewFromClientProgrammeBase.Id, programmeID);
                                    CloneProgramme.ClientProgrammeMembershipNumber = referenceNum;


                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportNZFSGServicePINewCompany(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "piuploadnewcompany.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("624399aa-6366-4165-a470-289fead38563"); //NZFSG PI FAP Programme ID
            Guid renewFromProgrammeID = Guid.Parse("a073a11f-c0e2-4ef6-b7c9-2b3db04a6017"); //NZFSG Programme 2020 ID
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            email = parts[1];
                            orgname = parts[0];

                            organisation = await _organisationService.GetOrganisationByEmailAndName(email, orgname);

                            user = await _userService.GetUserByEmail(email);

                            if (user != null && organisation != null)
                            {
                                ClientProgramme renewFromClientProgrammeBase = await _programmeService.GetOriginalClientProgrammeByOwnerByProgramme(organisation.Id, renewFromProgrammeID);
                                if (renewFromClientProgrammeBase != null)
                                {
                                    //Create a renew clientprogramme and UIS
                                    ClientProgramme CloneProgramme = await _programmeService.CloneForRenew(user, renewFromClientProgrammeBase.Id, programmeID);
                                }
                            }
                            else if (user != null && organisation == null)
                            {
                                //Create a new org and create a new clientprogramme and UIS
                                if (organisation == null)
                                {
                                    var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Corporation – Limited liability");
                                    if (organisationType == null)
                                    {
                                        organisationType = await _organisationTypeService.CreateNewOrganisationType(currentUser, "Corporation – Limited liability");
                                    }

                                    organisation = new Organisation(currentUser, Guid.NewGuid(), orgname, organisationType, email);
                                    await _organisationService.CreateNewOrganisation(organisation);
                                }

                                if (!user.Organisations.Contains(organisation))
                                    user.Organisations.Add(organisation);

                                await _userService.Update(user);

                                var programme = await _programmeService.GetProgramme(programmeID);
                                var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                                var reference = await _referenceService.GetLatestReferenceId();
                                var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                                await _referenceService.CreateClientInformationReference(sheet);

                                using (var uow = _unitOfWork.BeginUnitOfWork())
                                {
                                    clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                                    sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                                    try
                                    {
                                        await uow.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message);
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportFANZImportML(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "clientlistmlro.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("4b7eefbd-910f-4a9b-b56e-5c425afa4608"); //FANZ ML Programme ID
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            string username;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    username = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[3]) && !string.IsNullOrWhiteSpace(parts[5]))
                        {
                            email = parts[3];
                            orgname = parts[0];
                            username = parts[5];

                            organisation = await _organisationService.GetOrganisationByEmailAndName(email, orgname);

                            user = await _userService.GetUserByUserName(username);

                            if (user == null)
                            {
                                user = new User(currentUser, Guid.NewGuid(), username);
                                user.FirstName = parts[1].Trim();
                                user.LastName = parts[2].Trim();
                                user.FullName = parts[1].Trim() + " " + parts[2].Trim();
                                user.Email = email;
                                user.Address = "";
                                user.Phone = "12345";
                            }

                            if (organisation == null)
                            {
                                var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Corporation – Limited liability");
                                if (organisationType == null)
                                {
                                    organisationType = await _organisationTypeService.CreateNewOrganisationType(currentUser, "Corporation – Limited liability");
                                }

                                organisation = new Organisation(currentUser, Guid.NewGuid(), orgname, organisationType, email);
                                await _organisationService.CreateNewOrganisation(organisation);
                            }

                            if (!user.Organisations.Contains(organisation))
                                user.Organisations.Add(organisation);

                            await _userService.Update(user);

                            var programme = await _programmeService.GetProgramme(programmeID);
                            var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                            var reference = await _referenceService.GetLatestReferenceId();
                            var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                            await _referenceService.CreateClientInformationReference(sheet);

                            using (var uow = _unitOfWork.BeginUnitOfWork())
                            {
                                clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                                clientProgramme.ClientProgrammeMembershipNumber = parts[4];
                                sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                                try
                                {
                                    await uow.Commit();
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(ex.Message);
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportFANZImportRO(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "clientlistmlro.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("352a24bd-4b8f-4f69-8c2e-38ab10a389df"); //FANZ Run Off Programme ID
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            string username;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    username = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[3]) && !string.IsNullOrWhiteSpace(parts[5]))
                        {
                            email = parts[3];
                            orgname = parts[0];
                            username = parts[5];

                            organisation = await _organisationService.GetOrganisationByEmailAndName(email, orgname);

                            user = await _userService.GetUserByUserName(username);

                            if (user == null)
                            {
                                user = new User(currentUser, Guid.NewGuid(), username);
                                user.FirstName = parts[1].Trim();
                                user.LastName = parts[2].Trim();
                                user.FullName = parts[1].Trim() + " " + parts[2].Trim();
                                user.Email = email;
                                user.Address = "";
                                user.Phone = "12345";
                            }

                            if (organisation == null)
                            {
                                var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Corporation – Limited liability");
                                if (organisationType == null)
                                {
                                    organisationType = await _organisationTypeService.CreateNewOrganisationType(currentUser, "Corporation – Limited liability");
                                }

                                organisation = new Organisation(currentUser, Guid.NewGuid(), orgname, organisationType, email);
                                await _organisationService.CreateNewOrganisation(organisation);
                            }

                            if (!user.Organisations.Contains(organisation))
                                user.Organisations.Add(organisation);

                            await _userService.Update(user);

                            var programme = await _programmeService.GetProgramme(programmeID);
                            var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                            var reference = await _referenceService.GetLatestReferenceId();
                            var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                            await _referenceService.CreateClientInformationReference(sheet);

                            using (var uow = _unitOfWork.BeginUnitOfWork())
                            {
                                clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                                clientProgramme.ClientProgrammeMembershipNumber = parts[4];
                                sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                                try
                                {
                                    await uow.Commit();
                                }
                                catch (Exception ex)
                                {
                                    throw new Exception(ex.Message);
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportFANZMLPreRenewData(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            PreRenewOrRefData preRenewOrRefData;
            Guid programmeID = Guid.Parse("4b7eefbd-910f-4a9b-b56e-5c425afa4608"); //FANZ ML Programme ID
            bool readFirstLine = true;
            string line;
            var fileName = WorkingDirectory + "renewaldata.csv";
            Programme programme = await _programmeService.GetProgramme(programmeID);

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        preRenewOrRefData = new PreRenewOrRefData(currentUser, parts[1], parts[0]);
                        if (!string.IsNullOrEmpty(parts[2]))
                            preRenewOrRefData.CLRetro = parts[2];
                        if (!string.IsNullOrEmpty(parts[3]))
                            preRenewOrRefData.PIBoundLimit = parts[3];
                        if (!string.IsNullOrEmpty(parts[4]))
                            preRenewOrRefData.PIBoundExcess = parts[4];
                        if (!string.IsNullOrEmpty(parts[5]))
                            preRenewOrRefData.PIBoundPremium = parts[5];
                        if (!string.IsNullOrEmpty(parts[6]))
                            preRenewOrRefData.EndorsementProduct = parts[6];
                        if (!string.IsNullOrEmpty(parts[7]))
                            preRenewOrRefData.EndorsementTitle = parts[7];
                        if (!string.IsNullOrEmpty(parts[8]))
                            preRenewOrRefData.EndorsementText = parts[8];

                        await _programmeService.AddPreRenewOrRefDataByMembershipAndProgramme(preRenewOrRefData, programme);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportFANZPIPreRenewData(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            PreRenewOrRefData preRenewOrRefData;
            Guid programmeID = Guid.Parse("62aea93b-8f7e-4554-b037-bb6726bc3c2d"); //FANZ PI FAP Programme ID
            bool readFirstLine = true;
            string line;
            var fileName = WorkingDirectory + "renewaldata.csv";
            Programme programme = await _programmeService.GetProgramme(programmeID);

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        preRenewOrRefData = new PreRenewOrRefData(currentUser, parts[1], parts[0]);
                        if (!string.IsNullOrEmpty(parts[2]))
                            preRenewOrRefData.CLRetro = parts[2];
                        if (!string.IsNullOrEmpty(parts[3]))
                            preRenewOrRefData.PIBoundLimit = parts[3];
                        if (!string.IsNullOrEmpty(parts[4]))
                            preRenewOrRefData.PIBoundExcess = parts[4];
                        if (!string.IsNullOrEmpty(parts[5]))
                            preRenewOrRefData.PIBoundPremium = parts[5];
                        //if (!string.IsNullOrEmpty(parts[6]))
                        //    preRenewOrRefData.EndorsementProduct = parts[6];
                        //if (!string.IsNullOrEmpty(parts[7]))
                        //    preRenewOrRefData.EndorsementTitle = parts[7];
                        //if (!string.IsNullOrEmpty(parts[8]))
                        //    preRenewOrRefData.EndorsementText = parts[8];

                        await _programmeService.AddPreRenewOrRefDataByMembershipAndProgramme(preRenewOrRefData, programme);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportFANZROPreRenewData(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            PreRenewOrRefData preRenewOrRefData;
            Guid programmeID = Guid.Parse("352a24bd-4b8f-4f69-8c2e-38ab10a389df"); //FANZ Run Off Programme ID
            bool readFirstLine = true;
            string line;
            var fileName = WorkingDirectory + "renewaldata.csv";
            Programme programme = await _programmeService.GetProgramme(programmeID);

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        preRenewOrRefData = new PreRenewOrRefData(currentUser, parts[1], parts[0]);
                        if (!string.IsNullOrEmpty(parts[2]))
                            preRenewOrRefData.CLRetro = parts[2];
                        if (!string.IsNullOrEmpty(parts[3]))
                            preRenewOrRefData.PIBoundLimit = parts[3];
                        if (!string.IsNullOrEmpty(parts[4]))
                            preRenewOrRefData.PIBoundExcess = parts[4];
                        if (!string.IsNullOrEmpty(parts[5]))
                            preRenewOrRefData.PIBoundPremium = parts[5];
                        //if (!string.IsNullOrEmpty(parts[6]))
                        //    preRenewOrRefData.EndorsementProduct = parts[6];
                        //if (!string.IsNullOrEmpty(parts[7]))
                        //    preRenewOrRefData.EndorsementTitle = parts[7];
                        //if (!string.IsNullOrEmpty(parts[8]))
                        //    preRenewOrRefData.EndorsementText = parts[8];

                        await _programmeService.AddPreRenewOrRefDataByMembershipAndProgramme(preRenewOrRefData, programme);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportNZFSGServicePINewAll(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "piuploadnewall.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("624399aa-6366-4165-a470-289fead38563"); //NZFSG PI FAP Programme ID
            Guid renewFromProgrammeID = Guid.Parse("a073a11f-c0e2-4ef6-b7c9-2b3db04a6017"); //NZFSG Programme 2020 ID
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            email = parts[1];
                            orgname = parts[0];

                            organisation = await _organisationService.GetOrganisationByEmailAndName(email, orgname);

                            user = await _userService.GetUserByEmail(email);

                            if (user != null && organisation != null)
                            {
                                ClientProgramme renewFromClientProgrammeBase = await _programmeService.GetOriginalClientProgrammeByOwnerByProgramme(organisation.Id, renewFromProgrammeID);
                                if (renewFromClientProgrammeBase != null)
                                {
                                    //Create a renew clientprogramme and UIS
                                    ClientProgramme CloneProgramme = await _programmeService.CloneForRenew(user, renewFromClientProgrammeBase.Id, programmeID);
                                }
                            }
                            else if (user != null && organisation == null)
                            {
                                //Create a new org and create a new clientprogramme and UIS
                                if (organisation == null)
                                {
                                    var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Corporation – Limited liability");
                                    if (organisationType == null)
                                    {
                                        organisationType = await _organisationTypeService.CreateNewOrganisationType(currentUser, "Corporation – Limited liability");
                                    }

                                    organisation = new Organisation(currentUser, Guid.NewGuid(), orgname, organisationType, email);
                                    await _organisationService.CreateNewOrganisation(organisation);
                                }

                                if (!user.Organisations.Contains(organisation))
                                    user.Organisations.Add(organisation);

                                await _userService.Update(user);

                                var programme = await _programmeService.GetProgramme(programmeID);
                                var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                                var reference = await _referenceService.GetLatestReferenceId();
                                var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                                await _referenceService.CreateClientInformationReference(sheet);

                                using (var uow = _unitOfWork.BeginUnitOfWork())
                                {
                                    clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                                    sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                                    try
                                    {
                                        await uow.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message);
                                    }
                                }
                            }
                            else if (user == null && organisation == null)
                            {
                                //Create a new org, a new user and create a new clientprogramme and UIS
                                Random rand = new Random();
                                string username = parts[2].Trim() + "_" + parts[3].Trim() + rand.Next(100);
                                user = new User(currentUser, Guid.NewGuid(), username);
                                user.FirstName = parts[2];
                                user.LastName = parts[3];
                                user.FullName = parts[2].Trim() + " " + parts[3].Trim();
                                user.Email = email;
                                user.Address = "";
                                user.Phone = "12345";

                                if (organisation == null)
                                {
                                    var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Corporation – Limited liability");
                                    if (organisationType == null)
                                    {
                                        organisationType = await _organisationTypeService.CreateNewOrganisationType(currentUser, "Corporation – Limited liability");
                                    }

                                    organisation = new Organisation(currentUser, Guid.NewGuid(), orgname, organisationType, email);
                                    await _organisationService.CreateNewOrganisation(organisation);
                                }


                                if (!user.Organisations.Contains(organisation))
                                    user.Organisations.Add(organisation);
                                user.SetPrimaryOrganisation(organisation);

                                await _userService.ApplicationCreateUser(user);

                                var programme = await _programmeService.GetProgramme(programmeID);
                                var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                                var reference = await _referenceService.GetLatestReferenceId();
                                var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                                await _referenceService.CreateClientInformationReference(sheet);

                                using (var uow = _unitOfWork.BeginUnitOfWork())
                                {
                                    clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                                    sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                                    try
                                    {
                                        await uow.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message);
                                    }
                                }


                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportNZFSGServiceML(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "mlupload.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("4fec62eb-f1dd-478e-92df-9b5647249e4c"); //NZFSG ML Programme ID
            Guid renewFromProgrammeID = Guid.Parse("a073a11f-c0e2-4ef6-b7c9-2b3db04a6017"); //NZFSG Programme 2020 ID
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            email = parts[1];
                            orgname = parts[0];

                            organisation = await _organisationService.GetOrganisationByEmailAndName(email, orgname);

                            user = await _userService.GetUserByEmail(email);

                            if (user != null && organisation != null)
                            {
                                ClientProgramme renewFromClientProgrammeBase = await _programmeService.GetOriginalClientProgrammeByOwnerByProgramme(organisation.Id, renewFromProgrammeID);
                                if (renewFromClientProgrammeBase != null)
                                {
                                    //Create a renew
                                    ClientProgramme CloneProgramme = await _programmeService.CloneForRenew(user, renewFromClientProgrammeBase.Id, programmeID);
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

             public async Task ImportApolloServiceRO(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "mlupload.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("4fec62eb-f1dd-478e-92df-9b5647249e4c");
            Guid renewFromProgrammeID = Guid.Parse("a073a11f-c0e2-4ef6-b7c9-2b3db04a6017");
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            string referenceNum;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    referenceNum = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[0]))
                        {
                            referenceNum = parts[0];
                            if (user != null && organisation != null)
                            {
                                ClientProgramme renewFromClientProgrammeBase = await _programmeService.GetOriginalClientProgrammeByReferenceNum(referenceNum);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportApolloServiceML(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "mlupload.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("4fec62eb-f1dd-478e-92df-9b5647249e4c");
            Guid renewFromProgrammeID = Guid.Parse("a073a11f-c0e2-4ef6-b7c9-2b3db04a6017");
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            string referenceNum;

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    referenceNum = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[0]))
                        {
                            referenceNum = parts[0];
                            if (user != null && organisation != null)
                            {
                                ClientProgramme renewFromClientProgrammeBase = await _programmeService.GetOriginalClientProgrammeByReferenceNum(referenceNum);
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportNZFSGServiceMLNewCompany(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "mluploadnewcompany.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("4fec62eb-f1dd-478e-92df-9b5647249e4c"); //NZFSG ML Programme ID
            Guid renewFromProgrammeID = Guid.Parse("a073a11f-c0e2-4ef6-b7c9-2b3db04a6017"); //NZFSG Programme 2020 ID
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            email = parts[1];
                            orgname = parts[0];

                            organisation = await _organisationService.GetOrganisationByEmailAndName(email, orgname);

                            user = await _userService.GetUserByEmail(email);

                            if (user != null && organisation != null)
                            {
                                ClientProgramme renewFromClientProgrammeBase = await _programmeService.GetOriginalClientProgrammeByOwnerByProgramme(organisation.Id, renewFromProgrammeID);
                                if (renewFromClientProgrammeBase != null)
                                {
                                    //Create a renew clientprogramme and UIS
                                    ClientProgramme CloneProgramme = await _programmeService.CloneForRenew(user, renewFromClientProgrammeBase.Id, programmeID);
                                }
                            }
                            else if (user != null && organisation == null)
                            {
                                //Create a new org and create a new clientprogramme and UIS
                                if (organisation == null)
                                {
                                    var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Corporation – Limited liability");
                                    if (organisationType == null)
                                    {
                                        organisationType = await _organisationTypeService.CreateNewOrganisationType(currentUser, "Corporation – Limited liability");
                                    }

                                    organisation = new Organisation(currentUser, Guid.NewGuid(), orgname, organisationType, email);
                                    await _organisationService.CreateNewOrganisation(organisation);
                                }

                                if (!user.Organisations.Contains(organisation))
                                    user.Organisations.Add(organisation);

                                await _userService.Update(user);

                                var programme = await _programmeService.GetProgramme(programmeID);
                                var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                                var reference = await _referenceService.GetLatestReferenceId();
                                var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                                await _referenceService.CreateClientInformationReference(sheet);

                                using (var uow = _unitOfWork.BeginUnitOfWork())
                                {
                                    clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                                    sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                                    try
                                    {
                                        await uow.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message);
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportNZFSGServiceMLNewAll(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "mluploadnewall.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("4fec62eb-f1dd-478e-92df-9b5647249e4c"); //NZFSG ML Programme ID
            Guid renewFromProgrammeID = Guid.Parse("a073a11f-c0e2-4ef6-b7c9-2b3db04a6017"); //NZFSG Programme 2020 ID
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            email = parts[1];
                            orgname = parts[0];

                            organisation = await _organisationService.GetOrganisationByEmailAndName(email, orgname);

                            user = await _userService.GetUserByEmail(email);

                            if (user != null && organisation != null)
                            {
                                ClientProgramme renewFromClientProgrammeBase = await _programmeService.GetOriginalClientProgrammeByOwnerByProgramme(organisation.Id, renewFromProgrammeID);
                                if (renewFromClientProgrammeBase != null)
                                {
                                    //Create a renew clientprogramme and UIS
                                    ClientProgramme CloneProgramme = await _programmeService.CloneForRenew(user, renewFromClientProgrammeBase.Id, programmeID);
                                }
                            }
                            else if (user != null && organisation == null)
                            {
                                //Create a new org and create a new clientprogramme and UIS
                                if (organisation == null)
                                {
                                    var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Corporation – Limited liability");
                                    if (organisationType == null)
                                    {
                                        organisationType = await _organisationTypeService.CreateNewOrganisationType(currentUser, "Corporation – Limited liability");
                                    }

                                    organisation = new Organisation(currentUser, Guid.NewGuid(), orgname, organisationType, email);
                                    await _organisationService.CreateNewOrganisation(organisation);
                                }

                                if (!user.Organisations.Contains(organisation))
                                    user.Organisations.Add(organisation);

                                await _userService.Update(user);

                                var programme = await _programmeService.GetProgramme(programmeID);
                                var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                                var reference = await _referenceService.GetLatestReferenceId();
                                var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                                await _referenceService.CreateClientInformationReference(sheet);

                                using (var uow = _unitOfWork.BeginUnitOfWork())
                                {
                                    clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                                    sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                                    try
                                    {
                                        await uow.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message);
                                    }
                                }
                            }
                            else if (user == null && organisation == null)
                            {
                                //Create a new org, a new user and create a new clientprogramme and UIS
                                Random rand = new Random();
                                string username = parts[2].Trim() + "_" + parts[3].Trim() + rand.Next(100);
                                user = new User(currentUser, Guid.NewGuid(), username);
                                user.FirstName = parts[2];
                                user.LastName = parts[3];
                                user.FullName = parts[2].Trim() + " " + parts[3].Trim();
                                user.Email = email;
                                user.Address = "";
                                user.Phone = "12345";

                                if (organisation == null)
                                {
                                    var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Corporation – Limited liability");
                                    if (organisationType == null)
                                    {
                                        organisationType = await _organisationTypeService.CreateNewOrganisationType(currentUser, "Corporation – Limited liability");
                                    }

                                    organisation = new Organisation(currentUser, Guid.NewGuid(), orgname, organisationType, email);
                                    await _organisationService.CreateNewOrganisation(organisation);
                                }


                                if (!user.Organisations.Contains(organisation))
                                    user.Organisations.Add(organisation);
                                user.SetPrimaryOrganisation(organisation);

                                await _userService.ApplicationCreateUser(user);

                                var programme = await _programmeService.GetProgramme(programmeID);
                                var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                                var reference = await _referenceService.GetLatestReferenceId();
                                var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                                await _referenceService.CreateClientInformationReference(sheet);

                                using (var uow = _unitOfWork.BeginUnitOfWork())
                                {
                                    clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                                    sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                                    try
                                    {
                                        await uow.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message);
                                    }
                                }


                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }



        public async Task ImportNZFSGServiceRO(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "roupload.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("33606edd-a26f-4352-9d61-c219d1316cef"); //NZFSG Run Off Programme ID
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[0]) && !string.IsNullOrWhiteSpace(parts[1]))
                        {
                            email = parts[1];
                            orgname = parts[0];

                            organisation = await _organisationService.GetOrganisationByEmailAndName(email, orgname);

                            user = await _userService.GetUserByEmail(email);

                            if (user != null)
                            {
                                if (organisation == null)
                                {
                                    var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Person - Individual");
                                    if (organisationType == null)
                                    {
                                        organisationType = await _organisationTypeService.CreateNewOrganisationType(currentUser, "Person - Individual");
                                    }

                                    organisation = new Organisation(currentUser, Guid.NewGuid(), orgname, organisationType, email);
                                    await _organisationService.CreateNewOrganisation(organisation);
                                }

                                if (!user.Organisations.Contains(organisation))
                                    user.Organisations.Add(organisation);

                                await _userService.Update(user);

                                var programme = await _programmeService.GetProgramme(programmeID);
                                var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                                var reference = await _referenceService.GetLatestReferenceId();
                                var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                                await _referenceService.CreateClientInformationReference(sheet);

                                using (var uow = _unitOfWork.BeginUnitOfWork())
                                {
                                    clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                                    sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                                    try
                                    {
                                        await uow.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message);
                                    }
                                }

                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportApolloServiceROUIS(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "apollomemberupload2021mlro.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("0cc0d9b6-183b-4323-999a-a50a6b3a5510");
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            string eglobalnum;
            string membershipnumber;
            string fname;
            string lname;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    eglobalnum = "";
                    membershipnumber = "";
                    fname = "";
                    lname = "";
                    string userName = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[7]) && !string.IsNullOrWhiteSpace(parts[6]))
                        {
                            email = parts[8];
                            orgname = parts[5];
                            eglobalnum = parts[2];
                            membershipnumber = parts[1];
                            fname = parts[6];
                            lname = parts[7];
                            organisation = await _organisationService.GetOrganisationByEmailAndName(email, orgname);

                            user = await _userService.GetUserByEmail(email);
                            if (user == null)
                            {
                                userName = fname + "_" + lname;
                                try
                                {
                                    user = await _userService.GetUser(userName);
                                }
                                catch (Exception ex)
                                {
                                    Random random = new Random();
                                    int randomNumber = random.Next(10, 99);
                                    userName = userName + randomNumber.ToString();
                                }
                                user = new User(currentUser, Guid.NewGuid(), userName);
                                user.FirstName = fname;
                                user.LastName = lname;
                                user.FullName = fname + " " + lname;
                                user.Email = email;

                            }

                            if (user != null)
                            {
                                if (organisation == null)
                                {
                                    var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Person - Individual");
                                    if (organisationType == null)
                                    {
                                        organisationType = await _organisationTypeService.CreateNewOrganisationType(currentUser, "Person - Individual");
                                    }

                                    organisation = new Organisation(currentUser, Guid.NewGuid(), orgname, organisationType, email);
                                    await _organisationService.CreateNewOrganisation(organisation);
                                }

                                if (!user.Organisations.Contains(organisation))
                                    user.Organisations.Add(organisation);

                                await _userService.Update(user);

                                var programme = await _programmeService.GetProgramme(programmeID);
                                var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                                var reference = await _referenceService.GetLatestReferenceId();
                                var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                                await _referenceService.CreateClientInformationReference(sheet);

                                using (var uow = _unitOfWork.BeginUnitOfWork())
                                {
                                    clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                                    sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                                    clientProgramme.EGlobalClientNumber = eglobalnum;
                                    clientProgramme.ClientProgrammeMembershipNumber = membershipnumber;
                                    try
                                    {
                                        await uow.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message);
                                    }
                                }

                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportApolloServicePIUIS(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "apollomemberupload2021piadvisor.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("949b5d0b-2d7c-491a-b1a5-70bee2b5bebd");
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            string eglobalnum;
            string membershipnumber;
            string fname;
            string lname;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    eglobalnum = "";
                    membershipnumber = "";
                    fname = "";
                    lname = "";
                    string userName = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[7]) && !string.IsNullOrWhiteSpace(parts[6]))
                        {
                            email = parts[8];
                            orgname = parts[5];
                            eglobalnum = parts[2];
                            membershipnumber = parts[1];
                            fname = parts[6];
                            lname = parts[7];
                            organisation = await _organisationService.GetOrganisationByEmailAndName(email, orgname);

                            user = await _userService.GetUserByEmail(email);
                            if (user == null)
                            {
                                userName = fname + "_" + lname;
                                try
                                {
                                    user = await _userService.GetUser(userName);
                                }
                                catch (Exception ex)
                                {
                                    Random random = new Random();
                                    int randomNumber = random.Next(10, 99);
                                    userName = userName + randomNumber.ToString();
                                }
                                user = new User(currentUser, Guid.NewGuid(), userName);
                                user.FirstName = fname;
                                user.LastName = lname;
                                user.FullName = fname + " " + lname;
                                user.Email = email;

                            }

                            if (user != null)
                            {
                                if (organisation == null)
                                {
                                    var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Person - Individual");
                                    if (organisationType == null)
                                    {
                                        organisationType = await _organisationTypeService.CreateNewOrganisationType(currentUser, "Person - Individual");
                                    }

                                    organisation = new Organisation(currentUser, Guid.NewGuid(), orgname, organisationType, email);
                                    await _organisationService.CreateNewOrganisation(organisation);
                                }
 
                                if (!user.Organisations.Contains(organisation))
                                    user.Organisations.Add(organisation);

                                await _userService.Update(user);

                                var programme = await _programmeService.GetProgramme(programmeID);
                                //var clientProgramme = await _programmeService.get(membershipnumber);

                                using (var uow =  _unitOfWork.BeginUnitOfWork())
                                {
                                    var result = await _programmeService.AddOrganisationByMembershipByProgram(organisation, membershipnumber, programmeID);
                                    await uow.Commit();
                                }

                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportApolloServiceMLUIS(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "apollomemberupload2021mlro.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("8d026bbe-3750-4bc9-ac7d-e82de6b48d58");
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string orgname;
            string eglobalnum;
            string membershipnumber;
            string fname;
            string lname;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    orgname = "";
                    eglobalnum = "";
                    membershipnumber = "";
                    fname = "";
                    lname = "";
                    string userName = "";
                    try
                    {
                        if (!string.IsNullOrWhiteSpace(parts[7]) && !string.IsNullOrWhiteSpace(parts[6]))
                        {
                            email = parts[8];
                            orgname = parts[5];
                            eglobalnum = parts[2];
                            membershipnumber = parts[1];
                            fname = parts[6];
                            lname = parts[7];
                            organisation = await _organisationService.GetOrganisationByEmailAndName(email, orgname);

                            user = await _userService.GetUserByEmail(email);
                            if(user == null)
                            {
                                    userName = fname + "_" + lname;
                                    try
                                    {
                                        user = await _userService.GetUser(userName);
                                    }
                                    catch (Exception ex)
                                    {
                                        Random random = new Random();
                                        int randomNumber = random.Next(10, 99);
                                        userName = userName + randomNumber.ToString();
                                    }
                                    user = new User(currentUser, Guid.NewGuid(), userName);
                                    user.FirstName = fname;
                                    user.LastName = lname;
                                    user.FullName = fname + " " + lname;
                                    user.Email = email;
                                   
                            }

                            if (user != null)
                            {
                                if (organisation == null)
                                {
                                    var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Person - Individual");
                                    if (organisationType == null)
                                    {
                                        organisationType = await _organisationTypeService.CreateNewOrganisationType(currentUser, "Person - Individual");
                                    }

                                    organisation = new Organisation(currentUser, Guid.NewGuid(), orgname, organisationType, email);
                                    await _organisationService.CreateNewOrganisation(organisation);
                                }

                                if (!user.Organisations.Contains(organisation))
                                    user.Organisations.Add(organisation);

                                await _userService.Update(user);

                                var programme = await _programmeService.GetProgramme(programmeID);
                                var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                                var reference = await _referenceService.GetLatestReferenceId();
                                var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                                await _referenceService.CreateClientInformationReference(sheet);

                                using (var uow = _unitOfWork.BeginUnitOfWork())
                                {
                                    clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                                    sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                                    clientProgramme.EGlobalClientNumber = eglobalnum;
                                    clientProgramme.ClientProgrammeMembershipNumber = membershipnumber;
                                    try
                                    {
                                        await uow.Commit();
                                    }
                                    catch (Exception ex)
                                    {
                                        throw new Exception(ex.Message);
                                    }
                                }

                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportAbbottImportOwners(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;

            bool readFirstLine = false;
            string line;
            Guid.TryParse("44d385ee-1e64-403e-8470-a800d940e2f3", out Guid ProgrammeId);
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "AbbottMember.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        string type = "";
                        string Name = "";
                        User user = new User(currentUser, Guid.NewGuid())
                        {
                            FirstName = parts[2],
                            LastName = parts[3],
                            FullName = parts[2] + " " + parts[3],
                            Email = parts[4],
                            UserName = parts[6]
                        };
                        if (parts[0] == "t" && !string.IsNullOrEmpty(parts[1]))
                        {
                            type = "Corporation – Limited liability";
                            Name = parts[1];
                        }
                        else
                        {
                            type = "Person - Individual";
                            Name = user.FullName;
                        }

                        OrganisationType ownerType = new OrganisationType(type);
                        InsuranceAttribute ownerAttribute = new InsuranceAttribute(currentUser, type);
                        OrganisationalUnit ownerUnit = new OrganisationalUnit(currentUser, type, "Head Office", null);
                        Organisation Owner = new Organisation(currentUser, Guid.NewGuid())
                        {
                            OrganisationType = ownerType,
                            Email = user.Email,
                            Name = Name
                        };

                        Owner.OrganisationalUnits.Add(ownerUnit);
                        Owner.InsuranceAttributes.Add(ownerAttribute);
                        user.Organisations.Add(Owner);

                        OrganisationType advisorType = new OrganisationType("Person - Individual");
                        InsuranceAttribute advisorAttribute = new InsuranceAttribute(currentUser, "Advisor");
                        OrganisationalUnit defaultUnit = new OrganisationalUnit(currentUser, "Person - Individual", "Person - Individual", null);
                        AdvisorUnit AdvisorUnit = new AdvisorUnit(currentUser, "Advisor", "Person - Individual", null)
                        {
                            IsPrincipalAdvisor = true
                        };
                        Organisation Advisor = new Organisation(currentUser, Guid.NewGuid())
                        {
                            OrganisationType = advisorType,
                            Email = user.Email,
                            Name = user.FullName
                        };

                        Advisor.InsuranceAttributes.Add(advisorAttribute);
                        Advisor.OrganisationalUnits.Add(defaultUnit);
                        Advisor.OrganisationalUnits.Add(AdvisorUnit);
                        user.Organisations.Add(Advisor);
                        user.SetPrimaryOrganisation(Owner);
                        await _userService.ApplicationCreateUser(user);

                        var programme = await _programmeService.GetProgramme(ProgrammeId);
                        var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, Owner);

                        var reference = await _referenceService.GetLatestReferenceId();
                        var sheet = await _clientInformationService.IssueInformationFor(user, Owner, clientProgramme, reference);
                        await _referenceService.CreateClientInformationReference(sheet);
                        clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                        clientProgramme.ClientProgrammeMembershipNumber = parts[5];
                        sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                        sheet.Organisation.Add(Advisor);
                        await _programmeService.Update(clientProgramme);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }
        }

        public async Task ImportFanzOwners(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;

            bool readFirstLine = true;
            string line;
            Guid.TryParse("62aea93b-8f7e-4554-b037-bb6726bc3c2d", out Guid ProgrammeId);
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "fanzpalist.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        string type = "";
                        string Name = "";
                        User user = await _userService.GetUserByUserName(parts[5]);///username
                        if (user != null)
                        {

                            type = "Corporation – Limited liability";
                            Name = parts[6];/////use fap name

                            OrganisationType ownerType = new OrganisationType(type);
                            InsuranceAttribute ownerAttribute = new InsuranceAttribute(currentUser, type);
                            OrganisationalUnit ownerUnit = new OrganisationalUnit(currentUser, type, "Head Office", null);
                            Organisation Owner = new Organisation(currentUser, Guid.NewGuid())
                            {
                                OrganisationType = ownerType,
                                Email = user.Email,
                                Name = Name
                            };

                            Owner.OrganisationalUnits.Add(ownerUnit);
                            Owner.InsuranceAttributes.Add(ownerAttribute);
                            user.Organisations.Add(Owner);

                            OrganisationType advisorType = new OrganisationType("Person - Individual");
                            InsuranceAttribute advisorAttribute = new InsuranceAttribute(currentUser, "Advisor");
                            OrganisationalUnit defaultUnit = new OrganisationalUnit(currentUser, "Person - Individual", "Person - Individual", null);
                            AdvisorUnit AdvisorUnit = new AdvisorUnit(currentUser, "Advisor", "Person - Individual", null)
                            {
                                IsPrincipalAdvisor = true
                            };
                            Organisation Advisor = new Organisation(currentUser, Guid.NewGuid())
                            {
                                OrganisationType = advisorType,
                                Email = user.Email,
                                Name = user.FullName
                            };

                            Advisor.InsuranceAttributes.Add(advisorAttribute);
                            Advisor.OrganisationalUnits.Add(defaultUnit);
                            Advisor.OrganisationalUnits.Add(AdvisorUnit);
                            user.Organisations.Add(Advisor);
                            user.SetPrimaryOrganisation(Owner);
                            await _userService.ApplicationCreateUser(user);

                            var programme = await _programmeService.GetProgramme(ProgrammeId);
                            var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, Owner);

                            var reference = await _referenceService.GetLatestReferenceId();
                            var sheet = await _clientInformationService.IssueInformationFor(user, Owner, clientProgramme, reference);
                            await _referenceService.CreateClientInformationReference(sheet);
                            clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                            clientProgramme.ClientProgrammeMembershipNumber = parts[4];////proposalreferencenum
                            sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                            sheet.Organisation.Add(Advisor);
                            await _programmeService.Update(clientProgramme);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }
        }

        public async Task ImportFanzAdvisors(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "fanznonepalist.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("62aea93b-8f7e-4554-b037-bb6726bc3c2d");
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            int lineCount = 0;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    try
                    {

                        user = await _userService.GetUserByUserName(parts[5]);///username

                        // organisation = await _organisationService.GetOrganisationByEmail(email);

                        if (user != null)
                        {
                            OrganisationType advisorType = new OrganisationType("Person - Individual");
                            InsuranceAttribute advisorAttribute = new InsuranceAttribute(currentUser, "Advisor");
                            OrganisationalUnit defaultUnit = new OrganisationalUnit(currentUser, "Person - Individual", "Person - Individual", null);

                            var advisorUnit = new AdvisorUnit(null, "Advisor", "Private", null)
                            {
                                IsPrincipalAdvisor = false
                            };

                            organisation = await _organisationService.GetOrganisationByName(parts[6]);//faqp name///this is owner

                            Organisation Advisor = new Organisation(currentUser, Guid.NewGuid())
                            {
                                OrganisationType = advisorType,
                                Email = user.Email,
                                Name = user.FullName
                            };

                            Advisor.InsuranceAttributes.Add(advisorAttribute);
                            Advisor.OrganisationalUnits.Add(defaultUnit);
                            Advisor.OrganisationalUnits.Add(advisorUnit);
                            user.Organisations.Add(Advisor);
                            Programme programme = await _programmeService.GetProgramme(programmeID);
                            ClientProgramme clientProgramme = await _programmeService.GetClientProgrammebyOwnerName(programme.Name, organisation.Name);
                            ClientInformationSheet sheet = clientProgramme.InformationSheet;

                            sheet.Organisation.Add(Advisor);
                            await _programmeService.Update(clientProgramme);
                        }


                        lineCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message + lineCount);
                    }
                }
            }
        }
        public async Task ImportApolloImportOwners(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;

            bool readFirstLine = false;
            string line;
            Guid.TryParse("633b32f7-93bd-4ed1-9f7e-088ae5312b98", out Guid ProgrammeId);
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "ApolloMember.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        string type = "";
                        string Name = "";
                        User user = new User(currentUser, Guid.NewGuid())
                        {
                            FirstName = parts[2],
                            LastName = parts[3],
                            FullName = parts[2] + " " + parts[3],
                            Email = parts[4],
                            UserName = parts[6]
                        };
                        if (parts[0] == "t")
                        {
                            type = "Corporation – Limited liability";
                            Name = parts[1];
                        }
                        else
                        {
                            type = "Person - Individual";
                            Name = user.FullName;
                        }

                        OrganisationType ownerType = new OrganisationType(type);
                        InsuranceAttribute ownerAttribute = new InsuranceAttribute(currentUser, type);
                        OrganisationalUnit ownerUnit = new OrganisationalUnit(currentUser, type, "Head Office", null);
                        Organisation Owner = new Organisation(currentUser, Guid.NewGuid())
                        {
                            OrganisationType = ownerType,
                            Email = user.Email,
                            Name = Name
                        };

                        Owner.OrganisationalUnits.Add(ownerUnit);
                        Owner.InsuranceAttributes.Add(ownerAttribute);
                        user.Organisations.Add(Owner);

                        OrganisationType advisorType = new OrganisationType("Person - Individual");
                        InsuranceAttribute advisorAttribute = new InsuranceAttribute(currentUser, "Advisor");
                        OrganisationalUnit defaultUnit = new OrganisationalUnit(currentUser, "Person - Individual", "Person - Individual", null);
                        AdvisorUnit AdvisorUnit = new AdvisorUnit(currentUser, "Advisor", "Person - Individual", null)
                        {
                            IsPrincipalAdvisor = true
                        };
                        Organisation Advisor = new Organisation(currentUser, Guid.NewGuid())
                        {
                            OrganisationType = advisorType,
                            Email = user.Email,
                            Name = user.FullName
                        };

                        Advisor.InsuranceAttributes.Add(advisorAttribute);
                        Advisor.OrganisationalUnits.Add(defaultUnit);
                        Advisor.OrganisationalUnits.Add(AdvisorUnit);
                        user.Organisations.Add(Advisor);
                        user.SetPrimaryOrganisation(Owner);
                        await _userService.ApplicationCreateUser(user);

                        var programme = await _programmeService.GetProgramme(ProgrammeId);
                        var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, Owner);

                        var reference = await _referenceService.GetLatestReferenceId();
                        var sheet = await _clientInformationService.IssueInformationFor(user, Owner, clientProgramme, reference);
                        await _referenceService.CreateClientInformationReference(sheet);
                        clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                        clientProgramme.ClientProgrammeMembershipNumber = parts[5];
                        clientProgramme.EGlobalClientNumber = parts[7];
                        clientProgramme.Tier = parts[8];
                        sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                        sheet.Organisation.Add(Advisor);
                        await _programmeService.Update(clientProgramme);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }
        }

        public async Task ImportNZBarOwners(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = false;
            string line;
            string email;
            string userName;
            string type = "";
            string Name = "";
            Guid.TryParse("8b6b0ca4-2ba3-4e40-a196-222c3e4982a2", out Guid ProgrammeId);
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "nzbarownerdata.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                         userName = parts[5];
                         email = parts[3];
                         

                            try
                            {
                                user = await _userService.GetUser(userName);

                            }
                            catch (Exception ex)
                            {
                                Random random = new Random();
                                int randomNumber = random.Next(10, 99);
                                userName = userName + randomNumber.ToString();
                            }
                            user = new User(currentUser, Guid.NewGuid(), userName);
                            user.FirstName = parts[1];
                            user.LastName = parts[2];
                            user.FullName = parts[1] + " " + parts[2];
                            user.Email = email;
                           

                        type = "Person - Individual";
                        Name = user.FullName;
                        OrganisationType ownerType = new OrganisationType(type);
                        InsuranceAttribute ownerAttribute = new InsuranceAttribute(currentUser, type);
                        OrganisationalUnit ownerUnit = new OrganisationalUnit(currentUser, type, "Head Office", null);
                        Organisation Owner = new Organisation(currentUser, Guid.NewGuid())
                        {
                            OrganisationType = ownerType,
                            Email = user.Email,
                            Name = Name,
                            //Initial = parts[2],//////////////
                            //honorific = parts[2]////kjjhk
                    };

                        OrganisationType barristerType = new OrganisationType("Person - Individual");
                        InsuranceAttribute barristerAttribute = new InsuranceAttribute(currentUser, "Barrister");
                        OrganisationalUnit defaultUnit = new OrganisationalUnit(currentUser, "Person - Individual", "Person - Individual", null);
                        BarristerUnit BarristerUnit = new BarristerUnit(currentUser, "Barrister", "Person - Individual", null)
                        {
                            IsPrincipalBarrister = true,
                            Initial = parts[6],
                            honorific = parts[7]
                        };
                        Organisation Barrister = new Organisation(currentUser, Guid.NewGuid())
                        {
                            OrganisationType = barristerType,
                            Email = user.Email,
                            Name = user.FullName
                        };

                        Barrister.InsuranceAttributes.Add(barristerAttribute);
                        Barrister.OrganisationalUnits.Add(defaultUnit);
                        Barrister.OrganisationalUnits.Add(BarristerUnit);
                        user.Organisations.Add(Barrister);


                        user.SetPrimaryOrganisation(Owner);




                        await _userService.ApplicationCreateUser(user);

                        var programme = await _programmeService.GetProgramme(ProgrammeId);
                        var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, Owner);

                        var reference = await _referenceService.GetLatestReferenceId();
                        var sheet = await _clientInformationService.IssueInformationFor(user, Owner, clientProgramme, reference);
                        await _referenceService.CreateClientInformationReference(sheet);
                        clientProgramme.EGlobalClientNumber = parts[6];
                        clientProgramme.EGlobalExternalContactNumber = parts[7];
                        clientProgramme.EGlobalBranchCode = parts[8];
                        clientProgramme.ClientProgrammeMembershipNumber = parts[4];
                        sheet.IsRenewawl = true;
                        sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                        await _programmeService.Update(clientProgramme);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }
        }
        public async Task ImportNZPIImportOwners(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;

            bool readFirstLine = false;
            string line;
            Guid.TryParse("bcbd2a62-5144-4b1d-84e8-206601a5bc8d", out Guid ProgrammeId);
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "NZPIMembers2019UpdatedDataClean.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        string type = "";
                        string Name = "";
                        User user = new User(currentUser, Guid.NewGuid())
                        {
                            FirstName = parts[3],
                            LastName = parts[4],
                            FullName = parts[3] + " " + parts[4],
                            Email = parts[5],
                            UserName = parts[6]
                        };
                        if (parts[0] == "t")
                        {
                            type = "Corporation – Limited liability";
                            Name = parts[1];
                        }
                        else
                        {
                            type = "Person - Individual";
                            Name = user.FullName;
                        }

                        OrganisationType ownerType = new OrganisationType(type);
                        InsuranceAttribute ownerAttribute = new InsuranceAttribute(currentUser, type);
                        OrganisationalUnit ownerUnit = new OrganisationalUnit(currentUser, type, "Head Office", null);
                        Organisation Owner = new Organisation(currentUser, Guid.NewGuid())
                        {
                            OrganisationType = ownerType,
                            Email = user.Email,
                            Name = Name,
                            TradingName = parts[2]
                        };

                        Owner.OrganisationalUnits.Add(ownerUnit);
                        Owner.InsuranceAttributes.Add(ownerAttribute);
                        user.Organisations.Add(Owner);

                        OrganisationType plannerType = new OrganisationType("Person - Individual");
                        InsuranceAttribute plannerAttribute = new InsuranceAttribute(currentUser, "Planner");
                        OrganisationalUnit defaultUnit = new OrganisationalUnit(currentUser, "Person - Individual", "Person - Individual", null);
                        PlannerUnit ContractorUnit = new PlannerUnit(currentUser, "Planner", "Person - Individual", null)
                        {
                            Qualifications = parts[7],
                            IsPrincipalPlanner = true
                        };

                        int.TryParse(parts[9], out int YearsAtFirm);
                        bool isNPIMember = false;
                        if (parts[10] == "1")
                        {
                            isNPIMember = true;
                        }
                        ContractorUnit.IsNZPIAMember = isNPIMember;
                        ContractorUnit.YearsAtFirm = YearsAtFirm;

                        Organisation planner = new Organisation(currentUser, Guid.NewGuid())
                        {
                            OrganisationType = plannerType,
                            Email = user.Email,
                            Name = user.FullName
                        };

                        planner.OrganisationalUnits.Add(defaultUnit);
                        planner.OrganisationalUnits.Add(ContractorUnit);
                        planner.InsuranceAttributes.Add(plannerAttribute);

                        user.Organisations.Add(planner);
                        user.SetPrimaryOrganisation(Owner);
                        await _userService.ApplicationCreateUser(user);

                        var programme = await _programmeService.GetProgramme(ProgrammeId);
                        var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, Owner);

                        var reference = await _referenceService.GetLatestReferenceId();
                        var sheet = await _clientInformationService.IssueInformationFor(user, Owner, clientProgramme, reference);
                        await _referenceService.CreateClientInformationReference(sheet);
                        clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                        clientProgramme.ClientProgrammeMembershipNumber = parts[11];
                        sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                        sheet.Organisation.Add(planner);
                        await _programmeService.Update(clientProgramme);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }
        }
        public async Task ImportNZPIImportPlanners(User CreatedUser)
        {
            //addresses need to be on one line            
            //var fileName = "C:\\Users\\temp\\NZFSGDataUploadtest.csv";
            var fileName = WorkingDirectory + "NZPIPlanners2019UpdatedDataClean.csv";
            var currentUser = CreatedUser;
            StreamReader reader;
            bool readFirstLine = false;
            string line;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        Random rand = new Random();
                        string Membership = parts[6];
                        string username = parts[0].Trim() + "_" + parts[1] + rand.Next(100);
                        string email = rand.Next(1000000000).ToString();
                        User user = new User(currentUser, Guid.NewGuid())
                        {
                            FirstName = parts[0],
                            LastName = parts[1],
                            FullName = parts[0] + " " + parts[1],
                            Email = email,
                            UserName = username
                        };

                        OrganisationType plannerType = new OrganisationType("Person - Individual");
                        InsuranceAttribute plannerAttribute = new InsuranceAttribute(currentUser, "Planner");
                        OrganisationalUnit defaultUnit = new OrganisationalUnit(currentUser, "Person - Individual", "Person - Individual", null);
                        PlannerUnit ContractorUnit = new PlannerUnit(currentUser, "Planner", "Person - Individual", null)
                        {
                            Qualifications = parts[2]
                        };


                        int.TryParse(parts[4], out int YearsAtFirm);
                        bool isNPIMember = false;
                        if (parts[5] == "1")
                        {
                            isNPIMember = true;
                        }
                        ContractorUnit.IsNZPIAMember = isNPIMember;
                        ContractorUnit.YearsAtFirm = YearsAtFirm;


                        Organisation planner = new Organisation(currentUser, Guid.NewGuid())
                        {
                            OrganisationType = plannerType,
                            Email = user.Email,
                            Name = user.FullName
                        };

                        planner.OrganisationalUnits.Add(defaultUnit);
                        planner.OrganisationalUnits.Add(ContractorUnit);
                        planner.InsuranceAttributes.Add(plannerAttribute);

                        user.SetPrimaryOrganisation(planner);
                        var result = await _programmeService.AddOrganisationByMembership(planner, Membership);
                        if (result)
                        {
                            await _userService.ApplicationCreateUser(user);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportNZPIImportContractors(User CreatedUser)
        {
            //addresses need to be on one line            
            //var fileName = "C:\\Users\\temp\\NZFSGDataUploadtest.csv";
            var fileName = WorkingDirectory + "NZPIContractors2019UpdatedDataClean.csv";
            var currentUser = CreatedUser;
            StreamReader reader;
            bool readFirstLine = false;
            string line;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        Random rand = new Random();
                        string Membership = parts[8];
                        string username = parts[0].Trim() + "_" + parts[1] + rand.Next(100);
                        string email = rand.Next(1000000000).ToString();
                        User user = new User(currentUser, Guid.NewGuid())
                        {
                            FirstName = parts[0],
                            LastName = parts[1],
                            FullName = parts[0] + " " + parts[1],
                            Email = email,
                            UserName = username
                        };

                        OrganisationType plannerType = new OrganisationType("Person - Individual");
                        InsuranceAttribute plannerAttribute = new InsuranceAttribute(currentUser, "Contractor");
                        OrganisationalUnit defaultUnit = new OrganisationalUnit(currentUser, "Person - Individual", "Person - Individual", null);
                        PlannerUnit ContractorUnit = new PlannerUnit(currentUser, "Contractor", "Person - Individual", null)
                        {
                            Qualifications = parts[2]
                        };

                        int.TryParse(parts[4], out int YearsAtInsured);
                        bool isNPIMember = false;
                        if (parts[5] == "1")
                        {
                            isNPIMember = true;
                        }
                        ContractorUnit.IsNZPIAMember = isNPIMember;
                        ContractorUnit.YearsAtInsured = YearsAtInsured;


                        Organisation planner = new Organisation(currentUser, Guid.NewGuid())
                        {
                            OrganisationType = plannerType,
                            Email = user.Email,
                            Name = user.FullName
                        };

                        planner.OrganisationalUnits.Add(defaultUnit);
                        planner.OrganisationalUnits.Add(ContractorUnit);
                        planner.InsuranceAttributes.Add(plannerAttribute);

                        user.SetPrimaryOrganisation(planner);
                        var result = await _programmeService.AddOrganisationByMembership(planner, Membership);
                        if (result)
                        {
                            await _userService.ApplicationCreateUser(user);
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportCEASServicePrincipals(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = false;
            string line;
            string email;
            string userName;
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "CEASPrincipals2020.csv";
            //var insuranceAttribute = await _InsuranceAttributeService.GetInsuranceAttributeByName("Principal");
            var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Person - Individual");
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    try
                    {
                        var hasProgramme = await _programmeService.HasProgrammebyMembership(parts[1]);
                        if (hasProgramme)
                        {
                            userName = parts[4].Replace(" ", string.Empty) + "_" + parts[3];

                            if (string.IsNullOrWhiteSpace(parts[5]))
                            {
                                email = userName + "@techcertain.com";
                            }
                            else
                            {
                                email = parts[5];
                            }

                            organisation = new Organisation(currentUser, Guid.NewGuid(), parts[2], organisationType, email);
                            //organisation.InsuranceAttributes.Add(insuranceAttribute);
                            //organisation.NZIAmembership = parts[1];
                            organisation.Email = email;
                            organisation.Phone = "12345";

                            if (!string.IsNullOrEmpty(parts[15]))
                            {
                                //organisation.Qualifications = parts[15];
                            }
                            if (!string.IsNullOrEmpty(parts[11]))
                            {

                            }
                            if (!string.IsNullOrEmpty(parts[12]))
                            {
                                //organisation.CPEngQualified = parts[12];
                            }

                            using (var uom = _unitOfWork.BeginUnitOfWork())
                            {
                                //insuranceAttribute.IAOrganisations.Add(organisation);
                                try
                                {
                                    await uom.Commit();
                                }
                                catch (Exception ex)
                                {
                                    await uom.Rollback();
                                }
                            }

                            await _organisationService.CreateNewOrganisation(organisation);
                            //await _programmeService.AddOrganisationByMembership(organisation);

                            user = await _userService.GetUserByEmail(email);

                            if (user == null)
                            {
                                try
                                {
                                    user = await _userService.GetUser(userName);
                                }
                                catch (Exception ex)
                                {
                                    Random random = new Random();
                                    int randomNumber = random.Next(10, 99);
                                    userName = userName + randomNumber.ToString();
                                }
                                user = new User(currentUser, Guid.NewGuid(), userName);
                                user.FirstName = parts[4];
                                user.LastName = parts[3];
                                user.FullName = parts[4] + " " + parts[3];
                                user.Email = email;
                                user.Address = "Import Address";
                                user.Phone = "12345";


                                if (!user.Organisations.Contains(organisation))
                                    user.Organisations.Add(organisation);

                                user.SetPrimaryOrganisation(organisation);
                                await _userService.ApplicationCreateUser(user);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }
        }
        public async Task ImportPMINZServicePrincipals(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string userName;
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "PMINZPersonnel2019Final.csv";
            //var insuranceAttribute = await _InsuranceAttributeService.GetInsuranceAttributeByName("project management personnel");
            var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Person - Individual");
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    try
                    {
                        var hasProgramme = await _programmeService.HasProgrammebyMembership(parts[13]);
                        var fullname = parts[0] + " " + parts[1];
                        if (hasProgramme)
                        {
                            userName = parts[0].Replace(" ", string.Empty) + "_" + parts[1].Replace(" ", string.Empty);

                            if (string.IsNullOrWhiteSpace(parts[2]))
                            {
                                email = parts[0].Replace(" ", string.Empty) + parts[1].Replace(" ", string.Empty) + "@techcertain.com";
                            }
                            else
                            {
                                email = parts[2];
                            }

                            organisation = new Organisation(currentUser, Guid.NewGuid(), fullname, organisationType, email);
                            //organisation.InsuranceAttributes.Add(insuranceAttribute);
                            //organisation.NZIAmembership = parts[13];
                            organisation.Email = email;
                            organisation.Phone = "12345";

                            //if (!string.IsNullOrEmpty(parts[3]))
                            //{
                            //    organisation.JobTitle = parts[3];
                            //}
                            //if (!string.IsNullOrEmpty(parts[4]))
                            //{
                            //    //organisation.Qualifications = parts[4];
                            //}
                            //if (!string.IsNullOrEmpty(parts[5]))
                            //{
                            //    organisation.ProfAffiliation = parts[5];
                            //}
                            //if (!string.IsNullOrEmpty(parts[6]))
                            //{
                            //    organisation.CurrentMembershipNo = parts[6];
                            //}
                            //if (!string.IsNullOrEmpty(parts[7]))
                            //{
                            //    if (parts[7] == "1")
                            //    {
                            //        organisation.IsCurrentMembership = true;
                            //    }
                            //    else
                            //    {
                            //        organisation.IsCurrentMembership = false;
                            //    }
                            //}
                            //if (!string.IsNullOrEmpty(parts[9]))
                            //{
                            //    if (parts[9] == "1")
                            //    {
                            //        organisation.CertType = "PMP";
                            //    }
                            //    else if (parts[9] == "2")
                            //    {
                            //        organisation.CertType = "CAPM";
                            //    }
                            //    else if (parts[9] == "3")
                            //    {
                            //        organisation.CertType = "ProjectDirector";
                            //    }
                            //    else
                            //    {
                            //        organisation.CertType = "Ordinary";
                            //    }
                            //}
                            //else
                            //{
                            //    organisation.CertType = "Ordinary";
                            //}
                            //if (!string.IsNullOrEmpty(parts[10]))
                            //{
                            //    if (parts[10] == "1")
                            //    {
                            //        organisation.InsuredEntityRelation = "Director";
                            //    }
                            //    else if (parts[10] == "2")
                            //    {
                            //        organisation.InsuredEntityRelation = "Employee";
                            //    }
                            //    else if (parts[10] == "3")
                            //    {
                            //        organisation.InsuredEntityRelation = "Contractor";
                            //    }
                            //}
                            //if (!string.IsNullOrEmpty(parts[11]))
                            //{
                            //    if (parts[11] == "1")
                            //    {
                            //        organisation.IsContractorInsured = true;
                            //    }
                            //    else
                            //    {
                            //        organisation.IsContractorInsured = false;
                            //    }
                            //}
                            //if (!string.IsNullOrEmpty(parts[12]))
                            //{
                            //    if (parts[12] == "1")
                            //    {
                            //        organisation.IsInsuredRequired = true;
                            //    }
                            //    else
                            //    {
                            //        organisation.IsInsuredRequired = false;
                            //    }
                            //}

                            using (var uom = _unitOfWork.BeginUnitOfWork())
                            {
                                //insuranceAttribute.IAOrganisations.Add(organisation);
                                try
                                {
                                    await uom.Commit();
                                }
                                catch (Exception ex)
                                {
                                    await uom.Rollback();
                                }
                            }

                            await _organisationService.CreateNewOrganisation(organisation);
                            //await _programmeService.AddOrganisationByMembership(organisation);

                            user = await _userService.GetUserByEmail(email);

                            if (user == null)
                            {
                                try
                                {
                                    user = await _userService.GetUser(userName);
                                }
                                catch (Exception ex)
                                {
                                    Random random = new Random();
                                    int randomNumber = random.Next(10, 99);
                                    userName = userName + randomNumber.ToString();
                                }
                                user = new User(currentUser, Guid.NewGuid(), userName);
                                user.FirstName = parts[0];
                                user.LastName = parts[1];
                                user.FullName = fullname;
                                user.Email = email;
                                user.Address = "Import Address";
                                user.Phone = "12345";


                                if (!user.Organisations.Contains(organisation))
                                    user.Organisations.Add(organisation);

                                user.SetPrimaryOrganisation(organisation);
                                await _userService.ApplicationCreateUser(user);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }
        }

        public async Task ImportAAAServiceIndividuals(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            User user = null;
            // Organisation organisation = new Organisation();
            bool readFirstLine = true;
            string line;
            string email;
            string userName;
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "TripleAUserAdvisors.csv";

            var organisationType = new OrganisationType(null, "Person - Individual");
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    email = "";
                    try
                    {
                        var hasProgramme = await _programmeService.HasProgrammebyMembership(parts[0]);
                        var fullname = parts[9].Trim() + " " + parts[10].Trim();
                        if (hasProgramme)
                        {
                            userName = parts[9].Replace(" ", string.Empty) + "_" + parts[10].Replace(" ", string.Empty);

                            if (string.IsNullOrWhiteSpace(parts[12]))
                            {
                                email = email = parts[9].Replace(" ", string.Empty) + parts[10].Replace(" ", string.Empty) + "@techcertain.com";
                            }
                            else
                            {
                                email = parts[12];
                            }
                            var privateUnit = new OrganisationalUnit(null, "Private", "Person - Individual", null);
                            var advisorUnit = new AdvisorUnit(null, "Advisor", "Private", null)
                            {
                                RegisteredStatus = parts[13],
                                Qualifications = parts[14],
                                Duration = parts[15],
                                IsPrincipalAdvisor = false
                            };
                            var AdvisororganisationType = new OrganisationType(null, "Person - Individual");

                            var IA = new InsuranceAttribute(null, "Advisor");
                            var orgname = parts[9].Trim() + " " + parts[10].Trim();
                            Organisation Advisororganisation = new Organisation(currentUser, Guid.NewGuid(), orgname, AdvisororganisationType, email);
                            Advisororganisation.Clientmembership = parts[0];
                            Advisororganisation.InsuranceAttributes.Add(IA);
                            Advisororganisation.OrganisationalUnits.Add(privateUnit);
                            Advisororganisation.OrganisationalUnits.Add(advisorUnit);
                            await _organisationService.CreateNewOrganisation(Advisororganisation);
                            await _programmeService.AddOrganisationByMembership(Advisororganisation, parts[0]);

                            //await _programmeService.AddOrganisationByMembership(organisation);

                            user = await _userService.GetUserByEmail(email);

                            if (user == null)
                            {
                                try
                                {
                                    user = await _userService.GetUser(userName);
                                }
                                catch (Exception ex)
                                {
                                    Random random = new Random();
                                    int randomNumber = random.Next(10, 99);
                                    userName = userName + randomNumber.ToString();
                                }
                                user = new User(currentUser, Guid.NewGuid(), userName);
                                user.FirstName = parts[9].Trim();
                                user.LastName = parts[10].Trim();
                                user.FullName = user.FirstName + " " + user.LastName;
                                user.Email = email;
                                user.Address = "";
                                if (!user.Organisations.Contains(Advisororganisation))
                                    user.Organisations.Add(Advisororganisation);

                                user.SetPrimaryOrganisation(Advisororganisation);
                                await _userService.ApplicationCreateUser(user);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }

                }
            }
        }
        public async Task ImportCEASServiceClaims(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            ClaimNotification claimNotification;
            bool readFirstLine = false;
            string line;
            var fileName = WorkingDirectory + "CEASClaims2020.csv";
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    try
                    {
                        line = reader.ReadLine();
                        string[] parts = line.Split(',');
                        claimNotification = new ClaimNotification(currentUser);
                        claimNotification.ClaimMembershipNumber = parts[0];
                        claimNotification.ClaimTitle = parts[1];
                        claimNotification.ClaimReference = parts[2];
                        if (!string.IsNullOrWhiteSpace(parts[3]))
                        {
                            claimNotification.ClaimNotifiedDate = DateTime.Parse(parts[3]);
                        }
                        claimNotification.Claimant = parts[4];
                        claimNotification.ClaimStatus = parts[5];

                        await _programmeService.AddClaimNotificationByMembership(claimNotification);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportCEASServiceContract(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            BusinessContract businessContract;
            bool readFirstLine = false;
            string line;
            var fileName = WorkingDirectory + "CEASContracts2019.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        businessContract = new BusinessContract(currentUser);
                        businessContract.MembershipNumber = parts[6];
                        businessContract.ContractTitle = parts[0];
                        businessContract.ProjectDescription = parts[1];
                        businessContract.ProjectDuration = parts[5];
                        businessContract.ConstructionValue = parts[4];
                        businessContract.Fees = parts[3];
                        businessContract.MajorResponsibilities = parts[2];

                        await _programmeService.AddBusinessContractByMembership(businessContract);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportPMINZServiceContract(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            BusinessContract businessContract;
            bool readFirstLine = true;
            string line;
            var fileName = WorkingDirectory + "PMINZProjects2019Final.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        businessContract = new BusinessContract(currentUser);
                        businessContract.MembershipNumber = parts[7];
                        if (!string.IsNullOrEmpty(parts[0]))
                            businessContract.ProjectDescription = parts[0];
                        if (!string.IsNullOrEmpty(parts[1]))
                            businessContract.Fees = parts[1];
                        if (!string.IsNullOrEmpty(parts[2]))
                            businessContract.ConstructionValue = parts[2];
                        if (!string.IsNullOrEmpty(parts[3]))
                            businessContract.ProjectDuration = parts[3];
                        if (parts[4] == "1")
                        {
                            businessContract.ProjectDirector = true;
                        }
                        else
                        {
                            businessContract.ProjectDirector = false;
                        }
                        if (parts[5] == "1")
                        {
                            businessContract.ProjectManager = true;
                        }
                        else
                        {
                            businessContract.ProjectManager = false;
                        }
                        if (parts[6] == "1")
                        {
                            businessContract.ProjectCoordinator = true;
                        }
                        else
                        {
                            businessContract.ProjectCoordinator = false;
                        }

                        await _programmeService.AddBusinessContractByMembership(businessContract);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportPMINZServicePreRenewData(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            PreRenewOrRefData preRenewOrRefData;
            bool readFirstLine = true;
            string line;
            var fileName = WorkingDirectory + "PMINZPolicyData2019Final.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        preRenewOrRefData = new PreRenewOrRefData(currentUser, parts[1], parts[0]);
                        if (!string.IsNullOrEmpty(parts[2]))
                            preRenewOrRefData.PIBoundLimit = parts[2];
                        if (!string.IsNullOrEmpty(parts[3]))
                            preRenewOrRefData.PIBoundPremium = parts[3];
                        if (!string.IsNullOrEmpty(parts[4]))
                            preRenewOrRefData.PIRetro = parts[4];
                        if (!string.IsNullOrEmpty(parts[5]))
                            preRenewOrRefData.GLRetro = parts[5];
                        if (!string.IsNullOrEmpty(parts[6]))
                            preRenewOrRefData.DORetro = parts[6];
                        if (!string.IsNullOrEmpty(parts[7]))
                            preRenewOrRefData.ELRetro = parts[7];
                        if (!string.IsNullOrEmpty(parts[8]))
                            preRenewOrRefData.EDRetro = parts[8];
                        if (!string.IsNullOrEmpty(parts[9]))
                            preRenewOrRefData.SLRetro = parts[9];
                        if (!string.IsNullOrEmpty(parts[10]))
                            preRenewOrRefData.CLRetro = parts[10];
                        if (!string.IsNullOrEmpty(parts[11]))
                            preRenewOrRefData.EndorsementProduct = parts[11];
                        if (!string.IsNullOrEmpty(parts[12]))
                            preRenewOrRefData.EndorsementTitle = parts[12];
                        if (!string.IsNullOrEmpty(parts[13]))
                            preRenewOrRefData.EndorsementText = parts[13];

                        await _programmeService.AddPreRenewOrRefDataByMembership(preRenewOrRefData);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportAAAServicePreRenewData(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            PreRenewOrRefData preRenewOrRefData;
            bool readFirstLine = true;
            string line;
            var fileName = WorkingDirectory + "TripleAPolicyData2019.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        preRenewOrRefData = new PreRenewOrRefData(currentUser, parts[1], parts[0]);
                        if (!string.IsNullOrEmpty(parts[2]))
                            preRenewOrRefData.CLRetro = parts[2];
                        if (!string.IsNullOrEmpty(parts[3]))
                            preRenewOrRefData.EndorsementProduct = parts[3];
                        if (!string.IsNullOrEmpty(parts[4]))
                            preRenewOrRefData.EndorsementTitle = parts[4];
                        if (!string.IsNullOrEmpty(parts[5]))
                            preRenewOrRefData.EndorsementText = parts[5];

                        await _programmeService.AddPreRenewOrRefDataByMembership(preRenewOrRefData);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportNZPIServicePreRenewData(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            PreRenewOrRefData preRenewOrRefData;
            bool readFirstLine = false;
            string line;
            var fileName = WorkingDirectory + "NZPIPolicyData2019.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        preRenewOrRefData = new PreRenewOrRefData(currentUser, parts[1], parts[0]);
                        if (!string.IsNullOrEmpty(parts[2]))
                            preRenewOrRefData.PIRetro = parts[2];
                        if (!string.IsNullOrEmpty(parts[3]))
                            preRenewOrRefData.GLRetro = parts[3];
                        if (!string.IsNullOrEmpty(parts[4]))
                            preRenewOrRefData.DORetro = parts[4];
                        if (!string.IsNullOrEmpty(parts[5]))
                            preRenewOrRefData.ELRetro = parts[5];
                        if (!string.IsNullOrEmpty(parts[6]))
                            preRenewOrRefData.EDRetro = parts[6];
                        if (!string.IsNullOrEmpty(parts[7]))
                            preRenewOrRefData.SLRetro = parts[7];
                        if (!string.IsNullOrEmpty(parts[8]))
                            preRenewOrRefData.CLRetro = parts[8];
                        if (!string.IsNullOrEmpty(parts[9]))
                            preRenewOrRefData.LPDRetro = parts[9];
                        if (!string.IsNullOrEmpty(parts[10]))
                            preRenewOrRefData.FIDRetro = parts[10];
                        if (!string.IsNullOrEmpty(parts[11]))
                            preRenewOrRefData.EndorsementProduct = parts[11];
                        if (!string.IsNullOrEmpty(parts[12]))
                            preRenewOrRefData.EndorsementTitle = parts[12];
                        if (!string.IsNullOrEmpty(parts[13]))
                            preRenewOrRefData.EndorsementText = parts[13];

                        await _programmeService.AddPreRenewOrRefDataByMembership(preRenewOrRefData);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportNZBarPreRenewData(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            PreRenewOrRefData preRenewOrRefData;
            bool readFirstLine = false;
            string line;
            var fileName = WorkingDirectory + "nzbapolicydata2021.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        preRenewOrRefData = new PreRenewOrRefData(currentUser, parts[1], parts[0]);
                        if (!string.IsNullOrEmpty(parts[2]))
                            preRenewOrRefData.CLRetro = parts[2];
                        if (!string.IsNullOrEmpty(parts[3]))
                            preRenewOrRefData.EndorsementProduct = parts[3];
                        if (!string.IsNullOrEmpty(parts[4]))
                            preRenewOrRefData.EndorsementTitle = parts[4];
                        if (!string.IsNullOrEmpty(parts[5]))
                            preRenewOrRefData.EndorsementText = parts[5];

                        await _programmeService.AddPreRenewOrRefDataByMembership(preRenewOrRefData);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportNZBarImportClaims(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            ClaimNotification claimNotification;
            bool readFirstLine = false;
            string line;
            var fileName = WorkingDirectory + "nzbaclaimdata2021.csv";
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        claimNotification = new ClaimNotification(currentUser);
                        claimNotification.ClaimMembershipNumber = parts[0];
                        if (!string.IsNullOrEmpty(parts[1]))
                            claimNotification.ClaimTitle = parts[1];
                        if (!string.IsNullOrEmpty(parts[2]))
                        {
                            claimNotification.ClaimDescription = parts[2];
                            if (parts[2].Length > 255)
                            {
                                claimNotification.ClaimDescription = parts[2].Substring(0, 255);
                            }
                        }
                        if (!string.IsNullOrEmpty(parts[3]))
                            claimNotification.ClaimReference = parts[3];
                        if (!string.IsNullOrEmpty(parts[4]))
                            claimNotification.ClaimInsurerReference = parts[4];
                        if (!string.IsNullOrEmpty(parts[5]))
                            claimNotification.ClaimNotifiedDate = DateTime.Parse(parts[5]);
                        if (!string.IsNullOrEmpty(parts[6]))
                            claimNotification.ClaimInsuredName = parts[6];
                        if (!string.IsNullOrEmpty(parts[7]))
                            claimNotification.Claimant = parts[7];
                        if (!string.IsNullOrEmpty(parts[8]))
                            claimNotification.ClaimEstimateInsuredLiability = decimal.Parse(parts[8]);
                        if (!string.IsNullOrEmpty(parts[9]))
                        {
                            claimNotification.ClaimNotes = parts[9];
                            if (parts[9].Length > 255)
                            {
                                claimNotification.ClaimNotes = parts[9].Substring(0, 255);
                            }
                        }
                        claimNotification.ClaimStatus = "Closed";

                        await _programmeService.AddClaimNotificationByMembership(claimNotification);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportApolloServicePreRenewData(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            PreRenewOrRefData preRenewOrRefData;
            bool readFirstLine = false;
            string line;
            var fileName = WorkingDirectory + "ApolloPolicyData2019.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        preRenewOrRefData = new PreRenewOrRefData(currentUser, parts[1], parts[0]);
                        if (!string.IsNullOrEmpty(parts[2]))
                            preRenewOrRefData.CLRetro = parts[2];
                        if (!string.IsNullOrEmpty(parts[3]))
                            preRenewOrRefData.EndorsementProduct = parts[3];
                        if (!string.IsNullOrEmpty(parts[4]))
                            preRenewOrRefData.EndorsementTitle = parts[4];
                        if (!string.IsNullOrEmpty(parts[5]))
                            preRenewOrRefData.EndorsementText = parts[5];

                        await _programmeService.AddPreRenewOrRefDataByMembership(preRenewOrRefData);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportApolloSetELDefaultVale(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            bool readFirstLine = false;
            string line;
            var fileName = WorkingDirectory + "ApolloSubmittedUIS2020.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        if (!string.IsNullOrEmpty(parts[0]))
                        {
                            var sheet = await _clientInformationService.GetInformation(new Guid(parts[0]));

                            if (!sheet.Answers.Where(sa => sa.ItemName == "EPLViewModel.HaveAnyEmployeeYN").Any())
                            {
                                var newanswer = new ClientInformationAnswer(currentUser, "EPLViewModel.HaveAnyEmployeeYN", "1");
                                sheet.Answers.Add(newanswer);
                            }

                            await _clientInformationService.UpdateInformation(sheet);
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }


        public async Task ImportAbbottServicePreRenewData(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            PreRenewOrRefData preRenewOrRefData;
            bool readFirstLine = false;
            string line;
            var fileName = WorkingDirectory + "AbbottPolicyData2019.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        preRenewOrRefData = new PreRenewOrRefData(currentUser, parts[1], parts[0]);
                        if (!string.IsNullOrEmpty(parts[2]))
                            preRenewOrRefData.CLRetro = parts[2];
                        if (!string.IsNullOrEmpty(parts[3]))
                            preRenewOrRefData.OTRetro = parts[3];
                        if (!string.IsNullOrEmpty(parts[4]))
                            preRenewOrRefData.EndorsementProduct = parts[4];
                        if (!string.IsNullOrEmpty(parts[5]))
                            preRenewOrRefData.EndorsementTitle = parts[5];
                        if (!string.IsNullOrEmpty(parts[6]))
                            preRenewOrRefData.EndorsementText = parts[6];

                        await _programmeService.AddPreRenewOrRefDataByMembership(preRenewOrRefData);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }

        public async Task ImportDANZServicePreRenewData(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            PreRenewOrRefData preRenewOrRefData;
            bool readFirstLine = true;
            string line;
            var fileName = WorkingDirectory + "DANZPolicyData2019.csv";

            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    try
                    {
                        preRenewOrRefData = new PreRenewOrRefData(currentUser, parts[1], parts[0]);
                        if (!string.IsNullOrEmpty(parts[2]))
                            preRenewOrRefData.PIRetro = parts[2];
                        if (!string.IsNullOrEmpty(parts[3]))
                            preRenewOrRefData.GLRetro = parts[3];
                        if (!string.IsNullOrEmpty(parts[4]))
                            preRenewOrRefData.DORetro = parts[4];
                        if (!string.IsNullOrEmpty(parts[5]))
                            preRenewOrRefData.ELRetro = parts[5];
                        if (!string.IsNullOrEmpty(parts[6]))
                            preRenewOrRefData.EDRetro = parts[6];
                        if (!string.IsNullOrEmpty(parts[7]))
                            preRenewOrRefData.SLRetro = parts[7];
                        if (!string.IsNullOrEmpty(parts[8]))
                            preRenewOrRefData.CLRetro = parts[8];
                        if (!string.IsNullOrEmpty(parts[9]))
                            preRenewOrRefData.EndorsementProduct = parts[9];
                        if (!string.IsNullOrEmpty(parts[10]))
                            preRenewOrRefData.EndorsementTitle = parts[10];
                        if (!string.IsNullOrEmpty(parts[11]))
                            preRenewOrRefData.EndorsementText = parts[11];

                        await _programmeService.AddPreRenewOrRefDataByMembership(preRenewOrRefData);

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
        public async Task ImportDANZServiceIndividuals(User CreatedUser)
        {
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "DANZClients2019.csv";
            var currentUser = CreatedUser;
            Guid programmeID = Guid.Parse("226ca7cb-8145-4ac4-87dd-7f5dcc6358f4");
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            int lineCount = 0;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    //if (!readFirstLine)
                    //{
                    //    line = reader.ReadLine();
                    //    readFirstLine = true;
                    //}

                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    try
                    {
                        if (string.IsNullOrWhiteSpace(parts[4]))
                        {
                            email = parts[8] + "@techcertain.com";
                            user = await _userService.GetUserByEmail(email);
                        }
                        else
                        {
                            email = parts[4];
                        }

                        organisation = await _organisationService.GetOrganisationByEmail(email);

                        if (user == null)
                        {
                            user = new User(currentUser, Guid.NewGuid(), parts[8]);
                        }
                        organisation = await _organisationService.GetOrganisationByEmail(email);
                        if (parts[0] == "f")
                        {
                            if (organisation == null)
                            {
                                var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Person - Individual");
                                organisation = new Organisation(currentUser, Guid.NewGuid(), parts[2] + " " + parts[3], organisationType, email);
                                await _organisationService.CreateNewOrganisation(organisation);
                            }
                        }
                        else
                        {
                            if (organisation == null)
                            {
                                var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Corporation – Limited liability");
                                organisation = new Organisation(currentUser, Guid.NewGuid(), parts[1], organisationType, parts[4]);
                                await _organisationService.CreateNewOrganisation(organisation);
                            }
                        }

                        user.FirstName = parts[2];
                        user.LastName = parts[3];
                        user.FullName = parts[2] + " " + parts[3];
                        user.Email = email;
                        user.Address = "";
                        user.Phone = "12345";

                        if (!user.Organisations.Contains(organisation))
                            user.Organisations.Add(organisation);
                        user.SetPrimaryOrganisation(organisation);

                        await _userService.ApplicationCreateUser(user);

                        var programme = await _programmeService.GetProgramme(programmeID);
                        var clientProgramme = await _programmeService.CreateClientProgrammeFor(programme.Id, user, organisation);

                        var reference = await _referenceService.GetLatestReferenceId();
                        var sheet = await _clientInformationService.IssueInformationFor(user, organisation, clientProgramme, reference);
                        await _referenceService.CreateClientInformationReference(sheet);

                        using (var uow = _unitOfWork.BeginUnitOfWork())
                        {

                            clientProgramme.BrokerContactUser = programme.BrokerContactUser;
                            clientProgramme.ClientProgrammeMembershipNumber = parts[7];
                            sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, programme.Name + "UIS issue Process Completed"));
                            try
                            {
                                await uow.Commit();
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(ex.Message);
                            }
                        }
                        lineCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message + lineCount);
                    }
                }
            }
        }
        public async Task ImportDANZServicePersonnel(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            User user = null;
            Organisation organisation = null;
            bool readFirstLine = true;
            string line;
            string email;
            string userName;
            //addresses need to be on one line            
            var fileName = WorkingDirectory + "DANZPersonnel2019.csv";
            //var insuranceAttribute = await _InsuranceAttributeService.GetInsuranceAttributeByName("project management personnel");
            var organisationType = await _organisationTypeService.GetOrganisationTypeByName("Person - Individual");
            int lineCount = 0;
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if has a title row
                    if (!readFirstLine)
                    {
                        line = reader.ReadLine();
                        readFirstLine = true;
                    }
                    line = reader.ReadLine();
                    string[] parts = line.Split(',');
                    user = null;
                    organisation = null;
                    email = "";
                    try
                    {
                        var hasProgramme = await _programmeService.HasProgrammebyMembership(parts[12]);
                        var fullname = parts[0] + " " + parts[1];
                        if (hasProgramme)
                        {
                            userName = parts[0].Replace(" ", string.Empty) + "_" + parts[1].Replace(" ", string.Empty);

                            if (string.IsNullOrWhiteSpace(parts[2]))
                            {
                                email = parts[0].Replace(" ", string.Empty) + parts[1].Replace(" ", string.Empty) + "@techcertain.com";
                            }
                            else
                            {
                                email = parts[2];
                            }

                            organisation = new Organisation(currentUser, Guid.NewGuid(), fullname, organisationType, email);
                            //organisation.InsuranceAttributes.Add(insuranceAttribute);
                            //organisation.NZIAmembership = parts[12];
                            organisation.Email = email;
                            organisation.Phone = "12345";

                            if (!string.IsNullOrEmpty(parts[5]))
                            {
                                //organisation.Qualifications = parts[5];
                            }
                            if (!string.IsNullOrEmpty(parts[3]))
                            {
                                //organisation.DateQualified = parts[3];
                            }
                            if (!string.IsNullOrEmpty(parts[6]))
                            {
                                //if (parts[6] == "1")
                                //{
                                //    organisation.IsRegisteredLicensed = true;
                                //}
                                //else
                                //{
                                //    organisation.IsRegisteredLicensed = false;
                                //}

                            }
                            if (!string.IsNullOrEmpty(parts[7]))
                            {
                                //if (parts[7] == "1")
                                //{
                                //    organisation.DesignLicensed = "Yes";
                                //}
                                //else
                                //{
                                //    organisation.DesignLicensed = "No";
                                //}
                            }
                            if (!string.IsNullOrEmpty(parts[8]))
                            {
                                //if (parts[8] == "1")
                                //{
                                //    organisation.SiteLicensed = "Yes";
                                //}
                                //else
                                //{
                                //    organisation.SiteLicensed = "No";
                                //}
                            }
                            if (!string.IsNullOrEmpty(parts[9]))
                            {
                                //if (parts[9] == "1")
                                //{
                                //    organisation.InsuredEntityRelation = "Director";
                                //}
                                //else if (parts[9] == "2")
                                //{
                                //    organisation.InsuredEntityRelation = "Employee";
                                //}
                                //else if (parts[9] == "3")
                                //{
                                //    organisation.InsuredEntityRelation = "Contractor";
                                //}
                            }


                            using (var uom = _unitOfWork.BeginUnitOfWork())
                            {
                                //insuranceAttribute.IAOrganisations.Add(organisation);
                                try
                                {
                                    await uom.Commit();
                                }
                                catch (Exception ex)
                                {
                                    await uom.Rollback();
                                }
                            }

                            await _organisationService.CreateNewOrganisation(organisation);
                            //await _programmeService.AddOrganisationByMembership(organisation);

                            user = await _userService.GetUserByEmail(email);

                            if (user == null)
                            {
                                try
                                {
                                    user = await _userService.GetUser(userName);
                                }
                                catch (Exception ex)
                                {
                                    Random random = new Random();
                                    int randomNumber = random.Next(10, 99);
                                    userName = userName + randomNumber.ToString();
                                }
                                user = new User(currentUser, Guid.NewGuid(), userName);
                                user.FirstName = parts[0];
                                user.LastName = parts[1];
                                user.FullName = fullname;
                                user.Email = email;
                                user.Address = "Import Address";
                                user.Phone = "12345";


                                if (!user.Organisations.Contains(organisation))
                                    user.Organisations.Add(organisation);

                                user.SetPrimaryOrganisation(organisation);
                                await _userService.ApplicationCreateUser(user);
                            }
                        }
                        lineCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message + lineCount);
                    }

                }
            }
        }
        public async Task ImportDANZServiceClaims(User CreatedUser)
        {
            var currentUser = CreatedUser;
            StreamReader reader;
            ClaimNotification claimNotification;
            var programme = await _programmeService.GetProgramme(Guid.Parse("226ca7cb-8145-4ac4-87dd-7f5dcc6358f4"));
            Product DanzPIProd = programme.Products.FirstOrDefault(p => p.UnderwritingModuleCode == "DANZ_PI");
            Product DanzEPLProd = programme.Products.FirstOrDefault(p => p.UnderwritingModuleCode == "DANZ_ED");
            bool readFirstLine = false;
            string line;
            var fileName = WorkingDirectory + "DANZClaimsDetails.csv";
            using (reader = new StreamReader(fileName))
            {
                while (!reader.EndOfStream)
                {
                    //if (!readFirstLine)
                    //{
                    //    line = reader.ReadLine();
                    //    readFirstLine = true;
                    //}
                    try
                    {
                        line = reader.ReadLine();
                        string[] parts = line.Split(',');
                        claimNotification = new ClaimNotification(currentUser);
                        claimNotification.ClaimMembershipNumber = parts[0];
                        claimNotification.ClaimTitle = parts[4];
                        claimNotification.ClaimReference = parts[3];
                        claimNotification.ClaimDescription = parts[5];
                        claimNotification.ClaimNotifiedDate = DateTime.Parse(parts[7]);
                        claimNotification.ClaimDateOfLoss = DateTime.Parse(parts[8]);
                        claimNotification.ClaimEstimateInsuredLiability = decimal.Parse(parts[2]);
                        claimNotification.Claimant = parts[6];
                        claimNotification.ClaimStatus = parts[9];
                        if (parts[1] == "PI")
                        {
                            claimNotification.ClaimProducts.Add(DanzPIProd);
                        }
                        else
                        {
                            claimNotification.ClaimProducts.Add(DanzEPLProd);
                        }

                        await _programmeService.AddClaimNotificationByMembership(claimNotification);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
        }
    }
}

