using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DealEngine.Domain.Entities;

namespace DealEngine.WebUI.Models.Agreement
{
    public class EditExtensionTermsViewModel : BaseViewModel
    {
        public Guid clientAgreementId { get; set; }
        public Guid TermId { get; set; }
        public int TermLimit { get; set; }

        public decimal Premium { get; set; }
        public decimal Excess { get; set; }
      
    }
}