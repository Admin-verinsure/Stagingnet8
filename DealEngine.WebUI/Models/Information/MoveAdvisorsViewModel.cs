using DealEngine.Domain.Entities;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Linq;


namespace DealEngine.WebUI.Models.Information
{
    public class MoveAdvisorsViewModel : BaseViewModel
    {
        public Guid id { get; set; }
        [Display(Name = "List of Advisors")]
        public IList<SelectListItem> Advisors { get; set; }
        public IList<SelectListItem> UniqueOwners { get; set; }

        public MoveAdvisorsViewModel(Guid clientProgrammeId,String clientProgrammename, IList<Domain.Entities.Organisation> advisors, IList<ClientProgramme> clientProgrammes) 
        {
            id = clientProgrammeId;
            SourceClientProgrammeName = clientProgrammename;
            PopulateAdvisorList(advisors);
            PopulateUniqueOwnersList(clientProgrammes);
        }
        private void PopulateAdvisorList(IList<Domain.Entities.Organisation> advisors)
        {
            Advisors = new List<SelectListItem>();

            foreach (Domain.Entities.Organisation advisor in advisors)
            {
                Advisors.Add(new SelectListItem()
                {
                    Text = advisor.Name,
                    Value = advisor.Id.ToString()
                });
            }
            Advisors = Advisors.OrderBy(a => a.Text).ToList();
        }
        private void PopulateUniqueOwnersList(IList<ClientProgramme> clientProgrammes)
        {
            var owners = new List<KeyValuePair<Guid, List<string>>>();
            UniqueOwners = new List<SelectListItem>();
            SubClientProgramme forTypeComparison = new SubClientProgramme();
            var count = 0;
            try
            {
                foreach (ClientProgramme clientProgramme in clientProgrammes.Where(o => o.InformationSheet.DateDeleted == null && o.InformationSheet.NextInformationSheet == null && o.InformationSheet.Status == "Bound"))
                {

                    count++;
                    if (Object.ReferenceEquals(clientProgramme.GetType(), forTypeComparison.GetType()))
                    {
                        continue;
                    }
                    else
                    {
                       
                        List<string> ownerKeyInfo = new List<string>();
                        ownerKeyInfo.Add(clientProgramme.Owner.Name);
                        ownerKeyInfo.Add(clientProgramme.Id.ToString());
                        ownerKeyInfo.Add(clientProgramme.InformationSheet.ReferenceId.ToString());


                        KeyValuePair<Guid, List<string>> pair = new KeyValuePair<Guid, List<string>>(clientProgramme.Owner.Id, ownerKeyInfo);

                        if (owners.Contains(pair))
                        {
                            break;
                        }
                        else
                        {
                            owners.Add(pair);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("num of organisation processed.===="+count);

            }
            foreach (var owner in owners)
            {
                UniqueOwners.Add(new SelectListItem()
                {
                    Text = owner.Value.ElementAt(0) + " Reference: "+owner.Value.ElementAt(2),
                    Value = owner.Key.ToString() + " " + owner.Value.ElementAt(1)
                });
            }
            UniqueOwners = UniqueOwners.OrderBy(a => a.Text).ToList();

        }
        public string TargetOwner { get; set; } 
        public string TargetOwnerFAP { get; set; }
        public string NewFAP { get; set; }
        public string AdvisorToMove { get; set; }
        public string ExtraFAP { get; set; }
        public string SourceClientProgrammeId { get; set; }
        public string SourceClientProgrammeName{ get; set; }
    }
}