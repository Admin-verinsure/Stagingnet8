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
    public class ClubAssetInfoService : IClubAssetInfoService
    {
        IMapperSession<ClubTrustAssetsInfo> _ClubTrustAssetsInfoRepository;

        public ClubAssetInfoService(IMapperSession<ClubTrustAssetsInfo> locationRepository)
        {
            _ClubTrustAssetsInfoRepository = locationRepository;
        }

        public async Task<ClubTrustAssetsInfo> GetClubAssetById(Guid ClubAssetId)
        {
            return await _ClubTrustAssetsInfoRepository.GetByIdAsync(ClubAssetId);
        }

        //public async Task UpdateClubAsset(List<ClubTrustAssetsInfo> clubTrustAssetsInfolist)
        //{
        //    foreach(var clubTrustAssetsInfo in clubTrustAssetsInfolist)
        //    await _ClubTrustAssetsInfoRepository.UpdateAsync(clubTrustAssetsInfo);
        //}
        public async Task DeleteClubAssetById(User deletedBy, ClubTrustAssetsInfo clubTrustAssetsInfo)
        {
            clubTrustAssetsInfo.Delete(deletedBy);
            await UpdateClubAsset(clubTrustAssetsInfo);
        }

        public async Task UpdateClubAsset(ClubTrustAssetsInfo clubTrustAssetsInfo)
        {
                await _ClubTrustAssetsInfoRepository.UpdateAsync(clubTrustAssetsInfo);
        }

      
    }
}

