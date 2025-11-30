using DealEngine.Services.Interfaces;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Domain.Entities;
using System.Threading.Tasks;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.Services.Impl
{
    public class EventsInfoService : IEventsInfoService
    {
        IMapperSession<EventsInfo> _EventsInfoRepository;

        public EventsInfoService(IMapperSession<EventsInfo> EventsRepository)
        {
            _EventsInfoRepository = EventsRepository;
        }

        public async Task<EventsInfo> GetEventsById(Guid eventId)
        {
            return await _EventsInfoRepository.GetByIdAsync(eventId);
        }

        public async Task<EventsInfo> GetEventsByName(Guid sheet, String assetname)
        {
            return await _EventsInfoRepository.FindAll().FirstOrDefaultAsync(asset => asset.ClientInformationSheet.Id== sheet && asset.EventName == assetname);
        }
        //public async Task UpdateEvents(List<ClubTrustAssetsInfo> clubTrustAssetsInfolist)
        //{
        //    foreach(var clubTrustAssetsInfo in clubTrustAssetsInfolist)
        //    await _EventsInfoRepository.UpdateAsync(clubTrustAssetsInfo);
        //}
        public async Task DeleteEventsById(User deletedBy, EventsInfo eventsInfo)
        {
            eventsInfo.Delete(deletedBy);
            await UpdateEvents(eventsInfo);
        }
        public async Task<IList<EventsInfo>> GetEvents(Guid sheetId)
        {
            return  _EventsInfoRepository.FindAll().Where(Event => Event.ClientInformationSheet.Id == sheetId && Event.DateDeleted == null).ToList();
        }

        public async Task UpdateEvents(EventsInfo eventsInfoo)
        {
                await _EventsInfoRepository.UpdateAsync(eventsInfoo);
        }

      
    }
}

