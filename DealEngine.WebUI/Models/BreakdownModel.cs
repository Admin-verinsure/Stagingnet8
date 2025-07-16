//using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using System;
using System.Linq;

namespace DealEngine.WebUI.Models
{
    public class BreakdownModel
    {
        public BreakdownModel(Domain.Entities.Programme programme)
        {
            if(programme != null)
            {
                Programme = programme;
                PopulateModel(programme);
            }
        }

        private void PopulateModel(Domain.Entities.Programme programme)
        {
            var cProgrammes = programme.ClientProgrammes.Where(p => p.DateDeleted == null).ToList();
            NotStartedCount = 0;
            StartedCount = 0;
            CompletedCount = 0;
            PrivateCount = 0;
            TrustCount = 0;
            PartnershipCount = 0;
            CorporationCount = 0;
            QuotedCount = 0;
            BoundCount = 0;
            foreach (var prog in cProgrammes)
            {
                if (prog.Owner.OrganisationType.Name == "Person - Individual")
                {
                    PrivateCount++;
                }
                else if (prog.Owner.OrganisationType.Name == "Corporation – Limited liability")
                {
                    CorporationCount++;
                }
                else if (prog.Owner.OrganisationType.Name == "Trust")
                {
                    TrustCount++;
                }
                else if (prog.Owner.OrganisationType.Name == "Partnership")
                {
                    PartnershipCount++;
                }

                if (prog.InformationSheet.Status == "Not Started")
                {
                    NotStartedCount++;
                }
                else if (prog.InformationSheet.Status == "Submitted")
                {
                    CompletedCount++;
                }
                else
                {
                    StartedCount++;
                }
            }
            foreach (var prog in cProgrammes)
            {
                foreach(var agreement in prog.Agreements)
                {
                    if(agreement.Status == "Bound and pending payment" 
                        || agreement.Status == "Bound and invoice pending" 
                        || agreement.Status == "Bound and invoiced"
                        || agreement.Status == "Bound")
                    {
                        BoundCount++;
                    }
                    else if (agreement.Status == "Quoted")
                    {
                        QuotedCount++;
                    }
                }
            }
        }

        public Domain.Entities.Programme Programme { get; set; }
        public int NotStartedCount { get; set; }
        public int StartedCount { get; set; }
        public int CompletedCount { get; set; }
        public int QuotedCount { get; set; }
        public int BoundCount { get; set; }
        public int PrivateCount { get; set; }
        public int TrustCount { get; set; }
        public int PartnershipCount { get; set; }
        public int CorporationCount { get; set; }
    }
}
