using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace DealEngine.WebUI.Models
{
    public class CreateUserViewModel : BaseViewModel
    {

        public CreateUserViewModel()
        {
            OrganisationRoleMappings = new List<OrganisationRoleMapping>();
        }

        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public IList<OrganisationRoleMapping> OrganisationRoleMappings { get; set; }
        public Guid MainOrganisationId { get; set; }
        public string OktaUID { get; set; }
        public string ProposalOnlineUsername { get; set; }
        public string SalespersonUsername { get; set; }
        public string EmployeeNumber { get; set; }
        public string Password { get; set; }
    }

    public class OrganisationRoleMapping
    {
        public Guid OrganisationId { get; set; }

        public string RoleName { get; set; }

        public OrganisationRoleMapping() {}
        
        //public IList<string> RoleIds { get; set; }

        //public OrganisationRoleMapping()
        //{
        //    RoleIds = new List<string>();
        //}
    }

}
