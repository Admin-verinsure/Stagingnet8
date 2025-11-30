using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface IEventsInfoService
    {
        Task<EventsInfo> GetEventsById(Guid EventId);
        Task UpdateEvents(EventsInfo eventsInfolist);
         Task DeleteEventsById(User deletedBy, EventsInfo eventsInfo);
        Task<IList<EventsInfo>> GetEvents(Guid sheet);
        Task<EventsInfo> GetEventsByName(Guid sheet,String eventname);

        //Task UpdateClubAsset(List<ClubTrustAssetsInfo> clubTrustAssetsInfolist);
    }
}
