using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using DealEngine.Domain.Entities;
using DealEngine.WebUI.Models.ProductModels;
using System.Linq;

namespace DealEngine.WebUI.Models.Programme
{


	public class ProgrammeInfoViewModel : BaseViewModel
	{
        public ProgrammeInfoViewModel() 
        {
            Brokers = new List<SelectListItem>();
            Flags = GetSelectListOptions();
        }
        public ProgrammeInfoViewModel(List<User> brokers, Domain.Entities.Programme programme, ClientProgramme clientProgramme)
        {
            if(brokers != null)
            {
                Brokers = GetBrokerSelectList(brokers);
            }            
            if(programme != null)
            {
                Id = programme.Id;
                Name = programme.Name;
                Programme = programme;
                if (Brokers != null)
                {
                    Brokers.FirstOrDefault(i => i.Value == programme.BrokerContactUser.Id.ToString()).Selected = true;
                }                
            }
            if(clientProgramme != null)
            {
                ClientProgramme = clientProgramme;
            }
            BooleanOptions = GetBooleanOptions();
        }
        public IList<SelectListItem> BooleanOptions { get; set; }
        private IList<SelectListItem> GetBrokerSelectList(List<User> brokers)
        {
            Brokers = new List<SelectListItem>();
            foreach (var broker in brokers)
            {
                Brokers.Add(
                    new SelectListItem
                    {
                        Text = broker.FirstName + " " + broker.Email,
                        Value = broker.Id.ToString(),
                        Selected = false
                    });
            }
            return Brokers;
        }
        private IList<SelectListItem> GetSelectListOptions()
        {
            return new List<SelectListItem>()
            {
                new SelectListItem
                {
                    Text = "-- Select --", Value = "0"
                },
                new SelectListItem
                {
                    Text = "IsPdfDoc", Value = "IsPdfDoc"
                },
                 new SelectListItem
                {
                    Text = "ProgEnableEmail", Value = "ProgEnableEmail"
                }, new SelectListItem
                {
                    Text = "ProgHideAdminFee", Value = "ProgHideAdminFee"
                }, new SelectListItem
                {
                    Text = "ProgHidePremium", Value = "ProgHidePremium"
                }, new SelectListItem
                {
                    Text = "EnableFullProposalReport", Value = "EnableFullProposalReport"
                }, new SelectListItem
                {
                    Text = "IsFAPOrg", Value = "IsFAPOrg"
                }, new SelectListItem
                {
                    Text = "EnableEDReport", Value = "EnableEDReport"
                }, new SelectListItem
                {
                    Text = "EnablePIReport", Value = "EnablePIReport"
                }, new SelectListItem
                {
                    Text = "EnableCLReport", Value = "EnableCLReport"
                }, new SelectListItem
                {
                    Text = "EnableCyberReport", Value = "EnableCyberReport"
                }, new SelectListItem
                {
                    Text = "EnableFAPReport", Value = "EnableFAPReport"
                }
            };
        }

        public Guid Id { get; set; }
        public bool EGlobalIsActiveOrNot { get; set; }
        public ClientProgramme ClientProgramme { get; set; }
        public IList<ProductInfoViewModel> Product { get; set; }
        public IList<Domain.Entities.Organisation> Owner { get; set; }
        public IList<EGlobalSubmission> EGlobalSubmissions { get; set; }
        public IList<EGlobalResponse> EGlobalResponses { get; set; }
        public User BrokerContactUser { get; set; }
        public string Name { get; set; }
        public Domain.Entities.Programme Programme { get; set; }
        public string OwnerCompany { get; set; }
        public string DateCreated { get; set; }
        public IList<SelectListItem> Brokers { get; set; }
        public ProductViewModel ProductViewModel { get; set; }
        public InformationBuilderViewModel InformationBuilderViewModel { get; set; }
        public Guid AgreementId { get; set; }
        public Guid ProgId { get; set; }
        public IList<Rule> Rules { get; set; }
        public Guid ProductId { get; set; }
        public List<ProgrammeInfoViewModel> Programmes { get; set; }
        public IList<SelectListItem> Flags { get; set; }



    }
}

