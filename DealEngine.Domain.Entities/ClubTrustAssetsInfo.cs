using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Domain.Entities.Abstracts;

namespace DealEngine.Domain.Entities
{
    public class ClubTrustAssetsInfo : EntityBase, IAggregateRoot
    {
        public   ClubTrustAssetsInfo() : base(null) { }

        public  ClubTrustAssetsInfo(User createdBy) : base(createdBy)
        {
        }

        public ClubTrustAssetsInfo(string name, int currentval, int replacementval, Organisation owner,ClientInformationSheet sheet, User user)
           : base(user)
        {
            Name = name;
            CurrentVal = currentval;
            ReplacementVal = replacementval;
            Owner = owner;
            ClientInformationSheet = sheet;
        }

        public virtual string Name { get; set; }
        public virtual int CurrentVal { get; set; }
        public virtual int ReplacementVal { get; set; }
        public virtual Organisation Owner { get; set; }
        public virtual  ClientInformationSheet ClientInformationSheet { get; set; }
        //public virtual IList<ClubTrustAssetsInfo> clubTrustAssetsInfo { get; set; }


    }
}

