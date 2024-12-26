using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;

namespace DealEngine.Services.Impl
{
    public class TerritoryService : ITerritoryService
    {      
        IMapperSession<Territory> _territoryRepository;
        IMapperSession<TerritoryTemplate> _territoryTemplateRepository;

        public TerritoryService(IMapperSession<Territory> territoryRepository,  IMapperSession<TerritoryTemplate> territoryTemplateRepository)
        {
            _territoryRepository = territoryRepository;
            _territoryTemplateRepository = territoryTemplateRepository;
        }

        public async Task AddTerritoryTemplate(TerritoryTemplate territoryTemplate)
        {
            await _territoryTemplateRepository.AddAsync(territoryTemplate);
        }

        public async Task<Territory> CreateTerritory(Guid guid)
        {
            var template= await _territoryTemplateRepository.GetByIdAsync(guid);
            Territory territory = new Territory(null)
            {
                Location = template.Location,
                TemplateId = template.Id
            };
            await _territoryRepository.AddAsync(territory);
            return territory;
        }

        public async Task CreateTerritoryTemplate(string LocationName)
        {
            TerritoryTemplate territoryTemplate = new TerritoryTemplate(null, LocationName);
            territoryTemplate.Ispublic = true;
            await _territoryTemplateRepository.AddAsync(territoryTemplate);
        }

        public async Task<TerritoryTemplate> GetTerritoryTemplateByName(string LocationName)
        {
            if(LocationName == "New Zealand")
            {
                var territory = await _territoryTemplateRepository.FindAll().FirstOrDefaultAsync(t => t.Location == LocationName);
                if(territory == null)
                {
                    await CreateTerritoryTemplate("New Zealand");
                }                
            }

            return await _territoryTemplateRepository.FindAll().FirstOrDefaultAsync(t => t.Location == LocationName);
        }


        public async Task<List<Territory>> GetTerritory(Guid guid)
        {

            return await _territoryRepository.FindAll().Where(t => t.Id == guid).ToListAsync();
        }






    }
}

