using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface IClubAssetInfoService
    {
        Task<ClubTrustAssetsInfo> GetClubAssetById(Guid ClubAssetId);
        Task UpdateClubAsset(ClubTrustAssetsInfo clubTrustAssetsInfolist);
         Task DeleteClubAssetById(User deletedBy, ClubTrustAssetsInfo clubTrustAssetsInfo);
        Task<IList<ClubTrustAssetsInfo>> GetClubTrustAssets(Guid sheet);
        Task<ClubTrustAssetsInfo> GetClubAssetByName(Guid sheet,String assetname);

        //Task UpdateClubAsset(List<ClubTrustAssetsInfo> clubTrustAssetsInfolist);
    }
}
