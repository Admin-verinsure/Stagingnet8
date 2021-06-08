
using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DealEngine.WebUI.Models
{
    public class EditClientsViewModel : BaseViewModel
    {
        public Domain.Entities.Organisation Organisation { get; set; }
        public EditClientsViewModel(Domain.Entities.Programme programme)
        {
            if(programme != null)
            {
                GenerateClientsOptions(programme);
                Programme = programme;
            }            
            ClientProgramme = new ClientProgramme(null, null, null);
            Organisation = new Domain.Entities.Organisation();

            GetTiers();
        }

        private void GetTiers()
        {
            Tiers = new List<SelectListItem>();
            Tiers.Add(
                new SelectListItem()
                {
                    Text = "Apollo Standard",
                    Value = "Apollo Standard"
                });
            Tiers.Add(
                new SelectListItem()
                {
                    Text = "Apollo Prime",
                    Value = "Apollo Prime"
                });
            Tiers.Add(
                new SelectListItem()
                {
                    Text = "Mortgage Express",
                    Value = "Mortgage Express"
                });
        }

        private void GenerateClientsOptions(Domain.Entities.Programme programme)
        {
            Owners = new List<SelectListItem>();
            Domain.Entities.Organisation org = null;
            try
            {
                foreach (var owner in programme.ClientProgrammes.Where(c=>c.DateDeleted==null).OrderBy(pclp => pclp.Owner.Name).ToList())
                {
                    var objectType = owner.GetType();
                    if (!objectType.IsSubclassOf(typeof(ClientProgramme)))
                    {
                        if (owner.Owner != null)
                        {
                            Owners.Add(
                                new SelectListItem()
                                {
                                    Text = owner.Owner.Name,
                                    Value = owner.Owner.Id.ToString()
                                });
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public IList<SelectListItem> Tiers { get; set; }
        public IList<SelectListItem> Owners { get; set; }
        public ClientProgramme ClientProgramme { get; set; }
        public Domain.Entities.Programme Programme { get; set; }
    }
}
