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
    public class AssetDataService : IAssetData
    {
        IMapperSession<AssetData> _AssetDataRepository;

        public AssetDataService(IMapperSession<AssetData> AssetData)
        {
            _AssetDataRepository = AssetData;
        }

        public async Task<AssetData> GetAssetDataById(Guid AssetData)
        {
            return await _AssetDataRepository.GetByIdAsync(AssetData);
        }

        public async Task UpdateAssetData(AssetData AssetData)
        {
            await _AssetDataRepository.UpdateAsync(AssetData);
        }
        public async Task<AssetData> GetAssetDataBySheetId(Guid sheetid)
        {
            return await _AssetDataRepository.FindAll().FirstOrDefaultAsync(asset => asset.clientInformationSheet.Id == sheetid);
        }



    }
}

