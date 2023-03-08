using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DealEngine.WebUI.Models
{
	public class UserViewModel : BaseViewModel
	{

		public UserViewModel()
        {
			User = new User(null, null);
			BooleanOptions = GetBooleanOptions();
		}
		public Guid ID { get; set; }
		public string FullName { get; set; }
		public User User { get; set; }
		public IList<SelectListItem> BooleanOptions { get; set; }
		public Domain.Entities.Organisation Organisation { get; set; }
        public IList<Domain.Entities.Organisation> Organisations { get; set; }
        public string Organisationselected { get; set; }

    }
}

