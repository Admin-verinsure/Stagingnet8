using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace DealEngine.WebUI.Models.Agreement
{
    public class ViewAgreementChangeReasonViewModel : BaseViewModel
    {
        public Guid ClientAgreementID { get; set; }
        public Guid InformationSheetID { get; set; }
        public Guid ClientProgrammeID { get; set; }
        public Guid ClientAgreementChangeReasonID { get; set; }
        public string ChangeType { get; set; }
        public string Reason { get; set; }
        public string Description { get; set; }
        public string EffectiveDate { get; set; }

    }

}
