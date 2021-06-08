using System.Collections.Generic;
using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DealEngine.WebUI.Models
{
	public class IssueUISViewModel : BaseViewModel
	{		
		public List<ClientProgramme> ClientProgrammes { get; set; }
		public DealEngine.Domain.Entities.Programme programme { get; set; }

		public string ProgrammeId { get; set; }
		public string ProgrammeName { get; set; }
		public List<User> users { get; set; }
		public string IsSubUIS { get; set; }
		public List<SelectListItem> ListClientProgrammes { get; set; }
		public List<SelectListItem> ListQueries { get; set; }
		public  bool EnablePIReport { get; set; }
		public  bool EnableEDReport { get; set; }
		public  bool EnableCLReport { get; set; }
		public  bool EnableCyberReport { get; set; }
		public  bool EnableFAPReport { get; set; }
		public string ProgrammeNamedPartyUnitName { get; set; }

	}
}

