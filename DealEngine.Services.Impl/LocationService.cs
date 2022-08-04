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
    public class LocationService : ILocationService
    {
        IMapperSession<Location> _locationRepository;

        public LocationService(IMapperSession<Location> locationRepository)
        {
            _locationRepository = locationRepository;
        }

        public async Task<Location> GetLocationById(Guid locationId)
        {
            return await _locationRepository.GetByIdAsync(locationId);
        }

        public async Task<Location> GetLocationByStreet(string street)
        {
            return await _locationRepository.FindAll().FirstOrDefaultAsync(l => l.Street == street);
        }

        public async Task<List<string>> GetLocationStreetList()
        {
            return await _locationRepository.FindAll().Select(l => l.Street).ToListAsync();
        }

        public async Task UpdateLocation(Location location)
        {
            await _locationRepository.UpdateAsync(location);
        }

    }
}

