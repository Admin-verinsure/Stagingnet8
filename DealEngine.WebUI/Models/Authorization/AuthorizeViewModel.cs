using NHibernate.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DealEngine.Domain.Entities;

namespace DealEngine.WebUI.Models.Authorization
{
	public class AuthorizeViewModel : BaseViewModel
	{
        public IList<Claim> ClaimList { get; set; }
        public IList<IdentityRole> RoleList { get; set; }
        public IList<User> UserList { get; set; }
        public IList<Domain.Entities.Organisation> Organisations { get; set; }
        public Dictionary<string, List<string>> RoleClaims { get; set; }
    }
}

