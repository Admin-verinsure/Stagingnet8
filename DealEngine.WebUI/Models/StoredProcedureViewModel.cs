using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;

namespace DealEngine.WebUI.Models
{
	public class StoredProcedureViewModel : BaseViewModel
	{

		public StoredProcedureViewModel()
        {
			//User = new User(null, null);
			//BooleanOptions = GetBooleanOptions();
		}

		public Guid Id { get; set; }
		public string BoatName { get; set; }

		public List<Object> Objects { get; set; }
		//public string FullName { get; set; }
		//public User User { get; set; }
		//public IList<SelectListItem> BooleanOptions { get; set; }
		//public Domain.Entities.Organisation Organisation { get; set; }
	}
}

