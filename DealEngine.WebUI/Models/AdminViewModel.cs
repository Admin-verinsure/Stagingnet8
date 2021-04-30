using System;
using System.Collections.Generic;
using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using NHibernate.AspNetCore.Identity;

namespace DealEngine.WebUI.Models
{
    public class AdminViewModel : BaseViewModel
    {
        // public UpdateTypesViewModel UpdateTypesViewModel { get; set; }
        //public List<UpdateTypesViewModel> UpdateTypes { get; set; }

        //public IList<UpdateTypesViewModel> UpdateTypes { get; set; }

        public IList<PrivateServerViewModel> PrivateServers { get; set; }
        public IList<PaymentGatewayViewModel> PaymentGateways { get; set; }
        public IList<MerchantViewModel> Merchants { get; set; }
        public IList<SelectListItem> LockedUsers { get; set; }
        public List<IdentityUser> Users { get; internal set; }
       
    }
}