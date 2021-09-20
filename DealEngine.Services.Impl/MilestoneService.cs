using System;
using System.Linq;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;
using System.Threading.Tasks;
using NHibernate.Linq;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace DealEngine.Services.Impl
{
    public class MilestoneService : IMilestoneService
    {
        IMapperSession<Milestone> _milestoneRepository;
        IProgrammeService _programmeService;
        ITaskingService _taskingService;
        IUserService _userService;
        IEmailService _emailService;

        public MilestoneService(
            IEmailService emailService,
            IUserService userService,
            IMapperSession<Milestone> milestoneRepository,
            IProgrammeService programmeService,
            ITaskingService taskingService
            )
        {
            _emailService = emailService;
            _userService = userService;
            _taskingService = taskingService;
            _programmeService = programmeService;
            _milestoneRepository = milestoneRepository;
        }

        public async Task<Milestone> GetMilestoneProgrammeId(Guid programmeId)
        {
            return await _milestoneRepository.FindAll().FirstOrDefaultAsync(m => m.Programme.Id == programmeId);
        }

        public async Task<string> SetMilestoneFor(string activityName, User user, ClientInformationSheet sheet)
        {
            var milestone = await GetMilestoneProgrammeId(sheet.Programme.BaseProgramme.Id);
            string Discription = "";
            if (milestone != null)
            {
                if (activityName == "Agreement Status - Not Started")
                {
                    Discription = await NotStartedMilestone(activityName, user, milestone);
                    await CompleteMilestoneFor("Agreement Status - Not Started", user, sheet);
                }
                if (activityName == "Agreement Status - Declined")
                {
                    Discription = await DeclinedMilestone(activityName, user, milestone);                    
                }
                if (activityName == "Agreement Status – Referred")
                {
                    Discription = await ReferredMilestone(activityName, user, milestone, sheet);
                }
            }
            return Discription;
        }

        private async Task<string> DeclinedMilestone(string activityName, User user, Milestone milestone)
        {
            var ProgrammeProcesse = milestone.ProgrammeProcesses.FirstOrDefault(p => p.Activities.Any(a => a.Name == activityName));
            if (ProgrammeProcesse != null)
            {
                //run task
                //run email
                return ProgrammeProcesse.Activities.FirstOrDefault(a => a.Name == activityName).Advisory.Description;
            }
            return "";
        }

        private async Task<string> NotStartedMilestone(string activityName, User user, Milestone milestone)
        {
            var ProgrammeProcesse = milestone.ProgrammeProcesses.FirstOrDefault(p => p.Activities.Any(a => a.Name == activityName));
            if (ProgrammeProcesse != null)
            {
                //run task
                //run email
                return ProgrammeProcesse.Activities.FirstOrDefault(a => a.Name == activityName).Advisory.Description;
            }
            return "";
        }

        private async Task<string> ReferredMilestone(string activityName, User user, Milestone milestone, ClientInformationSheet sheet)
        {
            var ProgrammeProcesse = milestone.ProgrammeProcesses.FirstOrDefault(p => p.Activities.Any(a => a.Name == activityName));
            if (ProgrammeProcesse != null)
            {
                //run task
                var NotifyUsers = sheet.Programme.BaseProgramme.BrokerContactUser;
                var Advisory = ProgrammeProcesse.Activities.FirstOrDefault(a => a.Name == activityName).Advisory;
                if (!NotifyUsers.UserTasks.Any(t => t.Name.Contains(activityName + "_" + sheet.Id)))
                {
                    UserTask ReferralTask = new UserTask(user, activityName + "_" + sheet.Id, null)
                    {
                        URL = "/Agreement/ViewAcceptedAgreement/" + sheet.Programme.Id.ToString(),
                        Body = "UIS Referral: " + sheet.ReferenceId + " (" + sheet.Programme.BaseProgramme.Name + " - " + sheet.Programme.Owner.Name + ")"
                    };

                    NotifyUsers.UserTasks.Add(ReferralTask);
                    await _userService.Update(NotifyUsers);
                }                           
                //run email
                return Advisory.Description;
            }
            return "";
        }

        public async Task CompleteMilestoneFor(string activityName, User user, ClientInformationSheet sheet)
        {
            var milestone = await GetMilestoneProgrammeId(sheet.Programme.BaseProgramme.Id);
            if(milestone!= null)
            {
                if (activityName == "Agreement Status - Not Started")
                {
                    await NotStartedCompleted(activityName, user, sheet);
                }
                if (activityName == "Agreement Status – Referred")
                {
                    await ReferredComplete(activityName, user, sheet);
                }
            }

        }

        private async Task NotStartedCompleted(string activityName, User user, ClientInformationSheet sheet)
        {
            if(!sheet.ClientInformationSheetAuditLogs.Any(l=>l.AuditLogDetail.Contains(activityName)))
            {
                string log = "User: " + user.UserName + " closed " + activityName + " Advisory on " + DateTime.Now;
                sheet.ClientInformationSheetAuditLogs.Add(new AuditLog(user, sheet, null, log));
            }
        }

        public async Task CreateMilestone(User user, IFormCollection collection)
        {
            Guid.TryParse(collection["MilestoneViewModel.Programme"].ToString(), out Guid ProgrammeId);
            string programmeProcess = collection["MilestoneViewModel.ProgrammeProcesses"].ToString();
            string activity = collection["MilestoneViewModel.Activity"].ToString();
            Programme programme =  await _programmeService.GetProgramme(ProgrammeId);
            Milestone milestone =  await GetMilestoneProgrammeId(programme.Id);
            if (milestone == null)
            {
                milestone = new Milestone(user, programme);
            }
            var ProgrammeProcess = milestone.ProgrammeProcesses.FirstOrDefault(p => p.Name == programmeProcess);
            if (ProgrammeProcess == null)
            {
                ProgrammeProcess = new ProgrammeProcess(user, programmeProcess);
                milestone.ProgrammeProcesses.Add(ProgrammeProcess);
            }
            var Activity = ProgrammeProcess.Activities.FirstOrDefault(a => a.Name == activity);
            if (Activity == null)
            {
                Activity = new Activity(user, activity, collection, null);
                ProgrammeProcess.Activities.Add(Activity);
            }
            else
            {
                Activity.Advisory.PopulateEntity(collection);
            }

            await Update(milestone);
        }

        private async Task Update(Milestone milestone)
        {
            await _milestoneRepository.AddAsync(milestone);
        }

        private async Task ReferredComplete(string activityName, User user, ClientInformationSheet sheet)
        {
            //close task
            var UserTask = sheet.Programme.BaseProgramme.BrokerContactUser.UserTasks.FirstOrDefault(t => t.Name == activityName + "_" + sheet.Id && t.Completed == false);
            if (UserTask != null)
            {
                UserTask.Complete(user);
                await _taskingService.Update(UserTask);
            }
            //run email
            
        }

        public async Task<string> GetMilestone(IFormCollection collection)
        {
            Dictionary<string, object> JsonObjects = new Dictionary<string, object>();
            Guid.TryParse(collection["MilestoneViewModel.Programme"].ToString(), out Guid ProgrammeId);
            string programmeProcess = collection["MilestoneViewModel.ProgrammeProcesses"].ToString();
            string activity = collection["MilestoneViewModel.Activity"].ToString();
            Milestone Milestone = await GetMilestoneProgrammeId(ProgrammeId);
            if (Milestone != null)
            {
                var ProgrammeProcess = Milestone.ProgrammeProcesses.FirstOrDefault(p => p.Name == programmeProcess);
                if (ProgrammeProcess != null)
                {
                    var Activity = ProgrammeProcess.Activities.FirstOrDefault(a => a.Name == activity);
                    if (Activity != null)
                    {
                        JsonObjects.Add("Advisory", Activity.Advisory);
                        //JsonObjects.Add("UserTask", Activity.UserTask);
                        return GetSerializedModel(JsonObjects);
                    }
                }
            }
            return string.Empty;
        }

        public string GetSerializedModel(object model)
        {
            try
            {
                return JsonConvert.SerializeObject(model,
                    new JsonSerializerSettings()
                    {
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        NullValueHandling = NullValueHandling.Ignore,
                        FloatFormatHandling = FloatFormatHandling.DefaultValue,
                        DateParseHandling = DateParseHandling.DateTime
                    });
            }
            catch (Exception ex)
            {
                return ex.Message;
            }

        }

        public async Task CreateJoinOrganisationTask(User user, User organisationUser, Programme programme, Organisation organisation)
        {
            string URL = "/Organisation/RejoinProgramme/?ProgrammeId=" + programme.Id.ToString() + "&OrganisationId=" + organisation.Id.ToString();
            UserTask programmeUserTask = organisationUser.UserTasks.FirstOrDefault(t => t.URL == URL && t.IsActive == true);
            UserTask removedUserTask = organisationUser.UserTasks.FirstOrDefault(t => t.URL == URL && t.IsActive == true);

            if (programmeUserTask == null)
            {
                programmeUserTask = new UserTask(user, "Rejoin", null)
                {
                    URL = URL,
                    Body = organisationUser.FirstName + " click here to rejoin " + programme.Name,
                    IsActive = true
                };
                removedUserTask = new UserTask(user, "Rejoin", null)
                {
                    URL = URL,
                    Body = organisationUser.FirstName + " click here to rejoin " + programme.Name,
                    IsActive = true
                };

                var programmeUser = programme.BrokerContactUser;
                programmeUser.UserTasks.Add(programmeUserTask);
                organisationUser.UserTasks.Add(removedUserTask);

                await _userService.Update(programmeUser);
                await _userService.Update(organisationUser);
            }
        }

        public async Task CreateAttachOrganisationTask(User user, Programme programme, Organisation organisation)
        {           
            string URL = "/Organisation/RejoinProgramme/?ProgrammeId=" + programme.Id.ToString() + "&OrganisationId=" + organisation.Id.ToString();
            var programmeUser = programme.BrokerContactUser;
            var removedUser = await _userService.GetUserPrimaryOrganisationOrEmail(organisation);

            // Remove the old Task for Rejoin from Broker and RemovedUser
            UserTask programmeUserTask = programmeUser.UserTasks.FirstOrDefault(t => t.URL == URL && t.IsActive == true);
            UserTask removedUserTask = removedUser.UserTasks.FirstOrDefault(t => t.URL == URL && t.IsActive == true);

            if (programmeUserTask != null)
            {
                if (user == programmeUser)
                {
                    programmeUserTask.Complete(programmeUser);
                }
                else
                {
                    removedUserTask.Complete(removedUser);
                }

                programmeUser.UserTasks.Remove(programmeUserTask);
                removedUser.UserTasks.Remove(removedUserTask);


                programmeUserTask = new UserTask(programmeUser, "Attach", null)
                {
                    URL = "/Organisation/AttachOrganisation/?ProgrammeId=" + programme.Id.ToString() + "&OrganisationId=" + organisation.Id.ToString(),
                    Body = "Click here to rejoin " + organisation.Name + " to " + programme.Name,
                    IsActive = true
                };

                programmeUser.UserTasks.Add(programmeUserTask);
                await _userService.Update(programmeUser);
            }           
        }

        public async Task CompleteAttachOrganisationTask(User user, Programme programme, Organisation organisation)
        {
            string URL = "/Organisation/AttachOrganisation/?ProgrammeId=" + programme.Id.ToString() + "&OrganisationId=" + organisation.Id.ToString();
            UserTask userTask = user.UserTasks.FirstOrDefault(t => t.URL == URL && t.IsActive == true);
            if (userTask != null)
            {
                userTask.Complete(user);
                user.UserTasks.Remove(userTask);
                await _userService.Update(user);
            }
        }

        public async Task CreateRenewNotificationTask(User user, ClientProgramme renewFromProgrammeBase, Organisation renewClientOrg, Programme currentProgramm)
        {
            string URL = "/Home/RenewNotification/?renewfromprogrammebaseid=" + renewFromProgrammeBase.Id.ToString() + "&OrganisationId=" + renewClientOrg.Id.ToString() +
                "&ProgrammeId=" + currentProgramm.Id.ToString();
            var renewOrgContactUser = await _userService.GetUserPrimaryOrganisationOrEmail(renewClientOrg);

            DateTime taskduedate = DateTime.Now.AddDays(7);
            if (renewFromProgrammeBase != null)
            {
                if (renewFromProgrammeBase.Agreements.FirstOrDefault() != null)
                {
                    taskduedate = renewFromProgrammeBase.Agreements.FirstOrDefault().ExpiryDate.AddDays(14);
                }
            }
            
            UserTask renewOrgContactUserTask = renewOrgContactUser.UserTasks.FirstOrDefault(t => t.URL == URL && t.IsActive == true);

            if (renewOrgContactUserTask == null)
            {
                renewOrgContactUserTask = new UserTask(user, "Renew Notification", null)
                {
                    URL = URL,
                    Body = renewOrgContactUser.FirstName + " please click here to renew " + renewFromProgrammeBase.BaseProgramme.NamedPartyUnitName + " insurance",
                    IsActive = true,
                    DueDate = taskduedate
                };

                renewOrgContactUser.UserTasks.Add(renewOrgContactUserTask);

                await _userService.Update(renewOrgContactUser);
            }
        }

        public async Task CreateRenewTask(User user, ClientProgramme renewFromProgrammeBase, Organisation renewClientOrg, Programme currentProgramm)
        {
            string URL = "/Home/RenewNotification/?renewfromprogrammebaseid=" + renewFromProgrammeBase.Id.ToString() + "&OrganisationId=" + renewClientOrg.Id.ToString() + 
                "&ProgrammeId=" + currentProgramm.Id.ToString();
            var renewOrgContactUser = await _userService.GetUserPrimaryOrganisationOrEmail(renewClientOrg);

            // Remove the old Task
            UserTask renewOrgContactUserTask = renewOrgContactUser.UserTasks.FirstOrDefault(t => t.URL == URL && t.IsActive == true);

            if (renewOrgContactUserTask != null)
            {
                if (user == renewOrgContactUser)
                {
                    renewOrgContactUserTask.Complete(renewOrgContactUser);
                }
                else
                {
                    //User does not match
                }

                renewOrgContactUser.UserTasks.Remove(renewOrgContactUserTask);

            }
        }

        public async Task RemoveTask(User user, IFormCollection collection)
        {
            List<UserTask> tasks = await _taskingService.GetUserTasksByName(collection["taskName"]);

            foreach(UserTask task in tasks)
            {
                if (task.URL.Contains(collection["programmeId"]) && task.URL.Contains(collection["organisationId"]))
                {
                    task.Removed = true;
                    task.DateDeleted = DateTime.Now;
                    task.DeletedBy = user; 
                    await _taskingService.Update(task);
                }
            }
        }
    }
}
