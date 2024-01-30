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
            SelectedOrganisations = new List<Guid>();
            Roles = new List<string>();
        }

        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string HomePhone { get; set; }
        public string MobilePhone { get; set; }
        public IList<string> Roles { get; set; }
        public IList<Guid> SelectedOrganisations { get; set; }
        public Guid MainOrganisationId { get; set; }
        public string OktaUID { get; set; }
        public string SalespersonUsername { get; set; }
        public string EmployeeNumber { get; set; }
        public string Password { get; set; }
    }
}
