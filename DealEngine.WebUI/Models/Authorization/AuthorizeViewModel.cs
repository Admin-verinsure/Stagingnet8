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
        public IList<Domain.Entities.Organisation> OrganisationList { get; set; }
        public Dictionary<string, List<string>> RoleClaims { get; set; }        
        public bool IsTCUser { get; set; }
        public bool isMarshUser { get; set; }
        public bool IsProgrammeManagerCoastguard { get; set; }
        public string SelectedUserId { get; set; }
        public string SelectedUserFirstName { get; set; }
        public string SelectedUserLastName { get; set; }
        public string SelectedUserEmail { get; set; }
        public IList<string> SelectedUserRoles { get; set; }
    }
}

