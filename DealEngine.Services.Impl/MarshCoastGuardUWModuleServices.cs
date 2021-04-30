using System;
using System.Collections.Generic;
using System.Linq;
using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;

namespace DealEngine.Domain.Services.UnderwritingModules
{
    public class MarshCoastGuardUWModuleServices : IUnderwritingModule
    {
        public string Name { get; protected set; }

        public MarshCoastGuardUWModuleServices()
        {
            Name = "Marsh_CoastGuard";
        }

        public bool Underwrite(User CurrentUser, ClientInformationSheet informationSheet)
        {
            throw new NotImplementedException();
        }

        public bool Underwrite(User underwritingUser, ClientInformationSheet informationSheet, Product product, string reference)
        {
            ClientAgreement agreement = GetClientAgreement(underwritingUser, informationSheet, informationSheet.Programme, product, reference);
            Guid id = agreement.Id;

            ClientAgreementTerm term = GetAgreementTerm(underwritingUser, agreement, "BV");
            var bvTerms = term.BoatTerms;
            var mvTerms = term.MotorTerms;

            if (bvTerms != null)
            {
                foreach (ClientAgreementBVTerm bVTerm in bvTerms)
                {
                    bVTerm.Delete(underwritingUser);
                }
            }

            if (mvTerms != null)
            {
                foreach (ClientAgreementMVTerm mVTerm in mvTerms)
                {
                    mVTerm.Delete(underwritingUser);
                }
            }

            if (agreement.ClientAgreementRules.Count == 0)
                foreach (var rule in product.Rules.Where(r => !string.IsNullOrWhiteSpace(r.Name)))
                    agreement.ClientAgreementRules.Add(new ClientAgreementRule(underwritingUser, rule, agreement));

            if (agreement.ClientAgreementEndorsements.Count == 0)
                foreach (var endorsement in product.Endorsements.Where(e => !string.IsNullOrWhiteSpace(e.Name)))
                    agreement.ClientAgreementEndorsements.Add(new ClientAgreementEndorsement(underwritingUser, endorsement, agreement));

            IDictionary<string, decimal> rates = BuildRulesTable(agreement, "tcunder50kexcess250rate", "tcunder50kexcess500rate", "tcunder50kminpremium",
                "tc50k100kexcess250rate", "tc50k100kexcess500rate", "tc50k100kminpremium", "tc100k200kexcess500rate", "tc100k200kexcess1000rate", "tc100k200kminpremium",
                "tc200k250kexcess500rate", "tc200k250kexcess1000rate", "tc200k250kminpremium", "jbunder100kexcess500rate", "jbunder100kexcess1000rate", "jbunder100kminpremium",
                "jsunder100kexcess500rate", "jsunder100kexcess1000rate", "jsunder100kminpremium",
                "mcunder100kexcess250rate", "mcunder100kexcess500rate", "mcunder100kminpremium", "mc100k200kexcess500rate", "mc100k200kexcess1000rate", "mc100k200kminpremium",
                "mc200k350kexcess1000rate", "mc200k350kexcess2000rate", "mc200k350kminpremium", "mc350k500kexcess1000rate", "mc350k500kexcess2000rate", "mc350k500kminpremium",
                "fslfeefort", "fslratefortc", "loadingforcatandyc", "loadingforycraceusespinnakersuptp50nm", "loadingforycraceusespinnakersuptp200nm", "mvpremiumrate");

            //Create default referral points based on the clientagreementrules
            if (agreement.ClientAgreementReferrals.Count == 0)
            {
                foreach (var clientagreementreferralrule in agreement.ClientAgreementRules.Where(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null))
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, clientagreementreferralrule.Name, clientagreementreferralrule.Description, "", clientagreementreferralrule.Value, clientagreementreferralrule.OrderNumber, clientagreementreferralrule.DoNotCheckForRenew));
            }
            else
            {
                foreach (var clientagreementreferral in agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null))
                    clientagreementreferral.Status = "";
            }

            int agreementperiodindays = 0;
            agreementperiodindays = (agreement.ExpiryDate - agreement.InceptionDate).Days;

            int totalTermLimit = 0;
            decimal totalTermFsl = 0m;
            decimal totalTermPremium = 0m;
            decimal totalTermBrokerage = 0m;
            decimal totalVehicleFsl = 0m;
            int totalVehicleSumInsured = 0;
            decimal totalVehiclePremium = 0m;
            decimal totalVehicleBrokerage = 0m;
            decimal totalBoatFsl = 0m;
            int totalBoatSumInsured = 0;
            decimal totalBoatPremium = 0m;
            decimal totalBoatBrokerage = 0m;

            decimal totalBoatFslPre = 0m;
            decimal totalBoatFslDiffer = 0m;
            decimal totalBoatPremiumPre = 0m;
            decimal totalBoatPremiumDiffer = 0m;
            int totalBoatTermLimitPre = 0;
            int totalBoatTermLimitDiffer = 0;
            decimal totalBoatBrokeragePre = 0m;
            decimal totalBoatBrokerageDiffer = 0m;

            decimal totalVehicleFslPre = 0m;
            decimal totalVehicleFslDiffer = 0m;
            decimal totalVehiclePremiumPre = 0m;
            decimal totalVehiclePremiumDiffer = 0m;
            int totalVehicleTermLimitPre = 0;
            int totalVehicleTermLimitDiffer = 0;
            decimal totalVehicleBrokeragePre = 0m;
            decimal totalVehicleBrokerageDiffer = 0m;

            bool bolNewBoatAdded = false;
            bool bolNewVehicleAdded = false;

            agreement.QuoteDate = DateTime.UtcNow;

            //calculate boat premium and FSL (BV Term)
            foreach (var boat in informationSheet.Boats.Where(v => (!v.Removed || (v.Removed && v.BoatCeaseDate > DateTime.MinValue && v.BoatCeaseReason > 0)) && v.DateDeleted == null))
            {

                //Pre-rate premiums based on the vessel effectiove date and cease date
                DateTime boatinceptiondate;
                DateTime boatexpirydate;
                DateTime boateffectivedate;
                int boatperiodindays = 0;

                if (!informationSheet.IsChange && (boat.BoatEffectiveDate < agreement.InceptionDate || boat.BoatEffectiveDate > agreement.InceptionDate.AddDays(30)))
                {
                    boat.BoatEffectiveDate = agreement.InceptionDate;
                }

                //default for new boat
                boateffectivedate = (boat.BoatEffectiveDate > DateTime.MinValue) ? boat.BoatEffectiveDate : agreement.InceptionDate;
                boatexpirydate = (boat.BoatCeaseDate > DateTime.MinValue) ? boat.BoatCeaseDate : agreement.ExpiryDate;
                boatinceptiondate = boateffectivedate;

                if (boat.OriginalBoat != null) //exsiting boat
                {
                    boatinceptiondate = boat.OriginalBoat.BoatInceptionDate;
                    boatexpirydate = boat.OriginalBoat.BoatExpireDate;
                    boateffectivedate = (boat.BoatEffectiveDate > DateTime.MinValue) ? boat.BoatEffectiveDate : boat.OriginalBoat.BoatInceptionDate;

                    if (boat.OriginalBoat.BoatCeaseDate == DateTime.MinValue && boat.BoatCeaseDate > DateTime.MinValue)
                    {
                        boatexpirydate = boat.BoatCeaseDate;
                    }
                    else if (boat.OriginalBoat.BoatCeaseDate > DateTime.MinValue && boat.BoatCeaseDate == DateTime.MinValue)
                    {
                        boatinceptiondate = (boat.BoatEffectiveDate > DateTime.MinValue) ? boat.BoatEffectiveDate : boat.OriginalBoat.BoatInceptionDate;
                        boatexpirydate = agreement.ExpiryDate;
                    }
                }
                else
                {
                    bolNewBoatAdded = true;
                }

                boat.BoatInceptionDate = boatinceptiondate;
                boat.BoatExpireDate = boatexpirydate;

                boatperiodindays = (boatexpirydate - boateffectivedate).Days;

                decimal boatRate = 0m;
                decimal boatMinPremium = 0m;
                decimal boatFsl = 0m;
                decimal boatFslRate = 0m;
                decimal boatPremium = 0m;
                decimal boatBrokerage = 0m;
                decimal boatBrokerageRate = 0m;
                decimal boatproratedFsl = 0m;
                decimal boatproratedPremium = 0m;
                decimal boatproratedBrokerage = 0m;

                GetFslRateForTraileredBoat(rates, boat, ref boatFslRate);

                GetBoatRateAndMinPremiumFor(rates, boat, out boatRate, out boatMinPremium);

                boatFsl = boat.MaxSumInsured * boatFslRate / 100;
                boatPremium = boat.MaxSumInsured * boatRate / 100;

                //Additional loading for Hull Configuration (Cat) + BoatType1 (YachtsandCatamarans)
                if (boat.HullConfiguration == "Catamaran" && boat.BoatType1 == "YachtsandCatamarans")
                {
                    boatPremium = boatPremium * (1 + rates["loadingforcatandyc"]);
                }

                //Additional loading for BoatType1 (YachtsandCatamarans) + BoatUseRaceUseSpinnakers (Yes) + BoatUseRaceCategory (YachtClubSocialRacingupto50nm, Yachtracingupto200nm)
                if (boat.BoatType1 == "YachtsandCatamarans" && boat.BoatUses.Any(ycbu => ycbu.BoatUseRaceUseSpinnakers == "True" && ycbu.BoatUseCategory == "Race" && ycbu.DateDeleted == null && !ycbu.Removed))
                {
                    if (boat.BoatUses.Any(ycbu => ycbu.BoatUseRaceCategory == "YachtClubSocialRacingupto50nm")) //BoatUseRaceCategory (YachtClubSocialRacingupto50nm)
                    {
                        boatPremium = boatPremium * (1 + rates["loadingforycraceusespinnakersuptp50nm"]);
                    }
                    else if (boat.BoatUses.Any(ycbu => ycbu.BoatUseRaceCategory == "Yachtracingupto200nm")) //BoatUseRaceCategory (Yachtracingupto200nm)
                    {
                        boatPremium = boatPremium * (1 + rates["loadingforycraceusespinnakersuptp200nm"]);
                    }

                }

                if (boatPremium < boatMinPremium)
                    boatPremium = boatMinPremium;
                boatBrokerageRate = agreement.Brokerage;
                boatBrokerage = boatPremium * boatBrokerageRate / 100;
                boatproratedBrokerage = boatBrokerage;

                boatPremium = boatPremium + boatFsl;
                boatproratedPremium = boatPremium;
                boatproratedFsl = boatFsl;

                //Pre-rate premium if the boat effective date is later than policy inception date
                if (boat.BoatEffectiveDate > agreement.InceptionDate)
                {
                    boatproratedPremium = boatPremium * boatperiodindays / agreementperiodindays;
                    boatproratedFsl = boatFsl * boatperiodindays / agreementperiodindays;
                    boatproratedBrokerage = boatBrokerage * boatperiodindays / agreementperiodindays;
                }

                totalBoatFsl += boatproratedFsl;
                totalBoatSumInsured += (boat.BoatCeaseDate > DateTime.MinValue) ? 0 : boat.MaxSumInsured;
                totalBoatPremium += boatproratedPremium;
                totalBoatBrokerage += boatproratedBrokerage;

                ClientAgreementBVTerm bvTerm = null;
                if (term.BoatTerms != null)
                    bvTerm = term.BoatTerms.FirstOrDefault(bvt => bvt.Boat == boat && bvt.DateDeleted != null);
                else
                    term.BoatTerms = new List<ClientAgreementBVTerm>();

                if (bvTerm == null)
                {
                    bvTerm = new ClientAgreementBVTerm(underwritingUser, boat.BoatName, boat.YearOfManufacture, boat.BoatName, boat.BoatModel, boat.MaxSumInsured, boat.BoatQuoteExcessOption, boatproratedPremium, boatproratedFsl,
                                                       boatBrokerageRate, boatproratedBrokerage, term, boat);
                    bvTerm.BoatMake = boat.BoatMake;
                    bvTerm.BoatModel = boat.BoatModel;
                    bvTerm.TermCategory = "active";
                    bvTerm.AnnualPremium = boatPremium;
                    bvTerm.AnnualFSL = boatFsl;
                    bvTerm.AnnualBrokerage = boatBrokerage;
                    term.BoatTerms.Add(bvTerm);
                }
                else
                {
                    bvTerm.BoatName = boat.BoatName;
                    bvTerm.YearOfManufacture = boat.YearOfManufacture;
                    bvTerm.TermLimit = boat.MaxSumInsured;
                    bvTerm.Premium = boatproratedPremium;
                    bvTerm.Excess = boat.BoatQuoteExcessOption;
                    bvTerm.FSL = boatproratedFsl;
                    bvTerm.BoatMake = boat.BoatMake;
                    bvTerm.BoatModel = boat.BoatModel;
                    bvTerm.DateDeleted = null;
                    bvTerm.BrokerageRate = boatBrokerageRate;
                    bvTerm.Brokerage = boatproratedBrokerage;
                    bvTerm.LastModifiedOn = DateTime.UtcNow;
                    bvTerm.LastModifiedBy = underwritingUser;
                    bvTerm.TermCategory = "active";
                    bvTerm.AnnualPremium = boatPremium;
                    bvTerm.AnnualFSL = boatFsl;
                    bvTerm.AnnualBrokerage = boatBrokerage;
                }

                //===========================
                //For Change Agreement
                decimal boatFslPre = 0m;
                decimal boatFslDiffer = 0m;
                decimal boatPremiumPre = 0m;
                decimal boatPremiumDiffer = 0m;
                decimal boatExcessPre = 0m;
                decimal boatExcessDiffer = 0m;
                decimal boatBrokeragePre = 0m;
                decimal boatBrokerageDiffer = 0m;
                int boatTermLimitPre = 0;
                int boatTermLimitDiffer = 0;
                int preboatperiodindays = 0;

                if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null && boat.OriginalBoat != null)
                {
                    ClientAgreementTerm termPre = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault().ClientAgreementTerms.FirstOrDefault(t => t.SubTermType == "BV" && t.DateDeleted == null);
                    if (termPre != null)
                    {
                        ClientAgreementBVTerm bvTermPre = termPre.BoatTerms.FirstOrDefault(bvpret => bvpret.Boat == boat.OriginalBoat && bvpret.DateDeleted == null);

                        if (bvTermPre != null)
                        {
                            if (boat.OriginalBoat.BoatEffectiveDate > DateTime.MinValue)
                            {
                                preboatperiodindays = (boat.OriginalBoat.BoatExpireDate - boat.OriginalBoat.BoatEffectiveDate).Days;
                            }
                            else
                            {
                                preboatperiodindays = (boat.OriginalBoat.BoatExpireDate - boat.OriginalBoat.BoatInceptionDate).Days;
                            }

                            if (boat.OriginalBoat.BoatCeaseDate == DateTime.MinValue && boat.BoatCeaseDate > DateTime.MinValue)
                            {
                                preboatperiodindays = (boat.BoatExpireDate - boat.BoatInceptionDate).Days;
                            }
                            if (boat.OriginalBoat.BoatEffectiveDate > DateTime.MinValue && boat.BoatEffectiveDate > DateTime.MinValue)
                            {
                                if (boat.OriginalBoat.BoatCeaseDate == DateTime.MinValue && boat.BoatCeaseDate > DateTime.MinValue && boat.OriginalBoat.BoatEffectiveDate == boat.BoatEffectiveDate)
                                {
                                    preboatperiodindays = (boat.BoatExpireDate - boat.BoatEffectiveDate).Days;
                                }
                            }

                            boatFslPre = bvTermPre.FSL;
                            boatPremiumPre = bvTermPre.Premium;
                            boatExcessPre = bvTermPre.Excess;
                            boatTermLimitPre = bvTermPre.TermLimit;
                            boatBrokeragePre = bvTermPre.Brokerage;

                            totalBoatBrokeragePre += bvTermPre.Brokerage;
                            totalBoatPremiumPre += bvTermPre.Premium;
                            totalBoatTermLimitPre += bvTermPre.TermLimit;
                            totalBoatFslPre += bvTermPre.FSL;

                            if ((boat.MaxSumInsured == boat.OriginalBoat.MaxSumInsured && boat.BoatEffectiveDate == boat.OriginalBoat.BoatEffectiveDate &&
                                boat.BoatCeaseDate == boat.OriginalBoat.BoatCeaseDate) ||
                                (boat.BoatCeaseDate > DateTime.MinValue && boat.BoatCeaseReason == 3))
                            {
                                boatFslDiffer = 0m;
                                boatPremiumDiffer = 0m;
                                boatExcessDiffer = 0;
                                boatTermLimitDiffer = 0;

                                totalBoatFslDiffer += 0m;
                                totalBoatPremiumDiffer += 0m;
                                totalBoatTermLimitDiffer += 0;

                            }
                            else
                            {
                                if (boat.OriginalBoat.BoatCeaseDate > DateTime.MinValue && boat.BoatCeaseDate == DateTime.MinValue)
                                {
                                    boatFslDiffer = boatproratedFsl;
                                    boatPremiumDiffer = boatproratedPremium;
                                    boatExcessDiffer = boat.BoatQuoteExcessOption;
                                    boatTermLimitDiffer = boat.MaxSumInsured;

                                    totalBoatFslDiffer += boatproratedFsl;
                                    totalBoatPremiumDiffer += boatproratedPremium;
                                    totalBoatTermLimitDiffer += boat.MaxSumInsured;
                                }
                                else if (preboatperiodindays == 0)
                                {
                                    boatFslDiffer = boatproratedFsl - bvTermPre.FSL;
                                    boatPremiumDiffer = boatproratedPremium - bvTermPre.Premium;
                                    boatExcessDiffer = boat.BoatQuoteExcessOption - bvTermPre.Excess;
                                    boatTermLimitDiffer = boat.MaxSumInsured - bvTermPre.TermLimit;

                                    totalBoatFslDiffer += boatproratedFsl - bvTermPre.FSL;
                                    totalBoatPremiumDiffer += boatproratedPremium - bvTermPre.Premium;
                                    totalBoatTermLimitDiffer += (boat.OriginalBoat.BoatCeaseDate > DateTime.MinValue) ? (boat.MaxSumInsured) : (boat.MaxSumInsured - bvTermPre.TermLimit);
                                }
                                else
                                {
                                    boatFslDiffer = boatproratedFsl - (bvTermPre.FSL * boatperiodindays / preboatperiodindays);
                                    boatPremiumDiffer = boatproratedPremium - (bvTermPre.Premium * boatperiodindays / preboatperiodindays);
                                    boatExcessDiffer = boat.BoatQuoteExcessOption - bvTermPre.Excess;
                                    boatTermLimitDiffer = boat.MaxSumInsured - bvTermPre.TermLimit;

                                    totalBoatFslDiffer += boatproratedFsl - (bvTermPre.FSL * boatperiodindays / preboatperiodindays);
                                    totalBoatPremiumDiffer += boatproratedPremium - (bvTermPre.Premium * boatperiodindays / preboatperiodindays);
                                    totalBoatTermLimitDiffer += (boat.OriginalBoat.BoatCeaseDate > DateTime.MinValue) ? (boat.MaxSumInsured) : (boat.MaxSumInsured - bvTermPre.TermLimit);
                                }
                            }


                        }
                    }

                }
                else
                {
                    boatFslDiffer = boatproratedFsl;
                    boatPremiumDiffer = boatproratedPremium;
                    boatExcessDiffer = boat.BoatQuoteExcessOption;
                    boatTermLimitDiffer = boat.MaxSumInsured;

                    totalBoatFslDiffer += boatproratedFsl;
                    totalBoatPremiumDiffer += boatproratedPremium;
                    totalBoatTermLimitDiffer += boat.MaxSumInsured;

                }

                boatBrokerageDiffer = (boatPremiumDiffer - boatFslDiffer) * boatBrokerageRate / 100;
                totalBoatBrokerageDiffer += boatBrokerageDiffer;

                bvTerm.FSLPre = boatFslPre;
                bvTerm.FSLDiffer = boatFslDiffer;
                bvTerm.PremiumPre = boatPremiumPre;
                bvTerm.PremiumDiffer = boatPremiumDiffer;
                bvTerm.ExcessPre = boatExcessPre;
                bvTerm.ExcessDiffer = boatExcessDiffer;
                bvTerm.TermLimitPre = boatTermLimitPre;
                bvTerm.TermLimitDiffer = boatTermLimitDiffer;
                bvTerm.BrokeragePre = boatBrokeragePre;
                bvTerm.BrokerageDiffer = boatBrokerageDiffer;

                if (boat.BoatCeaseDate > DateTime.MinValue && boat.BoatCeaseReason == 4)
                {
                    bvTerm.TermCategory = "transfered";
                }
                else if (boat.BoatCeaseDate > DateTime.MinValue && boat.BoatCeaseReason != 4)
                {
                    bvTerm.TermCategory = "ceased";
                }
                else
                {
                    bvTerm.TermCategory = "active";
                }
                //===========

                totalTermLimit += boat.MaxSumInsured;
                totalTermFsl += boatproratedFsl;
                totalTermPremium += boatproratedPremium;
                totalTermBrokerage += boatproratedBrokerage;

                //Referral points per vessel
                //Trailer Craft Sum Insured over $250k
                uwrftrailercraftsuminsuredover250k(underwritingUser, boat, agreement);
                //Trailer Craft (jetboats) Sum Insured over $100k
                uwrfjetboatsuminsuredover100k(underwritingUser, boat, agreement);
                //Trailer Craft (jetskis) Sum Insured over $100k
                uwrfjetskisuminsuredover100k(underwritingUser, boat, agreement);
                //Moored Craft Sum Insured over $500k
                uwrfmooredcraftsuminsuredover500k(underwritingUser, boat, agreement);
                //Year of vessel built pre 1985
                uwrfyearbuiltpre1985(underwritingUser, boat, agreement);
                //Not built professionally
                uwrfnotbuiltprofessionally(underwritingUser, boat, agreement);
                //Vessel type (boattype1) other
                uwrfboattype1other(underwritingUser, boat, agreement);
                //Hull construction carbon or other
                uwrfhullconstructioncarbonorother(underwritingUser, boat, agreement);
                //Hull configuration trimaran or other
                uwrfhullconfigurationtrimaranorother(underwritingUser, boat, agreement);
                //Motor type inboardpetrol or jet
                uwrfmotortypeinboardpetrolorjet(underwritingUser, boat, agreement);
                //Motor modified
                uwrfmotormodified(underwritingUser, boat, agreement);
                //Max speed over 60 knots
                uwrfmaxspeedover60knots(underwritingUser, boat, agreement);
                //Vessel use live on board
                uwrfboatuseliveonboard(underwritingUser, boat, agreement);
                //Vessel use race (Oceangoingracingover200nm, Category1Racing)
                uwrfboatuseraceotheroption(underwritingUser, boat, agreement);
                //Swing Moored Type
                uwrfswingmooredtype(underwritingUser, boat, agreement);
                //Other Marina Referred to TC
                uwrfothermarinatc(underwritingUser, boat, agreement);
                //Other Marina
                uwrfothermarina(underwritingUser, boat, agreement);

            }


            //calculate trailer premium and FSL (MV Term)
            foreach (var vehicle in informationSheet.Vehicles.Where(v => (!v.Removed || (v.Removed && v.VehicleCeaseDate > DateTime.MinValue && v.VehicleCeaseReason > 0)) && v.DateDeleted == null))
            {

                //Pre-rate premiums based on the vehicle effectiove date and cease date
                DateTime vehicleinceptiondate;
                DateTime vehicleexpirydate;
                DateTime vehicleeffectivedate;
                int vehicleperiodindays = 0;

                if (vehicle.VehicleEffectiveDate < agreement.InceptionDate || vehicle.VehicleEffectiveDate > agreement.InceptionDate.AddDays(30))
                {
                    vehicle.VehicleEffectiveDate = agreement.InceptionDate;
                }

                //default for new boat
                vehicleeffectivedate = (vehicle.VehicleEffectiveDate > DateTime.MinValue) ? vehicle.VehicleEffectiveDate : agreement.InceptionDate;
                vehicleexpirydate = (vehicle.VehicleCeaseDate > DateTime.MinValue) ? vehicle.VehicleCeaseDate : agreement.ExpiryDate;
                vehicleinceptiondate = vehicleeffectivedate;

                if (vehicle.OriginalVehicle != null) //exsiting vehicle
                {
                    vehicleinceptiondate = vehicle.OriginalVehicle.VehicleInceptionDate;
                    vehicleexpirydate = vehicle.OriginalVehicle.VehicleExpireDate;
                    vehicleeffectivedate = (vehicle.VehicleEffectiveDate > DateTime.MinValue) ? vehicle.VehicleEffectiveDate : vehicle.OriginalVehicle.VehicleInceptionDate;

                    if (vehicle.OriginalVehicle.VehicleCeaseDate == DateTime.MinValue && vehicle.VehicleCeaseDate > DateTime.MinValue)
                    {
                        vehicleexpirydate = vehicle.VehicleCeaseDate;
                    }
                    else if (vehicle.OriginalVehicle.VehicleCeaseDate > DateTime.MinValue && vehicle.VehicleCeaseDate == DateTime.MinValue)
                    {
                        vehicleinceptiondate = (vehicle.VehicleEffectiveDate > DateTime.MinValue) ? vehicle.VehicleEffectiveDate : vehicle.OriginalVehicle.VehicleInceptionDate;
                        vehicleexpirydate = agreement.ExpiryDate;
                    }
                }
                else
                {
                    bolNewVehicleAdded = true;
                }

                vehicle.VehicleInceptionDate = vehicleinceptiondate;
                vehicle.VehicleExpireDate = vehicleexpirydate;

                vehicleperiodindays = (vehicleexpirydate - vehicleeffectivedate).Days;

                string vehicleCategory = null;
                decimal vehicleFsl = 0m;
                decimal vehiclePremium = 0m;
                decimal vehicleBurnerPremium = 0m;
                decimal vehicleExcess = 0m;
                decimal vehicleBrokerage = 0m;
                decimal vehicleBrokerageRate = 0m;
                decimal vehicleproratedFsl = 0m;
                decimal vehicleproratedPremium = 0m;
                decimal vehicleproratedBrokerage = 0m;

                GetFslFeeForTrailer(rates, vehicle, ref vehicleFsl);

                vehiclePremium = (vehicle.GroupSumInsured * rates["mvpremiumrate"] / 100) + vehicleFsl;
                vehicleBrokerageRate = agreement.Brokerage;
                vehicleBrokerage = (vehicle.GroupSumInsured * rates["mvpremiumrate"] / 100) * vehicleBrokerageRate / 100;
                vehicleproratedPremium = vehiclePremium;
                vehicleproratedFsl = vehicleFsl;
                vehicleproratedBrokerage = vehicleBrokerage;
                //Pre-rate premium if the vehicle effective date is later than policy inception date
                if (vehicle.VehicleEffectiveDate > agreement.InceptionDate)
                {
                    vehicleproratedPremium = vehiclePremium * vehicleperiodindays / agreementperiodindays;
                    vehicleproratedFsl = vehicleFsl * vehicleperiodindays / agreementperiodindays;
                    vehicleproratedBrokerage = vehicleBrokerage * vehicleperiodindays / agreementperiodindays;
                }

                totalVehicleFsl += vehicleproratedFsl;
                totalVehicleSumInsured += (vehicle.VehicleCeaseDate > DateTime.MinValue) ? 0 : vehicle.GroupSumInsured;
                totalVehiclePremium += vehicleproratedPremium;
                totalVehicleBrokerage += vehicleproratedBrokerage;

                ClientAgreementMVTerm mvTerm = null;
                if (term.MotorTerms != null)
                    mvTerm = term.MotorTerms.FirstOrDefault(mvt => mvt.Vehicle == vehicle && mvt.DateDeleted != null);
                else
                    term.MotorTerms = new List<ClientAgreementMVTerm>();

                if (mvTerm == null)
                {
                    mvTerm = new ClientAgreementMVTerm(underwritingUser, vehicle.Registration, vehicle.Year, vehicle.Make, vehicle.Model, vehicle.GroupSumInsured,
                                                       vehicleExcess, vehicleproratedPremium, vehicleproratedFsl, vehicleBrokerageRate, vehicleproratedBrokerage, vehicleCategory, vehicle.FleetNumber, term, vehicle, vehicleBurnerPremium);
                    mvTerm.TermCategory = "active";
                    mvTerm.AnnualPremium = vehiclePremium;
                    mvTerm.AnnualFSL = vehicleFsl;
                    mvTerm.AnnualBrokerage = vehicleBrokerage;
                    term.MotorTerms.Add(mvTerm);
                }
                else
                {
                    mvTerm.Registration = vehicle.Registration;
                    mvTerm.Year = vehicle.Year;
                    mvTerm.Make = vehicle.Make;
                    mvTerm.Model = vehicle.Model;
                    mvTerm.TermLimit = vehicle.GroupSumInsured;
                    mvTerm.Premium = vehicleproratedPremium;
                    mvTerm.BurnerPremium = vehicleBurnerPremium;
                    mvTerm.FSL = vehicleproratedFsl;
                    mvTerm.DateDeleted = null;
                    mvTerm.BrokerageRate = vehicleBrokerageRate;
                    mvTerm.Brokerage = vehicleproratedBrokerage;
                    mvTerm.VehicleCategory = vehicleCategory;
                    mvTerm.FleetNumber = vehicle.FleetNumber;
                    mvTerm.LastModifiedOn = DateTime.UtcNow;
                    mvTerm.LastModifiedBy = underwritingUser;
                    mvTerm.TermCategory = "active";
                    mvTerm.AnnualPremium = vehiclePremium;
                    mvTerm.AnnualFSL = vehicleFsl;
                    mvTerm.AnnualBrokerage = vehicleBrokerage;
                }

                //===========
                //For Change Agreement
                decimal vehicleFslPre = 0m;
                decimal vehicleFslDiffer = 0m;
                decimal vehiclePremiumPre = 0m;
                decimal vehiclePremiumDiffer = 0m;
                decimal vehicleExcessPre = 0m;
                decimal vehicleExcessDiffer = 0m;
                int vehicleTermLimitPre = 0;
                int vehicleTermLimitDiffer = 0;
                int prevehicleperiodindays = 0;
                decimal vehicleBrokeragePre = 0m;
                decimal vehicleBrokerageDiffer = 0m;

                if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null && vehicle.OriginalVehicle != null)
                {
                    ClientAgreementTerm termPre = agreement.ClientInformationSheet.PreviousInformationSheet.Programme.Agreements.FirstOrDefault().ClientAgreementTerms.FirstOrDefault(t => t.SubTermType == "MV" && t.DateDeleted == null);
                    if (termPre != null)
                    {
                        ClientAgreementMVTerm mvTermPre = termPre.MotorTerms.FirstOrDefault(mvpret => mvpret.Vehicle == vehicle.OriginalVehicle && mvpret.DateDeleted == null);

                        if (mvTermPre != null)
                        {
                            if (vehicle.OriginalVehicle.VehicleEffectiveDate > DateTime.MinValue)
                            {
                                prevehicleperiodindays = (vehicle.OriginalVehicle.VehicleExpireDate - vehicle.OriginalVehicle.VehicleEffectiveDate).Days;
                            }
                            else
                            {
                                prevehicleperiodindays = (vehicle.OriginalVehicle.VehicleExpireDate - vehicle.OriginalVehicle.VehicleInceptionDate).Days;
                            }

                            if (vehicle.OriginalVehicle.VehicleCeaseDate == DateTime.MinValue && vehicle.VehicleCeaseDate > DateTime.MinValue)
                            {
                                prevehicleperiodindays = (vehicle.VehicleExpireDate - vehicle.VehicleInceptionDate).Days;
                            }
                            if (vehicle.OriginalVehicle.VehicleEffectiveDate > DateTime.MinValue && vehicle.VehicleEffectiveDate > DateTime.MinValue)
                            {
                                if (vehicle.OriginalVehicle.VehicleCeaseDate == DateTime.MinValue && vehicle.VehicleCeaseDate > DateTime.MinValue && vehicle.OriginalVehicle.VehicleEffectiveDate == vehicle.VehicleEffectiveDate)
                                {
                                    prevehicleperiodindays = (vehicle.VehicleExpireDate - vehicle.VehicleEffectiveDate).Days;
                                }
                            }

                            if (vehicle.OriginalVehicle.VehicleEffectiveDate > DateTime.MinValue && vehicle.VehicleEffectiveDate > DateTime.MinValue)
                            {
                                if (vehicle.OriginalVehicle.VehicleCeaseDate == DateTime.MinValue && vehicle.VehicleCeaseDate > DateTime.MinValue && vehicle.OriginalVehicle.VehicleEffectiveDate == vehicle.VehicleEffectiveDate)
                                {
                                    prevehicleperiodindays = (vehicle.VehicleExpireDate - vehicle.VehicleEffectiveDate).Days;
                                }
                            }

                            vehicleFslPre = mvTermPre.FSL;
                            vehiclePremiumPre = mvTermPre.Premium;
                            vehicleExcessPre = mvTermPre.Excess;
                            vehicleTermLimitPre = mvTermPre.TermLimit;
                            vehicleBrokeragePre = mvTermPre.Brokerage;

                            totalVehicleFslPre += mvTermPre.FSL;
                            totalVehiclePremiumPre += mvTermPre.Premium;
                            totalVehicleTermLimitPre += mvTermPre.TermLimit;
                            totalVehicleBrokeragePre += mvTermPre.Brokerage;

                            if ((vehicle.GroupSumInsured == vehicle.OriginalVehicle.GroupSumInsured && vehicle.VehicleEffectiveDate == vehicle.OriginalVehicle.VehicleEffectiveDate &&
                                vehicle.VehicleCeaseDate == vehicle.OriginalVehicle.VehicleCeaseDate) ||
                                (vehicle.VehicleCeaseDate > DateTime.MinValue && vehicle.VehicleCeaseReason == 3))
                            {
                                vehicleFslDiffer = 0m;
                                vehiclePremiumDiffer = 0m;
                                vehicleExcessDiffer = 0;
                                vehicleTermLimitDiffer = 0;

                                totalVehicleFslDiffer += 0m;
                                totalVehiclePremiumDiffer += 0m;
                                totalVehicleTermLimitDiffer += 0;

                            }
                            else
                            {
                                if (vehicle.OriginalVehicle.VehicleCeaseDate > DateTime.MinValue && vehicle.VehicleCeaseDate == DateTime.MinValue)
                                {
                                    vehicleFslDiffer = vehicleproratedFsl;
                                    vehiclePremiumDiffer = vehicleproratedPremium;
                                    vehicleExcessDiffer = vehicleExcess;
                                    vehicleTermLimitDiffer = vehicle.GroupSumInsured;

                                    totalVehicleFslDiffer += vehicleproratedFsl;
                                    totalVehiclePremiumDiffer += vehicleproratedPremium;
                                    totalVehicleTermLimitDiffer += vehicle.GroupSumInsured;
                                }
                                else if (prevehicleperiodindays == 0)
                                {
                                    vehicleFslDiffer = vehicleproratedFsl - mvTermPre.FSL;
                                    vehiclePremiumDiffer = vehicleproratedPremium - mvTermPre.Premium;
                                    vehicleExcessDiffer = vehicleExcess - mvTermPre.Excess;
                                    vehicleTermLimitDiffer = vehicle.GroupSumInsured - mvTermPre.TermLimit;

                                    totalVehicleFslDiffer += vehicleproratedFsl - mvTermPre.FSL;
                                    totalVehiclePremiumDiffer += vehicleproratedPremium - mvTermPre.Premium;
                                    totalVehicleTermLimitDiffer += (vehicle.OriginalVehicle.VehicleCeaseDate > DateTime.MinValue) ? (vehicle.GroupSumInsured) : (vehicle.GroupSumInsured - mvTermPre.TermLimit);
                                }
                                else
                                {
                                    vehicleFslDiffer = vehicleproratedFsl - (mvTermPre.FSL * vehicleperiodindays / prevehicleperiodindays);
                                    vehiclePremiumDiffer = vehicleproratedPremium - (mvTermPre.Premium * vehicleperiodindays / prevehicleperiodindays);
                                    vehicleExcessDiffer = vehicleExcess - mvTermPre.Excess;
                                    vehicleTermLimitDiffer = vehicle.GroupSumInsured - mvTermPre.TermLimit;

                                    totalVehicleFslDiffer += vehicleproratedFsl - (mvTermPre.FSL * vehicleperiodindays / prevehicleperiodindays);
                                    totalVehiclePremiumDiffer += vehicleproratedPremium - (mvTermPre.Premium * vehicleperiodindays / prevehicleperiodindays);
                                    totalVehicleTermLimitDiffer += (vehicle.OriginalVehicle.VehicleCeaseDate > DateTime.MinValue) ? (vehicle.GroupSumInsured) : (vehicle.GroupSumInsured - mvTermPre.TermLimit);
                                }
                            }


                        }
                    }

                }
                else
                {
                    vehicleFslDiffer = vehicleproratedFsl;
                    vehiclePremiumDiffer = vehicleproratedPremium;
                    vehicleExcessDiffer = vehicleExcess;
                    vehicleTermLimitDiffer = vehicle.GroupSumInsured;

                    totalVehicleFslDiffer += vehicleproratedFsl;
                    totalVehiclePremiumDiffer += vehicleproratedPremium;
                    totalVehicleTermLimitDiffer += vehicle.GroupSumInsured;
                }

                vehicleBrokerageDiffer = (vehiclePremiumDiffer - vehicleFslDiffer) * vehicleBrokerageRate / 100;
                totalVehicleBrokerageDiffer += vehicleBrokerageDiffer;

                mvTerm.FSLPre = vehicleFslPre;
                mvTerm.FSLDiffer = vehicleFslDiffer;
                mvTerm.PremiumPre = vehiclePremiumPre;
                mvTerm.PremiumDiffer = vehiclePremiumDiffer;
                mvTerm.ExcessPre = vehicleExcessPre;
                mvTerm.ExcessDiffer = vehicleExcessDiffer;
                mvTerm.TermLimitPre = vehicleTermLimitPre;
                mvTerm.TermLimitDiffer = vehicleTermLimitDiffer;
                mvTerm.BrokeragePre = vehicleBrokeragePre;
                mvTerm.BrokerageDiffer = vehicleBrokerageDiffer;

                if (vehicle.VehicleCeaseDate > DateTime.MinValue && vehicle.VehicleCeaseReason == 4)
                {
                    mvTerm.TermCategory = "transfered";
                }
                else if (vehicle.VehicleCeaseDate > DateTime.MinValue && vehicle.VehicleCeaseReason != 4)
                {
                    mvTerm.TermCategory = "ceased";
                }
                else
                {
                    mvTerm.TermCategory = "active";
                }
                //===========

                totalTermLimit += vehicle.GroupSumInsured;
                totalTermFsl += vehicleproratedFsl;
                totalTermPremium += vehicleproratedPremium;
                totalTermBrokerage += vehicleproratedBrokerage;
            }

            term.TermLimit = totalTermLimit;
            term.Premium = totalTermPremium;
            term.FSL = totalTermFsl;
            term.BrokerageRate = agreement.Brokerage;
            term.Brokerage = totalTermBrokerage;
            term.TermLimitPre = totalBoatTermLimitPre + totalVehicleTermLimitPre;
            term.TermLimitDiffer = totalBoatTermLimitDiffer + totalVehicleTermLimitDiffer;
            term.PremiumPre = totalBoatPremiumPre + totalVehiclePremiumPre;
            term.PremiumDiffer = totalBoatPremiumDiffer + totalVehiclePremiumDiffer;
            term.FSLPre = totalBoatFslPre + totalVehicleFslPre;
            term.FSLDiffer = totalBoatFslDiffer + totalVehicleFslDiffer;
            term.BrokeragePre = totalBoatBrokeragePre + totalVehicleBrokeragePre;
            term.BrokerageDiffer = totalBoatBrokerageDiffer + totalVehicleBrokerageDiffer;
            term.Bound = true;

            //Referral points per agreement
            //Claim over $5k of losses
            uwrfclaimover5koflosses(underwritingUser, agreement);
            //Prior insurance
            uwrfpriorinsurance(underwritingUser, agreement);

            //Update agreement status
            if (agreement.ClientAgreementReferrals.Where(cref => cref.DateDeleted == null && cref.Status == "Pending").Count() > 0)
            {
                agreement.Status = "Referred";
            }
            else
            {
                agreement.Status = "Quoted";
            }

            //Set broker fee for change agreement
            if (agreement.ClientInformationSheet.IsChange && (bolNewBoatAdded || bolNewVehicleAdded))
            {
                agreement.BrokerFee = 15;
            }
            else if (agreement.ClientInformationSheet.IsChange && !bolNewBoatAdded && !bolNewVehicleAdded)
            {
                agreement.BrokerFee = 0;
            }

            string auditLogDetail = "Marsh Coastguard UW created/modified";
            AuditLog auditLog = new AuditLog(underwritingUser, informationSheet, agreement, auditLogDetail);
            agreement.ClientAgreementAuditLogs.Add(auditLog);

            return true;

        }

        ClientAgreement GetClientAgreement(User CurrentUser, ClientInformationSheet informationSheet, ClientProgramme programme, Product product, string reference)
        {
            ClientAgreement clientAgreement = programme.Agreements.FirstOrDefault(a => a.Product != null && a.Product.Id == product.Id);
            if (clientAgreement == null)
            {
                DateTime inceptionDate = DateTime.UtcNow;
                DateTime expiryDate = DateTime.UtcNow.AddYears(1);
                clientAgreement = new ClientAgreement(CurrentUser, informationSheet.Owner.Name, inceptionDate, expiryDate, product.DefaultBrokerage, product.DefaultBrokerFee, informationSheet, product, reference);
                programme.Agreements.Add(clientAgreement);
                clientAgreement.Status = "Quoted";

            }
            return clientAgreement;
        }

        ClientAgreementTerm GetAgreementTerm(User CurrentUser, ClientAgreement agreement, string subTerm)
        {
            ClientAgreementTerm term = agreement.ClientAgreementTerms.FirstOrDefault(t => t.SubTermType == subTerm && t.DateDeleted == null);
            if (term == null)
            {
                term = new ClientAgreementTerm(CurrentUser, 0, 0m, 0m, 0m, 0m, 0m, agreement, subTerm);
                agreement.ClientAgreementTerms.Add(term);
            }

            return term;
        }

        IDictionary<string, decimal> BuildRulesTable(ClientAgreement agreement, params string[] names)
        {
            var dict = new Dictionary<string, decimal>();

            foreach (string name in names)
                dict[name] = Convert.ToDecimal(agreement.ClientAgreementRules.FirstOrDefault(r => r.Name == name).Value);

            return dict;
        }

        void GetFslFeeForTrailer(IDictionary<string, decimal> rates, Vehicle vehicle, ref decimal vehicleFsl)
        {
            vehicleFsl = rates["fslfeefort"];
        }

        void GetFslRateForTraileredBoat(IDictionary<string, decimal> rates, Boat boat, ref decimal boatFslRate)
        {
            if (boat.BoatType2 != "Moored" && boat.BoatType2 != "Berthed") //Moored Craft
            {
                boatFslRate = rates["fslratefortc"];
            }
        }

        void GetBoatRateAndMinPremiumFor(IDictionary<string, decimal> rates, Boat boat, out decimal boatRate, out decimal boatMinPremium)
        {
            int boatUWCategory = 0;

            boatRate = 0M;
            boatMinPremium = 0M;

            if (boat.BoatType2 != null || boat.BoatType1 != null)
            {
                if (boat.BoatType2 == "Roadtrailer" || boat.BoatType2 == "Stored") //Trailer Craft
                {
                    if (boat.BoatType1 == "Jetboat")
                    {
                        boatUWCategory = 2; //Trailer Craft (jetboats)
                    }
                    else if (boat.BoatType1 == "Jetski")
                    {
                        boatUWCategory = 3; //Trailer Craft (jetskis)
                    }
                    else
                    {
                        boatUWCategory = 1; //Trailer Craft (excluding jetboats, jetskis)
                    }
                }
                else if (boat.BoatType2 == "Berthed" || boat.BoatType2 == "Moored") //Moored Craft
                {
                    boatUWCategory = 4;
                }
            }
            else
            {
                throw new Exception(string.Format("Can not get BoatType for boat", boat.Id));
            }

            switch (boatUWCategory)
            {
                case 1:
                    {
                        if (boat.MaxSumInsured > 0 && boat.MaxSumInsured <= 50000)
                        {
                            boatMinPremium = rates["tcunder50kminpremium"];
                            switch (boat.BoatQuoteExcessOption)
                            {
                                case 250:
                                    {
                                        boatRate = rates["tcunder50kexcess250rate"];
                                        break;
                                    }
                                case 500:
                                    {
                                        boatRate = rates["tcunder50kexcess500rate"];
                                        break;
                                    }
                                default:
                                    {
                                        throw new Exception(string.Format("Can not get rate or min premium for boat", boat.Id));
                                    }
                            }
                        }
                        else if (boat.MaxSumInsured > 50000 && boat.MaxSumInsured <= 100000)
                        {
                            boatMinPremium = rates["tc50k100kminpremium"];
                            switch (boat.BoatQuoteExcessOption)
                            {
                                case 250:
                                    {
                                        boatRate = rates["tc50k100kexcess250rate"];
                                        break;
                                    }
                                case 500:
                                    {
                                        boatRate = rates["tc50k100kexcess500rate"];
                                        break;
                                    }
                                default:
                                    {
                                        throw new Exception(string.Format("Can not get rate or min premium for boat", boat.Id));
                                    }
                            }
                        }
                        else if (boat.MaxSumInsured > 100000 && boat.MaxSumInsured <= 200000)
                        {
                            boatMinPremium = rates["tc100k200kminpremium"];
                            switch (boat.BoatQuoteExcessOption)
                            {
                                case 500:
                                    {
                                        boatRate = rates["tc100k200kexcess500rate"];
                                        break;
                                    }
                                case 1000:
                                    {
                                        boatRate = rates["tc100k200kexcess1000rate"];
                                        break;
                                    }
                                default:
                                    {
                                        throw new Exception(string.Format("Can not get rate or min premium for boat", boat.Id));
                                    }
                            }
                        }
                        else if (boat.MaxSumInsured > 200000 && boat.MaxSumInsured <= 250000)
                        {
                            boatMinPremium = rates["tc200k250kminpremium"];
                            switch (boat.BoatQuoteExcessOption)
                            {
                                case 500:
                                    {
                                        boatRate = rates["tc200k250kexcess500rate"];
                                        break;
                                    }
                                case 1000:
                                    {
                                        boatRate = rates["tc200k250kexcess1000rate"];
                                        break;
                                    }
                                default:
                                    {
                                        throw new Exception(string.Format("Can not get rate or min premium for boat", boat.Id));
                                    }
                            }
                        }
                        break;
                    }
                case 2:
                    {
                        if (boat.MaxSumInsured > 0 && boat.MaxSumInsured <= 100000)
                        {
                            boatMinPremium = rates["jbunder100kminpremium"];
                            switch (boat.BoatQuoteExcessOption)
                            {
                                case 500:
                                    {
                                        boatRate = rates["jbunder100kexcess500rate"];
                                        break;
                                    }
                                case 1000:
                                    {
                                        boatRate = rates["jbunder100kexcess1000rate"];
                                        break;
                                    }
                                default:
                                    {
                                        throw new Exception(string.Format("Can not get rate or min premium for boat", boat.Id));
                                    }
                            }
                        }
                        break;
                    }
                case 3:
                    {
                        if (boat.MaxSumInsured > 0 && boat.MaxSumInsured <= 100000)
                        {
                            boatMinPremium = rates["jsunder100kminpremium"];
                            switch (boat.BoatQuoteExcessOption)
                            {
                                case 500:
                                    {
                                        boatRate = rates["jsunder100kexcess500rate"];
                                        break;
                                    }
                                case 1000:
                                    {
                                        boatRate = rates["jsunder100kexcess1000rate"];
                                        break;
                                    }
                                default:
                                    {
                                        throw new Exception(string.Format("Can not get rate or min premium for boat", boat.Id));
                                    }
                            }
                        }
                        break; ;
                    }
                case 4:
                    {
                        if (boat.MaxSumInsured > 0 && boat.MaxSumInsured <= 100000)
                        {
                            boatMinPremium = rates["mcunder100kminpremium"];
                            switch (boat.BoatQuoteExcessOption)
                            {
                                case 250:
                                    {
                                        boatRate = rates["mcunder100kexcess250rate"];
                                        break;
                                    }
                                case 500:
                                    {
                                        boatRate = rates["mcunder100kexcess500rate"];
                                        break;
                                    }
                                default:
                                    {
                                        throw new Exception(string.Format("Can not get rate or min premium for boat", boat.Id));
                                    }
                            }
                        }
                        else if (boat.MaxSumInsured > 100000 && boat.MaxSumInsured <= 200000)
                        {
                            boatMinPremium = rates["mc100k200kminpremium"];
                            switch (boat.BoatQuoteExcessOption)
                            {
                                case 500:
                                    {
                                        boatRate = rates["mc100k200kexcess500rate"];
                                        break;
                                    }
                                case 1000:
                                    {
                                        boatRate = rates["mc100k200kexcess1000rate"];
                                        break;
                                    }
                                default:
                                    {
                                        throw new Exception(string.Format("Can not get rate or min premium for boat", boat.Id));
                                    }
                            }
                        }
                        else if (boat.MaxSumInsured > 200000 && boat.MaxSumInsured <= 350000)
                        {
                            boatMinPremium = rates["mc200k350kminpremium"];
                            switch (boat.BoatQuoteExcessOption)
                            {
                                case 1000:
                                    {
                                        boatRate = rates["mc200k350kexcess1000rate"];
                                        break;
                                    }
                                case 2000:
                                    {
                                        boatRate = rates["mc200k350kexcess2000rate"];
                                        break;
                                    }
                                default:
                                    {
                                        throw new Exception(string.Format("Can not get rate or min premium for boat", boat.Id));
                                    }
                            }
                        }
                        else if (boat.MaxSumInsured > 350000 && boat.MaxSumInsured <= 500000)
                        {
                            boatMinPremium = rates["mc350k500kminpremium"];
                            switch (boat.BoatQuoteExcessOption)
                            {
                                case 1000:
                                    {
                                        boatRate = rates["mc350k500kexcess1000rate"];
                                        break;
                                    }
                                case 2000:
                                    {
                                        boatRate = rates["mc350k500kexcess2000rate"];
                                        break;
                                    }
                                default:
                                    {
                                        throw new Exception(string.Format("Can not get rate or min premium for boat", boat.Id));
                                    }
                            }
                        }
                        break;
                    }
                default:
                    {
                        throw new Exception(string.Format("Can not get rate or min premium for boat", boat.Id));
                    }
            }
        }

        void uwrftrailercraftsuminsuredover250k(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrftrailercraftsuminsuredover250k" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrftrailercraftsuminsuredover250k") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrftrailercraftsuminsuredover250k").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrftrailercraftsuminsuredover250k").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrftrailercraftsuminsuredover250k").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrftrailercraftsuminsuredover250k").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrftrailercraftsuminsuredover250k").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrftrailercraftsuminsuredover250k" && cref.DateDeleted == null).Status != "Pending")
                {
                    if ((boat.BoatType2 == "Roadtrailer" || boat.BoatType2 == "Stored") && (boat.BoatType1 != "Jetboat" && boat.BoatType1 != "Jetski")) //Trailer Craft (excluding jetboats, jetskis)
                    {
                        if (boat.MaxSumInsured > 250000)
                        {
                            agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrftrailercraftsuminsuredover250k" && cref.DateDeleted == null).Status = "Pending";
                        }
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrftrailercraftsuminsuredover250k" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrftrailercraftsuminsuredover250k" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfjetboatsuminsuredover100k(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfjetboatsuminsuredover100k" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfjetboatsuminsuredover100k") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfjetboatsuminsuredover100k").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfjetboatsuminsuredover100k").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfjetboatsuminsuredover100k").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfjetboatsuminsuredover100k").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfjetboatsuminsuredover100k").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfjetboatsuminsuredover100k" && cref.DateDeleted == null).Status != "Pending")
                {
                    if ((boat.BoatType2 == "Roadtrailer" || boat.BoatType2 == "Stored") && boat.BoatType1 == "Jetboat") //Trailer Craft (jetboats)
                    {
                        if (boat.MaxSumInsured > 100000)
                        {
                            agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfjetboatsuminsuredover100k" && cref.DateDeleted == null).Status = "Pending";
                        }
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfjetboatsuminsuredover100k" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfjetboatsuminsuredover100k" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfjetskisuminsuredover100k(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfjetskisuminsuredover100k" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfjetskisuminsuredover100k") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfjetskisuminsuredover100k").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfjetskisuminsuredover100k").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfjetskisuminsuredover100k").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfjetskisuminsuredover100k").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfjetskisuminsuredover100k").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfjetskisuminsuredover100k" && cref.DateDeleted == null).Status != "Pending")
                {
                    if ((boat.BoatType2 == "Roadtrailer" || boat.BoatType2 == "Stored") && boat.BoatType1 == "Jetski") //Trailer Craft (jetskis)
                    {
                        if (boat.MaxSumInsured > 100000)
                        {
                            agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfjetskisuminsuredover100k" && cref.DateDeleted == null).Status = "Pending";
                        }
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfjetskisuminsuredover100k" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfjetskisuminsuredover100k" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfmooredcraftsuminsuredover500k(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmooredcraftsuminsuredover500k" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmooredcraftsuminsuredover500k") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmooredcraftsuminsuredover500k").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmooredcraftsuminsuredover500k").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmooredcraftsuminsuredover500k").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmooredcraftsuminsuredover500k").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmooredcraftsuminsuredover500k").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmooredcraftsuminsuredover500k" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (boat.BoatType2 == "Moored" || boat.BoatType2 == "Berthed") //Moored Craft
                    {
                        if (boat.MaxSumInsured > 500000)
                        {
                            agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmooredcraftsuminsuredover500k" && cref.DateDeleted == null).Status = "Pending";
                        }
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmooredcraftsuminsuredover500k" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmooredcraftsuminsuredover500k" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfyearbuiltpre1985(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfyearbuiltpre1985" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfyearbuiltpre1985") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfyearbuiltpre1985").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfyearbuiltpre1985").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfyearbuiltpre1985").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfyearbuiltpre1985").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfyearbuiltpre1985").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfyearbuiltpre1985" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (boat.YearOfManufacture < 1985)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfyearbuiltpre1985" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfyearbuiltpre1985" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfyearbuiltpre1985" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfnotbuiltprofessionally(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotbuiltprofessionally" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotbuiltprofessionally") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotbuiltprofessionally").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotbuiltprofessionally").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotbuiltprofessionally").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotbuiltprofessionally").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfnotbuiltprofessionally").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotbuiltprofessionally" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (boat.BuiltProfessionally == "False")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotbuiltprofessionally" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotbuiltprofessionally" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfnotbuiltprofessionally" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfboattype1other(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboattype1other" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboattype1other") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboattype1other").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboattype1other").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboattype1other").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboattype1other").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboattype1other").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboattype1other" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (boat.BoatType1 == "Other")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboattype1other" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboattype1other" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboattype1other" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfhullconstructioncarbonorother(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhullconstructioncarbonorother" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhullconstructioncarbonorother") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhullconstructioncarbonorother").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhullconstructioncarbonorother").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhullconstructioncarbonorother").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhullconstructioncarbonorother").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhullconstructioncarbonorother").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhullconstructioncarbonorother" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (boat.HullConstruction == "Carbon" || boat.HullConstruction == "Other")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhullconstructioncarbonorother" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhullconstructioncarbonorother" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhullconstructioncarbonorother" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfhullconfigurationtrimaranorother(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhullconfigurationtrimaranorother" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhullconfigurationtrimaranorother") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhullconfigurationtrimaranorother").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhullconfigurationtrimaranorother").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhullconfigurationtrimaranorother").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhullconfigurationtrimaranorother").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfhullconfigurationtrimaranorother").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhullconfigurationtrimaranorother" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (boat.HullConfiguration == "Trimaran" || boat.HullConfiguration == "Other")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhullconfigurationtrimaranorother" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhullconfigurationtrimaranorother" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfhullconfigurationtrimaranorother" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfmotortypeinboardpetrolorjet(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmotortypeinboardpetrolorjet" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmotortypeinboardpetrolorjet") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmotortypeinboardpetrolorjet").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmotortypeinboardpetrolorjet").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmotortypeinboardpetrolorjet").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmotortypeinboardpetrolorjet").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmotortypeinboardpetrolorjet").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmotortypeinboardpetrolorjet" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (boat.MotorType == "InboardPetrol" || boat.MotorType == "Jet")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmotortypeinboardpetrolorjet" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmotortypeinboardpetrolorjet" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmotortypeinboardpetrolorjet" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfmotormodified(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmotormodified" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmotormodified") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmotormodified").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmotormodified").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmotormodified").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmotormodified").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmotormodified").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmotormodified" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (boat.ModifiedMotor == "True")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmotormodified" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmotormodified" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmotormodified" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfmaxspeedover60knots(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmaxspeedover60knots" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmaxspeedover60knots") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmaxspeedover60knots").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmaxspeedover60knots").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmaxspeedover60knots").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmaxspeedover60knots").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfmaxspeedover60knots").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmaxspeedover60knots" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (boat.MaxRatedSpeed == "Over60Knots")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmaxspeedover60knots" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmaxspeedover60knots" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfmaxspeedover60knots" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfclaimover5koflosses(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclaimover5koflosses" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclaimover5koflosses") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclaimover5koflosses").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclaimover5koflosses").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclaimover5koflosses").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclaimover5koflosses").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfclaimover5koflosses").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclaimover5koflosses" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.ClientInformationSheet.ClaimNotifications.Any(clm => clm.ClaimEstimateInsuredLiability > 5000))
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclaimover5koflosses" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclaimover5koflosses" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfclaimover5koflosses" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfpriorinsurance(User underwritingUser, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpriorinsurance" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpriorinsurance") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpriorinsurance").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpriorinsurance").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpriorinsurance").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpriorinsurance").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfpriorinsurance").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpriorinsurance" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasDamageLossOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasWithdrawnOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasRefusedOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasStatutoryOffenceOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "ClaimsHistoryViewModel.HasLiquidationOptions").First().Value == "1" ||
                        agreement.ClientInformationSheet.ClaimNotifications.Where(acscn => acscn.DateDeleted == null && !acscn.Removed && (acscn.ClaimStatus == "Settled" || acscn.ClaimStatus == "Precautionary notification only" || acscn.ClaimStatus == "Part Settled")).Count() > 0)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpriorinsurance" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpriorinsurance" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfpriorinsurance" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfboatuseliveonboard(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboatuseliveonboard" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboatuseliveonboard") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboatuseliveonboard").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboatuseliveonboard").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboatuseliveonboard").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboatuseliveonboard").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboatuseliveonboard").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboatuseliveonboard" && cref.DateDeleted == null).Status != "Pending")
                {
                    if ((boat.BoatType1 == "CruisersandLaunches" || boat.BoatType1 == "YachtsandCatamarans")
                        && boat.BoatUses.Any(ycbu => ycbu.BoatUseCategory == "LiveOnBoard" && ycbu.DateDeleted == null && !ycbu.Removed))
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboatuseliveonboard" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboatuseliveonboard" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboatuseliveonboard" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfboatuseraceotheroption(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboatuseraceotheroption" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboatuseraceotheroption") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboatuseraceotheroption").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboatuseraceotheroption").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboatuseraceotheroption").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboatuseraceotheroption").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfboatuseraceotheroption").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboatuseraceotheroption" && cref.DateDeleted == null).Status != "Pending")
                {
                    if (boat.BoatType1 == "YachtsandCatamarans" && boat.BoatUses.Any(ycbu => ycbu.BoatUseCategory == "Race" && ycbu.BoatUseRaceUseSpinnakers == "True" && ycbu.DateDeleted == null && !ycbu.Removed))
                    {
                        if (boat.BoatUses.Any(ycbu => ycbu.BoatUseRaceCategory == "Oceangoingracingover200nm") ||
                            boat.BoatUses.Any(ycbu => ycbu.BoatUseRaceCategory == "Category1Racing")) //BoatUseRaceCategory (Oceangoingracingover200nm, Category1Racing)
                        {
                            agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboatuseraceotheroption" && cref.DateDeleted == null).Status = "Pending";
                        }
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboatuseraceotheroption" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfboatuseraceotheroption" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfswingmooredtype(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfswingmooredtype" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfswingmooredtype") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfswingmooredtype").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfswingmooredtype").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfswingmooredtype").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfswingmooredtype").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfswingmooredtype").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfswingmooredtype" && cref.DateDeleted == null).Status != "Pending")
                {
                    if ((boat.BoatType2 == "Berthed" || boat.BoatType2 == "Moored") && boat.WaterLocationMooringType == "Swing")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfswingmooredtype" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfswingmooredtype" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfswingmooredtype" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfothermarinatc(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            agreement.ReferToTC = false;

            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfothermarinatc" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfothermarinatc") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfothermarinatc").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfothermarinatc").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfothermarinatc").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfothermarinatc").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfothermarinatc").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfothermarinatc" && cref.DateDeleted == null).Status != "Pending")
                {
                    if ((boat.BoatType2 == "Berthed" || boat.BoatType2 == "Moored") && boat.BoatWaterLocation == null && boat.OtherMarina && boat.OtherMarinaName != "")
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfothermarinatc" && cref.DateDeleted == null).Status = "Pending";
                        agreement.ReferToTC = true;
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfothermarinatc" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfothermarinatc" && cref.DateDeleted == null).Status = "";
                }
            }
        }

        void uwrfothermarina(User underwritingUser, Boat boat, ClientAgreement agreement)
        {
            if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfothermarina" && cref.DateDeleted == null) == null)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfothermarina") != null)
                    agreement.ClientAgreementReferrals.Add(new ClientAgreementReferral(underwritingUser, agreement, agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfothermarina").Name,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfothermarina").Description,
                        "",
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfothermarina").Value,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfothermarina").OrderNumber,
                        agreement.ClientAgreementRules.FirstOrDefault(cr => cr.RuleCategory == "uwreferral" && cr.DateDeleted == null && cr.Value == "uwrfothermarina").DoNotCheckForRenew));
            }
            else
            {
                if (agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfothermarina" && cref.DateDeleted == null).Status != "Pending")
                {
                    if ((boat.BoatType2 == "Berthed" || boat.BoatType2 == "Moored") && boat.BoatWaterLocation != null && !boat.BoatWaterLocation.IsApproved &&
                        boat.BoatWaterLocation.InsuranceAttributes.FirstOrDefault(bwlocia => bwlocia.InsuranceAttributeName == "Other Marina" && bwlocia.DateDeleted == null) != null)
                    {
                        agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfothermarina" && cref.DateDeleted == null).Status = "Pending";
                    }
                }

                if (agreement.ClientInformationSheet.IsRenewawl
                            && agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfothermarina" && cref.DateDeleted == null).DoNotCheckForRenew)
                {
                    agreement.ClientAgreementReferrals.FirstOrDefault(cref => cref.ActionName == "uwrfothermarina" && cref.DateDeleted == null).Status = "";
                }
            }
        }


    }
}

