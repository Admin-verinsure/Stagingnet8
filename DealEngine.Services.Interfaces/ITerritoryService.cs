
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface ITerritoryService
    {
        Task<TerritoryTemplate> GetTerritoryTemplateByName(string LocationName);
        Task AddTerritoryTemplate(TerritoryTemplate TerritoryTemplate);
        Task<Territory> CreateTerritory(Guid guid);
        Task<List<Territory>> GetTerritory(Guid guid);

    }
}

 
