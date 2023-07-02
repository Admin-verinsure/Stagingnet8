using System;
using System.Collections.Generic;

namespace DealEngine.WebUI.Models.Agreement
{
	public class MyAgreementsViewModel : BaseViewModel
	{
		public IList<AgreementViewModel> MyAgreements { get; set; }
	}

	public class AgreementViewModel
	{
		public string InformationName { get; set; }
		public Guid InformationId { get; set; }
        public Guid AgreementId { get; set; }
        public decimal RefferLodPrc { get; set; }
        public decimal RefferLodAmt{ get; set; }
        public string AdditionalNotes { get; set; }
        public string Status { get; set; }
        public string DeclineNotes { get; set; }
        public string CancellNotes { get; set; }
        public DateTime CancellEffectiveDate { get; set; }
        public string CancelAgreementReason { get; set; }       
        public DateTime InceptionDate { get; set; }
        public DateTime ExpiryDate { get; set; }

    }
}

