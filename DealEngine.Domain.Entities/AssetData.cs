using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Domain.Entities.Abstracts;

namespace DealEngine.Domain.Entities
{
    public class AssetData : EntityBase, IAggregateRoot
    {
        public AssetData() : base(null) { }

        public AssetData(string hasClubTrustAssets, string hasClubTrustAssetMore, IList<ClubTrustAssetsInfo> clubTrustAssetsInfolist,ClientInformationSheet sheet, User user = null)
           : base(user) 
        {
            HasClubTrustAssets = hasClubTrustAssets;
            HasClubTrustAssetMore = hasClubTrustAssetMore;
            ClubTrustAssetsInfo = GetClubTrustAssetsInfo(clubTrustAssetsInfolist);
            clientInformationSheet = sheet;

        }
        //public ClubTrustAssetsInfo(ClientInformationSheet sheet, User user = null)
        //   : base(user)
        //{
        //    clubTrustAssetsInfo = new List<SharedDataRole>();
        //    if (sheet != null)
        //    {
        //        DataRoles = CreateRoles(sheet);
        //    }
        //}
        public virtual  string HasClubTrustAssets { get; set; }
        public virtual  string HasClubTrustAssetMore { get; set; }
        public virtual IList<ClubTrustAssetsInfo> ClubTrustAssetsInfo { get; set; }
        public virtual ClientInformationSheet clientInformationSheet { get; set; }
        private IList<ClubTrustAssetsInfo> GetClubTrustAssetsInfo(IList<ClubTrustAssetsInfo> clubTrustAssetsInfolist)
        {
            List<ClubTrustAssetsInfo>  ClubTrustAssetsInfo = new List<ClubTrustAssetsInfo>();

            foreach (var trustdate in clubTrustAssetsInfolist)
            {
                ClubTrustAssetsInfo.Add(trustdate);
            }

            return ClubTrustAssetsInfo;
        }

    }
}

