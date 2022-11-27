using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface IAssetData
    {
        Task<AssetData> GetAssetDataById(Guid AssetDataId);
        Task UpdateAssetData(AssetData AssetData);
        Task<AssetData> GetAssetDataBySheetId(Guid sheetid);

    }
}
