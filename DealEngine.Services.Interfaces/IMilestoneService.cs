using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace DealEngine.Services.Interfaces
{
    public interface IMilestoneService
    {      
        Task<Milestone> GetMilestoneProgrammeId(Guid programmeId);
        Task CompleteMilestoneFor(string activityType, User user, ClientInformationSheet sheet);
        Task CreateMilestone(User user, IFormCollection collection);
        Task<string> SetMilestoneFor(string activity, User user, ClientInformationSheet sheet);
        Task<string> GetMilestone(IFormCollection collection);
        Task CreateJoinOrganisationTask(User user, User organisationUser, Programme programme, Organisation organisation);
        Task CreateAttachOrganisationTask(User user, Programme programme, Organisation organisation);
        Task CompleteAttachOrganisationTask(User user, Programme programme, Organisation organisation);
        Task RemoveTask(User user, IFormCollection collection);

        Task CreateRenewNotificationTask(User user, ClientProgramme renewFromProgrammeBase, Organisation renewClientOrg, Programme currentProgramm);
        Task CreateRenewTask(User user, ClientProgramme renewFromProgrammeBase, Organisation renewClientOrg, Programme currentProgramm);
    }   
}
