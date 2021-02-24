using System;
using System.Collections.Generic;
using System.Linq;
using DealEngine.Domain.Entities;

namespace DealEngine.WebUI.Models
{
    public class ClaimViewModel
    {
     

        public Guid AnswerSheetId { get; set; }

        public Guid ClaimId { get; set; }

        public string ClaimTitle { get; set; }
        
        public string ClaimDescription { get; set; }

        public string ClaimDateOfLoss { get; set; }

        public string ClaimInsuredName { get; set; }

        public string ClaimNotifiedDate { get; set; }

        public string Claimant { get; set; }

        public decimal ClaimEstimateInsuredLiability { get; set; }

        public decimal ClaimPaid { get; set; }

        public decimal ClaimReserve { get; set; }

        public string ClaimNotes { get; set; }

        public string ClaimReference { get; set; }

        public string ClaimPolicyReference { get; set; }

        public string ClaimBrokerReference { get; set; }

        public string ClaimInsurerReference { get; set; }

        public string ClaimInsurerName { get; set; }

        public string ClaimStatus { get; set; }

        public string SelectedClaimProducts { get; set; }

        public string SelectedResponsiblePrincipal { get; set; }

        public Guid OrganisationId { get; set; }

        public Guid[] ClaimProducts { get; set; }

        //public ClaimViewModel(ClientInformationSheet ClientInformationSheet)
        //{
        //    claim = GetLocations(ClientInformationSheet);
        //    LocationType = GetLocationTypes();

        //}
        public ClaimNotification ToEntity(User creatingUser)
        {
            ClaimNotification claim = new ClaimNotification(creatingUser);
            UpdateEntity(claim);
            return claim;
        }

        public ClaimNotification UpdateEntity(ClaimNotification claim)
        {
            claim.ClaimTitle = ClaimTitle;
            claim.ClaimDescription = ClaimDescription;
            claim.ClaimInsuredName = ClaimInsuredName;
            claim.Claimant = Claimant;
            claim.ClaimEstimateInsuredLiability = ClaimEstimateInsuredLiability;
            claim.ClaimPaid = ClaimPaid;
            claim.ClaimReserve = ClaimReserve;
            claim.ClaimReference = ClaimReference;
            claim.ClaimPolicyReference = ClaimPolicyReference;
            claim.ClaimBrokerReference = ClaimBrokerReference;
            claim.ClaimInsurerReference = ClaimInsurerReference;
            claim.SelectedClaimProducts = SelectedClaimProducts;
            claim.SelectedResponsiblePrincipal = SelectedResponsiblePrincipal;
            claim.ClaimInsurerName = ClaimInsurerName;
            claim.ClaimStatus = "Precautionary notification only";
            claim.ClaimNotes = ClaimNotes;
            if (!string.IsNullOrEmpty(ClaimDateOfLoss))
            {
                claim.ClaimDateOfLoss = DateTime.Parse(ClaimDateOfLoss, System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"));
            }
            else
            {
                claim.ClaimDateOfLoss = DateTime.MinValue;
            }
            if (!string.IsNullOrEmpty(ClaimNotifiedDate))
            {
                claim.ClaimNotifiedDate = DateTime.Parse(ClaimNotifiedDate, System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"));
            }
            else
            {
                claim.ClaimNotifiedDate = DateTime.MinValue;
            }

            return claim;
        }

        public static ClaimViewModel FromEntity(ClaimNotification claim)
        {
            ClaimViewModel model = new ClaimViewModel
            {
                ClaimId = claim.Id,
                ClaimTitle = claim.ClaimTitle,
                ClaimDescription = claim.ClaimDescription,
                ClaimInsuredName = claim.ClaimInsuredName,
                Claimant = claim.Claimant,
                ClaimEstimateInsuredLiability = claim.ClaimEstimateInsuredLiability,
                ClaimPaid = claim.ClaimPaid,
                ClaimReserve = claim.ClaimReserve,
                ClaimReference = claim.ClaimReference,
                ClaimPolicyReference = claim.ClaimPolicyReference,
                ClaimBrokerReference = claim.ClaimBrokerReference,
                ClaimInsurerReference = claim.ClaimInsurerReference,
                ClaimInsurerName = claim.ClaimInsurerName,
                ClaimStatus = claim.ClaimStatus,
                SelectedClaimProducts = claim.SelectedClaimProducts,
                SelectedResponsiblePrincipal = claim.SelectedResponsiblePrincipal,
                ClaimProducts = claim.ClaimProducts.Select(p => p.Id).ToArray(),
                ClaimNotes = claim.ClaimNotes,
                ClaimDateOfLoss = (claim.ClaimDateOfLoss > DateTime.MinValue) ? claim.ClaimDateOfLoss.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ")) : "",
                ClaimNotifiedDate = (claim.ClaimNotifiedDate > DateTime.MinValue) ? claim.ClaimNotifiedDate.ToString("dd/MM/yyyy", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ")) : "",
            };

            return model;
        }
    }
}

