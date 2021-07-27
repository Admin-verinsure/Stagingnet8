using System;
using System.Collections.Generic;
using System.Linq;
using DealEngine.Domain.Entities;

namespace DealEngine.WebUI.Models
{
	public class DashboardViewModel : BaseViewModel
	{
		public bool DisplayProducts { get; set; }
		public bool DisplayDeals { get; set; }
		public IList<ProductItemV2> ProductItems { get; set; }
		public IList<ProductItem> DealItems { get; set; }
		public IList<ProgrammeItem> ProgrammeItems { get; set; }
		public IList<TaskItem> CriticalTaskItems { get; set; }

		public IList<TaskItem> ImportantTaskItems { get; set; }

		public string CurrentUserType { get; set; }
		public IList<UserTask> UserTasks { get; set; }
	}


	public class ButtonItem
	{
		public string Text { get; set; }

		public string RedirectLink { get; set; }

		public string Classes { get; set; }
	}

	public class ProductItem
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public IList<string> Languages { get; set; }

		public IList<ButtonItem> Buttons { get; set; }

		public string RedirectLink { get; set; }

		public string Status { get; set; }

		public bool HasProduct { get; set; }
	}

	public class ProductItemV2
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public string SheetId { get; set; }

		public IList<string> Languages { get; set; }

		public IList<KeyValuePair<string, Guid>> SheetHistory { get; set; }

		public bool HasSheetHistory
		{
			get { return SheetHistory != null && SheetHistory.Count > 1; }
		}

		public string RedirectLink { get; set; }

		public string Status { get; set; }
	}
//	public class UpdateType
//    {
//		public string ValueType { get; set; }
//		public string NameType { get; set; }
//		public object DateDeleted { get; internal set; }
//		public Guid Id { get; set; }
//		public List<UpdateTypesViewModel> UpdateTypes { get; set; }
//		public List<UpdateTypesViewModel> updateType { get; set; }
//		public virtual bool TypeIsTc {get; set;}
//		public virtual bool TypeIsBroker { get; set; }
//		public virtual bool TypeIsInsurer { get; set; }
//		public virtual bool TypeIsClient { get; set; }
//}
	public class DealItem
	{
		public string Name { get; set; }

		public string Description { get; set; }

		public string Status { get; set; }

		public string ReferenceId { get; set; }

		public string Id { get; set; }

		public string LocalDateCreated { get; set; }

		public string LocalDateSubmitted { get; set; }
		public Boolean NextInfoSheet { get; set; }
		public Boolean ProgrammeAllowUsesChange { get; set; }
		public Boolean ProgrammeUseEglobal { get; set; }
		public IList<SubClientProgramme> SubClientProgrammes { get; set; }
		public string AgreementStatus { get; set; }
		public string DocSendDate { get; set; }
		public Boolean IsSubclientSubmitted { get; set; }
		public bool IsChange { get; set; }
		public string GetStatusDisplay(string agreementstatus)
		{
			List<string> statusDisplay = new List<string>();

			if (!string.IsNullOrWhiteSpace(LocalDateCreated))
				statusDisplay.Add("Created on " + LocalDateCreated);
			if (!string.IsNullOrWhiteSpace(LocalDateSubmitted))
				statusDisplay.Add("UIS Completed on " + LocalDateSubmitted);
			if (agreementstatus != "")
			{
				statusDisplay.Add(" Agreement Status: " + agreementstatus);

			}
			return string.Join(", ", statusDisplay);
		}


	}

	public class ProgrammeItem : BaseViewModel
	{
		[Obsolete]
	
        public ProgrammeItem(Domain.Entities.Programme baseProgramme)
        {
			OrganisationViewModel = new OrganisationViewModel(null, null);
			Programme = baseProgramme;
			Deals = new List<DealItem>();
		}

        public ProgrammeItem(List<ClientInformationSheet> sheets)
        {
			OrganisationViewModel = new OrganisationViewModel(null, null);
			Programme = sheets.FirstOrDefault().Programme.BaseProgramme;
			BuildDeals(sheets);
        }
        public ProgrammeItem(Domain.Entities.UpdateType updateType)
        {
            updateTypesViewModel = new UpdateTypesViewModel();
            UpdateType = updateType;
            updateTypes = new List<UpdateType>();


        }
        private void BuildDeals(List<ClientInformationSheet> sheets)
        {
			Deals = new List<DealItem>();
			foreach (ClientInformationSheet sheet in sheets)
			{
				ClientProgramme client = sheet.Programme;

				string status = client.InformationSheet.Status;
				string localDateCreated = LocalizeTime(client.InformationSheet.DateCreated.GetValueOrDefault(), "dd/MM/yyyy");
				string localDateSubmitted = null;

				if (client.InformationSheet.Status != "Not Started" && client.InformationSheet.Status != "Started")
				{
					localDateSubmitted = LocalizeTime(client.InformationSheet.SubmitDate, "dd/MM/yyyy");
				}

				Deals.Add(new DealItem
				{
					Id = client.Id.ToString(),
					Name = sheet.Programme.BaseProgramme.Name + " for " + client.Owner.Name,
					LocalDateCreated = localDateCreated,
					LocalDateSubmitted = localDateSubmitted,
					Status = status,
					SubClientProgrammes = client.SubClientProgrammes,
					ReferenceId = client.InformationSheet.ReferenceId// Move into ClientProgramme?
				});
			}
		}
		public List<string> SelectedUpdateTypes { get; set; }
		public IList<string> Languages { get; set; }
		public IList<DealItem> Deals { get; set; }
		public IList<UpdateType> updateTypes { get; set; }

		public string CurrentUserIsBroker { get; set; }
		public string CurrentUserIsInsurer { get; set; }
		public string CurrentUserIsTC { get; set; }
		public string CurrentUserIsProgrammeManager { get; set; }
		public string CurrentUserIsClient { get; set; }
		public bool IsSubclientEnabled { get; set; }
		public OrganisationViewModel OrganisationViewModel { get; set; }
		public UpdateTypesViewModel updateTypesViewModel { get; set; }

		public Domain.Entities.Programme Programme { get; internal set; }
		public Domain.Entities.UpdateType UpdateType { get; internal set; }

		public ChangeReason ChangeReason { get; set; }

        public List<UpdateTypesViewModel> UpdateTypes { get; internal set; }
   
		public bool IsRenewFromProgramme { get; set; }

	}


	public class TaskItem : BaseViewModel
	{
		public Guid Id { get; set; }


		public string ClientName { get; set; }

		public string Description { get; set; }

		public string Details { get; set; }

		public string TaskUrl { get; set; }

		public int Priority { get; set; }

		public string DueDate { get; set; }

		public bool Completed { get; set; }
	}

}
