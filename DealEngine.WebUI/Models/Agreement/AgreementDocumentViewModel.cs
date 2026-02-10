using System;

namespace DealEngine.WebUI.Models.Agreement
{
	public class AgreementDocumentViewModel : BaseViewModel
	{
		public string DisplayName { get; set; }
		public string Url { get; set; }
		public Guid ClientAgreementId { get; set; }
		public int DocType { get; set; }
		public bool RenderToPDF { get; set; }
        public string ContentType { get; set; }
    }
}

