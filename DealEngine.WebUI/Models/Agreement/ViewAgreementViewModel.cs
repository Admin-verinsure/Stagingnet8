using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace DealEngine.WebUI.Models.Agreement
{
    public class ViewAgreementViewModel : BaseViewModel
    {
        public ViewAgreementViewModel()
        {
            GetPaymentMethodOptions();
            GetPaymentFrequencyOptions();
        }

        public ViewAgreementViewModel(ClientAgreement agreement, ClientInformationSheet sheet, System.Globalization.CultureInfo userCulture)
        {
            GetPaymentMethodOptions();
            GetPaymentFrequencyOptions();
            if (agreement != null)
            {
                GetInsuranceInclusionsExclusions(agreement, userCulture);
                GetMultiCoverOptions(agreement, userCulture);
                GetExtensionCoverOptions(agreement, userCulture);
                GetRiskPremiums(agreement, userCulture);
            }
            if (sheet != null)
            {
                ClientInformationSheet = sheet;
                GetVehicles(sheet);
                GetBoats(agreement, sheet);
            }
        }

        private void  GetPaymentMethodOptions()
        {
            PaymentMethodOptions = new List<SelectListItem>();
            PaymentMethodOptions.Add(
                new SelectListItem()
                {
                    Text = "-- Select --",
                    Value = "Invalid"
                });
            PaymentMethodOptions.Add(
                new SelectListItem()
                {
                    Text = "Credit Card",
                    Value = "Credit Card"
                });
            PaymentMethodOptions.Add(
                new SelectListItem()
                {
                    Text = "Invoice",
                    Value = "Invoice"
                });
            PaymentMethodOptions.Add(
                new SelectListItem()
                {
                    Text = "Hunter Premium Funding",
                    Value = "Hunter Premium Funding"
                });
        }
        private void GetPaymentFrequencyOptions()
        {
            PaymentFrequencyOptions = new List<SelectListItem>();
            PaymentFrequencyOptions.Add(
                new SelectListItem()
                {
                    Text = "-- Select --",
                    Value = "Invalid"
                });
            PaymentFrequencyOptions.Add(
                new SelectListItem()
                {
                    Text = "Monthly",
                    Value = "Monthly"
                });
            PaymentFrequencyOptions.Add(
                new SelectListItem()
                {
                    Text = "Annually",
                    Value = "Annually"
                });
        }

        private void GetBoats(ClientAgreement agreement, ClientInformationSheet sheet)
        {
            Boats = new List<BoatViewModel>();
            HasBoats = false;
            foreach (Boat b in sheet.Boats)
            {
                if (!b.Removed && b.DateDeleted == null && b.BoatCeaseDate == DateTime.MinValue)
                {
                    Boats.Add(new BoatViewModel { BoatName = b.BoatName, BoatMake = b.BoatMake, BoatModel = b.BoatModel, MaxSumInsured = b.MaxSumInsured, BoatQuoteExcessOption = b.BoatQuoteExcessOption });
                    HasBoats = true;
                }
            }

            if (HasBoats)
            {
                BVTerms = new List<EditTermsViewModel>();
                foreach (ClientAgreementBVTerm bt in
                    agreement.ClientAgreementTerms.FirstOrDefault(t => t.SubTermType == "BV" && t.DateDeleted == null).BoatTerms)
                {
                    if (bt.DateDeleted == null)
                    {
                        BVTerms.Add(new EditTermsViewModel
                        {
                            BoatName = bt.BoatName,
                            BoatMake = bt.BoatMake,
                            BoatModel = bt.BoatModel,
                            TermLimit = bt.TermLimit,
                            Excess = bt.Excess
                        });
                    }
                }
            }
        }
        private async Task GetVehicles(ClientInformationSheet sheet)
        {
            Vehicles = new List<VehicleViewModel>();
            HasVehicles = false;
            foreach (Vehicle v in sheet.Vehicles)
            {
                if (!v.Removed && v.DateDeleted == null && v.VehicleCeaseDate == DateTime.MinValue)
                {
                    Vehicles.Add(new VehicleViewModel { VehicleCategory = v.VehicleCategory, Make = v.Make, Year = v.Year, Registration = v.Registration, FleetNumber = v.FleetNumber, VehicleModel = v.Model, SumInsured = v.GroupSumInsured });
                    HasVehicles = true;
                }
            }
        }
        private async Task GetRiskPremiums(ClientAgreement agreement, System.Globalization.CultureInfo userCulture)
        {
            RiskPremiums = new List<RiskPremiumsViewModel>();
            var sheet = agreement.ClientInformationSheet;
            foreach (ClientAgreementTerm term in agreement.ClientAgreementTerms.Where(t => t.DateDeleted == null))
            {
                if (sheet.IsChange && sheet.PreviousInformationSheet != null)
                {
                    RiskPremiums.Add(new RiskPremiumsViewModel
                    {
                        RiskName = agreement.Product.Name,
                        Premium = (term.PremiumDiffer - term.FSLDiffer).ToString("C", userCulture),
                        FSL = term.FSLDiffer.ToString("C", userCulture),
                        TotalPremium = term.PremiumDiffer.ToString("C", userCulture),
                        TotalPremiumIncFeeGST = ((term.PremiumDiffer + agreement.BrokerFee) * agreement.ClientInformationSheet.Programme.BaseProgramme.TaxRate).ToString("C", userCulture),
                        TotalPremiumIncFeeIncGST = ((term.PremiumDiffer + agreement.BrokerFee) * (1 + agreement.ClientInformationSheet.Programme.BaseProgramme.TaxRate)).ToString("C", userCulture)
                    });
                }
                else
                {
                    RiskPremiums.Add(new RiskPremiumsViewModel
                    {
                        RiskName = agreement.Product.Name,
                        Premium = (term.Premium - term.FSL).ToString("C", userCulture),
                        FSL = term.FSL.ToString("C", userCulture),
                        TotalPremium = term.Premium.ToString("C", userCulture),
                        TotalPremiumIncFeeGST = ((term.Premium + agreement.BrokerFee) * agreement.ClientInformationSheet.Programme.BaseProgramme.TaxRate).ToString("C", userCulture),
                        TotalPremiumIncFeeIncGST = ((term.Premium + agreement.BrokerFee) * (1 + agreement.ClientInformationSheet.Programme.BaseProgramme.TaxRate)).ToString("C", userCulture)
                    });
                }
            }
        }

        private async Task GetExtensionCoverOptions(ClientAgreement agreement, System.Globalization.CultureInfo userCulture)
        {
            ExtensionCoverOptions = new List<ExtensionCoverOptions>();
            int intMonthlyInstalmentNumber = 1;
           
            foreach (ClientAgreementTermExtension term in agreement.ClientAgreementTermExtensions.Where(t => t.DateDeleted == null).OrderBy(acat => acat.TermLimit).ThenBy(acat => acat.Excess))
            {
                
                    ExtensionCoverOptions.Add(new ExtensionCoverOptions
                    {
                        TermId = term.Id,
                        //isSelected = (term.Bound == true) ? "checked" : "",
                        ProductId = agreement.Product.Id,
                        RiskName = agreement.Product.Name,
                        Inclusion = "Limit: " + term.TermLimit.ToString("C", userCulture),
                        Exclusion = "Minimum Excess: " + term.Excess.ToString("C", userCulture),
                        TotalPremium = term.Premium.ToString("C", userCulture),
                        ExtensionName = term.ExtentionName,
                    });
               
               

            }
        }
        private async Task GetMultiCoverOptions(ClientAgreement agreement, System.Globalization.CultureInfo userCulture)
        {
            MultiCoverOptions = new List<MultiCoverOptions>();
            int intMonthlyInstalmentNumber = 1;
            if (agreement.ClientInformationSheet.Programme.BaseProgramme.EnableMonthlyPremiumDisplay)
            {
                intMonthlyInstalmentNumber = agreement.ClientInformationSheet.Programme.BaseProgramme.MonthlyInstalmentNumber;
            }
            foreach (ClientAgreementTerm term in agreement.ClientAgreementTerms.Where(t => t.DateDeleted == null).OrderBy(acat => acat.TermLimit).ThenBy(acat => acat.Excess))
            {
                if ((agreement.Product.Id == new Guid("094f0b97-f288-440d-a32a-3c2128e35e70") || agreement.Product.Id == new Guid("eda1fa59-19e3-48f6-aef9-3057582717b4")) 
                    && term.TermLimit == 0 && term.Excess == 0 && term.Premium == 0) //Apollo PIFAP or Abbott PIFAP
                {
                    MultiCoverOptions.Add(new MultiCoverOptions
                    {
                        TermId = term.Id,
                        isSelected = (term.Bound == true) ? "checked" : "",
                        ProductId = agreement.Product.Id,
                        RiskName = agreement.Product.Name,
                        Inclusion = "Same as Professional Indemnity",
                        Exclusion = "Same as Professional Indemnity",
                        TotalPremium = "Included",
                        monthlypremium = "Included",
                        Dependableproduct = "NonDependable"
                    });
                }
                else
                {
                    if (null != agreement.Product.DependableProduct)
                    {
                        if (agreement.Product.Id == new Guid("0e9ce29b-f1e4-499a-8994-a96e96962953")) //NZFSG
                        {
                            if (agreement.ClientInformationSheet.Programme.BaseProgramme.ProgHidePremium)
                            {
                                MultiCoverOptions.Add(new MultiCoverOptions
                                {
                                    TermId = term.Id,
                                    isSelected = (term.Bound == true) ? "checked" : "",
                                    ProductId = agreement.Product.Id,
                                    RiskName = agreement.Product.Name,
                                    Inclusion = "Limit: " + term.TermLimit.ToString("C", userCulture),
                                    Exclusion = "Minimum Excess: " + term.Excess.ToString("C", userCulture),
                                    TotalPremium = "To be advised",
                                    monthlypremium = "To be advised",
                                    Dependableproduct = agreement.Product.DependableProduct.Name
                                });
                            }
                            else
                            {
                                MultiCoverOptions.Add(new MultiCoverOptions
                                {
                                    TermId = term.Id,
                                    isSelected = (term.Bound == true) ? "checked" : "",
                                    ProductId = agreement.Product.Id,
                                    RiskName = agreement.Product.Name,
                                    Inclusion = "Limit: " + term.TermLimit.ToString("C", userCulture),
                                    Exclusion = "Minimum Excess: " + term.Excess.ToString("C", userCulture),
                                    TotalPremium = term.Premium.ToString("C", userCulture),
                                    monthlypremium = "To be advised", //(term.Premium / agreement.ClientInformationSheet.Programme.BaseProgramme.MonthlyInstalmentNumber).ToString("C", userCulture),
                                    Dependableproduct = agreement.Product.DependableProduct.Name
                                });
                            }

                        }
                        else
                        {
                            if (agreement.ClientInformationSheet.Programme.BaseProgramme.ProgHidePremium)
                            {
                                MultiCoverOptions.Add(new MultiCoverOptions
                                {
                                    TermId = term.Id,
                                    isSelected = (term.Bound == true) ? "checked" : "",
                                    ProductId = agreement.Product.Id,
                                    RiskName = agreement.Product.Name,
                                    Inclusion = "Limit: " + term.TermLimit.ToString("C", userCulture),
                                    Exclusion = "Excess: " + term.Excess.ToString("C", userCulture),
                                    TotalPremium = "To be advised",
                                    monthlypremium = "To be advised",
                                    Dependableproduct = agreement.Product.DependableProduct.Name
                                });
                            }
                            else
                            {
                                MultiCoverOptions.Add(new MultiCoverOptions
                                {
                                    TermId = term.Id,
                                    isSelected = (term.Bound == true) ? "checked" : "",
                                    ProductId = agreement.Product.Id,
                                    RiskName = agreement.Product.Name,
                                    Inclusion = "Limit: " + term.TermLimit.ToString("C", userCulture),
                                    Exclusion = "Excess: " + term.Excess.ToString("C", userCulture),
                                    TotalPremium = term.Premium.ToString("C", userCulture),
                                    monthlypremium = "To be advised", //(term.Premium / intMonthlyInstalmentNumber).ToString("C", userCulture),
                                    Dependableproduct = agreement.Product.DependableProduct.Name
                                });
                            }

                        }

                    }
                    else
                    {
                        if (agreement.Product.Id == new Guid("0e9ce29b-f1e4-499a-8994-a96e96962953")) //NZFSG
                        {
                            if (agreement.ClientInformationSheet.Programme.BaseProgramme.ProgHidePremium)
                            {
                                MultiCoverOptions.Add(new MultiCoverOptions
                                {
                                    TermId = term.Id,
                                    isSelected = (term.Bound == true) ? "checked" : "",
                                    ProductId = agreement.Product.Id,
                                    RiskName = agreement.Product.Name,
                                    Inclusion = "Limit: " + term.TermLimit.ToString("C", userCulture),
                                    Exclusion = "Minimum Excess: " + term.Excess.ToString("C", userCulture),
                                    TotalPremium = "To be advised",
                                    monthlypremium = "To be advised",
                                    Dependableproduct = "NonDependable"
                                });
                            }
                            else
                            {
                                if (agreement.ClientInformationSheet.IsChange) 
                                {
                                    MultiCoverOptions.Add(new MultiCoverOptions
                                    {
                                        TermId = term.Id,
                                        isSelected = (term.Bound == true) ? "checked" : "",
                                        ProductId = agreement.Product.Id,
                                        RiskName = agreement.Product.Name,
                                        Inclusion = "Limit: " + term.TermLimit.ToString("C", userCulture),
                                        Exclusion = "Minimum Excess: " + term.Excess.ToString("C", userCulture),
                                        TotalPremium = term.PremiumDiffer.ToString("C", userCulture),
										monthlypremium = "To be advised", //(term.PremiumDiffer / intMonthlyInstalmentNumber).ToString("C", userCulture),
                                        Dependableproduct = "NonDependable"
                                    });
                                } else
                                {
                                    MultiCoverOptions.Add(new MultiCoverOptions
                                    {
                                        TermId = term.Id,
                                        isSelected = (term.Bound == true) ? "checked" : "",
                                        ProductId = agreement.Product.Id,
                                        RiskName = agreement.Product.Name,
                                        Inclusion = "Limit: " + term.TermLimit.ToString("C", userCulture),
                                        Exclusion = "Minimum Excess: " + term.Excess.ToString("C", userCulture),
                                        TotalPremium = term.Premium.ToString("C", userCulture),
										monthlypremium = "To be advised", //(term.Premium / intMonthlyInstalmentNumber).ToString("C", userCulture),
                                        Dependableproduct = "NonDependable"
                                    });
                                }

                            }

                        }
                        else
                        {
                            if (agreement.ClientInformationSheet.Programme.BaseProgramme.ProgHidePremium)
                            {
                                MultiCoverOptions.Add(new MultiCoverOptions
                                {
                                    TermId = term.Id,
                                    isSelected = (term.Bound == true) ? "checked" : "",
                                    ProductId = agreement.Product.Id,
                                    RiskName = agreement.Product.Name,
                                    Inclusion = "Limit: " + term.TermLimit.ToString("C", userCulture),
                                    Exclusion = "Excess: " + term.Excess.ToString("C", userCulture),
                                    TotalPremium = "To be advised",
                                    monthlypremium = "To be advised",
                                    Dependableproduct = "NonDependable"
                                });
                            }
                            else
                            {

                                if (agreement.ClientInformationSheet.IsChange)
                                {
                                    MultiCoverOptions.Add(new MultiCoverOptions
                                    {
                                        TermId = term.Id,
                                        isSelected = (term.Bound == true) ? "checked" : "",
                                        ProductId = agreement.Product.Id,
                                        RiskName = agreement.Product.Name,
                                        Inclusion = "Limit: " + term.TermLimit.ToString("C", userCulture),
                                        Exclusion = "Excess: " + term.Excess.ToString("C", userCulture),
                                        TotalPremium = term.PremiumDiffer.ToString("C", userCulture),
                                        monthlypremium = "To be advised", //(term.PremiumDiffer / intMonthlyInstalmentNumber).ToString("C", userCulture),
                                        Dependableproduct = "NonDependable"
                                    });
                                }
                                else
                                {
                                    MultiCoverOptions.Add(new MultiCoverOptions
                                    {
                                        TermId = term.Id,
                                        isSelected = (term.Bound == true) ? "checked" : "",
                                        ProductId = agreement.Product.Id,
                                        RiskName = agreement.Product.Name,
                                        Inclusion = "Limit: " + term.TermLimit.ToString("C", userCulture),
                                        Exclusion = "Excess: " + term.Excess.ToString("C", userCulture),
                                        TotalPremium = term.Premium.ToString("C", userCulture),
                                        monthlypremium = "To be advised", //(term.Premium / intMonthlyInstalmentNumber).ToString("C", userCulture),
                                        Dependableproduct = "NonDependable"
                                    });
                                }
                               
                            }

                        }
                    }
                }

            }
        }
        //limit
        private async Task GetInsuranceInclusionsExclusions(ClientAgreement agreement, System.Globalization.CultureInfo userCulture)
        {
            Inclusions = new List<InsuranceInclusion>();
            Exclusions = new List<InsuranceExclusion>();
            if (agreement.Product.IsMultipleOption)
            {
                if (agreement.Product.Id == new Guid("79dc8bcd-01f2-4551-9caa-aa9200f1d659")) //NZACS DO
                {
                    Inclusions.Add(new InsuranceInclusion { RiskName = agreement.Product.Name, Inclusion = "Limit: Options displayed below / Extension Covers" });
                }
                else
                {
                    Inclusions.Add(new InsuranceInclusion { RiskName = agreement.Product.Name, Inclusion = "Limit: Options displayed below" });
                }
                Exclusions.Add(new InsuranceExclusion { RiskName = agreement.Product.Name, Exclusion = "Excess: Options displayed below" });

            }
            else
            {
                foreach (ClientAgreementTerm term in agreement.ClientAgreementTerms)
                {
                    var riskname = "";
                    if (term.SubTermType == "MV")
                    {
                        riskname = "Motor Vehicle";
                    }
                    else if (term.SubTermType == "BV")
                    {
                        riskname = "Vessel";
                    }
                    else
                    {
                        riskname = agreement.Product.Name;
                    }
                    Inclusions.Add(new InsuranceInclusion { RiskName = riskname, Inclusion = term.TermLimit.ToString("C0", userCulture) });
                }

                if (agreement.Product.Id == new Guid("107c38d6-0d46-4ec1-b3bd-a73b0021f2e3")) //HIANZ
                {
                    foreach (ClientAgreementTerm term in agreement.ClientAgreementTerms)
                    {
                        Exclusions.Add(new InsuranceExclusion
                        {
                            RiskName = "Motor Vehicle",
                            Exclusion = "Excess: <br /> - 1% of Sum Insured subject to a minimum of $500 " +
                                                            "<br /> - theft excess 1 % of the sum insured with a minimum of $1,000 including whilst on hire, non return from hire and from the clients yard " +
                                                            "<br /> - theft excess nil for any vehicle or item insured fitted with a GPS tracking device " +
                                                            "<br /> PLUS " +
                                                            "<br /> - Whilst being driven by any person under 25 years of age $500 " +
                                                            "<br /> - Breach of Warranty / Invalidation Clause $1, 000"
                        });
                    }
                }
                else if (agreement.Product.Id == new Guid("bc62172c-1e15-4e5a-8547-a7bd002121eb"))
                { //Arcco
                    foreach (ClientAgreementTerm term in agreement.ClientAgreementTerms)
                    {
                        Exclusions.Add(new InsuranceExclusion
                        {
                            RiskName = "Motor Vehicle",
                            Exclusion = "Excess: <br /> $2,000 each and every claim. " +
                                                            "<br /> An additional $1,000 excess applies when the vehicle is on hire and is being driven by an under 21 year old driver or has held a full licence less than 12 months. " +
                                                            "<br /> $500 excess on trailers. "
                        });
                    }
                }
                else if (agreement.Product.Id == new Guid("e2eae6d8-d68e-4a40-b50a-f200f393777a"))
                { //CoastGuard
                    foreach (ClientAgreementTerm term in agreement.ClientAgreementTerms)
                    {
                        Exclusions.Add(new InsuranceExclusion
                        {
                            RiskName = "Vessel",
                            Exclusion = "Excess: refer to vessel excess "
                        });
                    }
                }
                else
                {
                    foreach (ClientAgreementTerm term in agreement.ClientAgreementTerms)
                    {
                        Exclusions.Add(new InsuranceExclusion
                        {
                            RiskName = agreement.Product.Name,
                            Exclusion = "Excess: " + term.Excess.ToString("C", userCulture)
                        });
                    }
                }
            }

        }

        public IEnumerable<InsuranceRoleViewModel> InsuranceRoles { get; set; }
        public string ProductName { get; set; }
        public string ProgrammeName { get; set; }
        public string ProgrammeNamedPartyName { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? IssuedToCustomer { get; set; }
        public DateTime? AcceptedDate { get; set; }
        public Boolean NextInfoSheet { get; set; }
        public Boolean IsChange { get; set; }
        public string StartDate { get; set; }
        public string Content { get; set; }
        public string Sheetstatus { get; set; }
        public string EndDate { get; set; }
        public string CurrencySymbol { get; set; }
        public string AdministrationFee { get; set; }
        public string PlacementFee { get; set; }
        public string AdditionalCertFee { get; set; }
        public IList<InsuranceInclusion> Inclusions { get; set; }
        public IList<InsuranceExclusion> Exclusions { get; set; }
        public IList<MultiCoverOptions> MultiCoverOptions { get; set; }
        public IList<RiskPremiumsViewModel> RiskPremiums { get; set; }
        public Guid InformationSheetId { get; set; }
        public bool HasVehicles { get; set; }
        public IList<VehicleViewModel> Vehicles { get; set; }
        public Guid ClientAgreementId { get; set; }
        public string ClientNumber { get; set; }
        public string PolicyNumber { get; set; }
        public string BrokerageRate { get; set; }
        public IList<AgreementDocumentViewModel> Documents { get; set; }
        public bool EditEnabled { get; set; }
        public bool IsMultipleOption { get; set; }
        public bool IsOptionalProduct { get; set; }
        public Guid ClientProgrammeId { get; set; }
        public Guid ProgrammeId { get; set; }
        public ClientInformationSheet ClientInformationSheet { get; set; }
        public bool HasBoats { get; set; }
        public IList<BoatViewModel> Boats { get; set; }
        public List<EditTermsViewModel> BVTerms { get; internal set; }
        public List<EditTermsViewModel> MVTerms { get; internal set; }
        //public List<EditTermsViewModel> PLTerms { get; internal set; }
        //public List<EditTermsViewModel> EDTerms { get; internal set; }
        //public List<EditTermsViewModel> PITerms { get; internal set; }
        //public List<EditTermsViewModel> ELTerms { get; internal set; }
        //public List<EditTermsViewModel> CLTerms { get; internal set; }
        //public List<EditTermsViewModel> SLTerms { get; internal set; }
        //public List<EditTermsViewModel> DOTerms { get; internal set; }
        public List<EditTermsViewModel> SubtypeTerms { get; internal set; }
        
        public List<EditTermsCancelViewModel> BVTermsCan { get; internal set; }
        public List<EditTermsCancelViewModel> MVTermsCan { get; internal set; }
        public List<ClientAgreementReferral> Referrals { get; set; }
        public IList<SelectListItem> UserList { get; set; }
        public User CurrentUser { get; set; }
        public User SelectedBroker { get; set; }
        public DateTime CancellEffectiveDate { get; set; }
        public string InformationSheetStatus { get; set; }
        public decimal ReferralAmount { get; set; }
        public decimal ReferralLoading { get; set; }
        public string AuthorisationNotes { get; set; }
        public bool EGlobalIsActive { get; set; }
        public string CancellNotes { get; set; }
        public string DeclineNotes { get; set; }
        public string Advisory { get; internal set; }
        public string Declaration { get; set; }
        public bool ProgrammeStopAgreement { get; set; }
        public string AgreementMessage { get; set; }
        public bool RequirePayment { get; set; }
        public string NoPaymentRequiredMessage { get; set; }
        public string CancelAgreementReason { get; set; }
        public bool SentOnlineAcceptance { get; set; }
        public string RetroactiveDate { get; set; }
        public string TerritoryLimit { get; set; }
        public string Jurisdiction { get; set; }
        public string ProfessionalBusiness { get; set; }
        public string issuetobrokercomment { get; set; }
        public DateTime? IssuedToBroker { get; set; }
        public string issuetobrokerby { get; set; }
        public string issuetobrokerto { get; set; }
        public string InsuredName { get; set; }
        public string BindNotes { get; set; }
        public string AdjustmentAmount { get; set; }
        public decimal SelectedPremium { get; set; }
        public decimal BasePremium { get; set; }
        public IList<SelectListItem> PaymentMethodOptions { get; set; }
        public IList<SelectListItem> PaymentFrequencyOptions { get; set; }
        public string ProductCode { get; set; }
        public IList<ClientAgreementTermExtension> AgreementExtensions { get; set; }
        public IList<ExtensionCoverOptions> ExtensionCoverOptions { get; set; }
        public bool IsExtentionCoverOption { get; set; }
        public bool ExtentionCoverName { get; set; }
    }

    public class InsuranceInclusion
    {
        public string RiskName { get; set; }

        public string Inclusion { get; set; }
   
    }

    public class MultiCoverOptions
    {
        public Guid ProductId { get; set; }
        public Guid TermId { get; set; }
        public string RiskName { get; set; }
        public string isSelected { get; set; }
        public string Inclusion { get; set; }
        public string Exclusion { get; set; }
        public string limit { get; set; }
        public string excess { get; set; }
        public string premium { get; set; }
        public string TotalPremium { get; set; }
        public string monthlypremium { get; set; }
        public string Dependableproduct { get; set; }
    }

    public class ExtensionCoverOptions
    {
        public Guid ProductId { get; set; }
        public Guid TermId { get; set; }
        public string RiskName { get; set; }
        public string isSelected { get; set; }
        public string Inclusion { get; set; }
        public string Exclusion { get; set; }
        public string limit { get; set; }
        public string excess { get; set; }
        public string premium { get; set; }
        public string TotalPremium { get; set; }
        public string ExtensionName { get; set; }

    }

    public class InsuranceExclusion
    {
        public string RiskName { get; set; }

        public string Exclusion { get; set; }
    }

    public class InsuranceRoleViewModel
    {
        public string RoleName { get; set; }

        public string Name { get; set; }

        public string ManagedBy { get; set; }

        public string Email { get; set; }
    }

    public class RiskPremiumsViewModel
    {
        public string RiskName { get; set; }

        public string Premium { get; set; }

		public string FSL { get; set; }
           
        public string TotalPremium { get; set; }

        public string TotalPremiumIncFeeGST { get; set; }

        public string TotalPremiumIncFeeIncGST { get; set; }

    }

}