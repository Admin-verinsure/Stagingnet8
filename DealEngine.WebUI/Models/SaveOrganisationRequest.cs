using System;

namespace DealEngine.WebUI.Models
{
    public class SaveOrganisationRequest
    {
        public Guid orgId { get; set; }

        // CLUB
        public int? ActiveFeePaying { get; set; }
        public int? Honorary { get; set; }
        public int? Associate { get; set; }
        public int? Family { get; set; }
        public int? Community { get; set; }
        public int? Volunteer { get; set; }
        public int? Corporate { get; set; }
        public int? Alumni { get; set; }
        public int? Trustees { get; set; }
        public int? OtherMembers { get; set; }
        public int? ClubTotal { get; set; }

        // DISTRICT
        public int? Dist_Rotary { get; set; }
        public int? Dist_Rotaract { get; set; }
        public int? Dist_Interact { get; set; }
        public int? Dist_RotaKids { get; set; }
        public int? Dist_CommunityCore { get; set; }
        public int? DistrictTotal { get; set; }

        // SPT
        public int? SPT_Companies { get; set; }
        public int? SPT_TradingTrusts { get; set; }
        public string SPT_RevenueOver1m { get; set; }
        public int? SPT_Revenue { get; set; }
        public int? SPT_Total { get; set; }
    }
}
