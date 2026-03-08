using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;
using DealEngine.Services.Interfaces.Enums;
using DealEngine.Services.Interfaces.Models;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;



namespace DealEngine.Services.Impl
{
    public class CertificateBuilderService : ICertificateBuilderService
    {
        public async Task<CertificateAggregateModel> BuildAsync(
            ClientAgreement agreement,
            ClientProgramme programme,
            CertificateType certificateType
            )
        {


            var sheet = agreement.ClientInformationSheet;

            var model = new CertificateAggregateModel();


            model.PolicyNumber = agreement.PolicyNumber;

            model.PolicyType = certificateType switch
            {
                CertificateType.MD =>
                    "Material Damage Cover Certificate",

                CertificateType.AS =>
                    "Associations Cover Certificate",

                CertificateType.MLGGL =>
                    "Rotary_Oceania_Associations_GL_Liability_Multiguard Cover Certificate",

                _ =>
                    "Certificate"
            };
            model.CertificateTitle = model.PolicyType;


            model.IssueDate =
                DateTime.UtcNow.ToString("dd MMMM yyyy",
                    CultureInfo.CreateSpecificCulture("en-NZ"));

            model.InceptionDate =
                agreement.InceptionDate.ToString("dd/MM/yyyy",
                    CultureInfo.CreateSpecificCulture("en-NZ"));

            model.ExpiryDate =
                agreement.ExpiryDate.ToString("dd/MM/yyyy",
                    CultureInfo.CreateSpecificCulture("en-NZ"));

            model.Insurer = "Chubb Group Of Insurance Companies";

            model.BusinessDescription =
                "Charitable organisation including fundraising and events";

            model.InterestInsured =
                "The Insured’s assets (excluding real estate) up to NZD 5,000";

            model.LimitsOfLiability =
                "As detailed in the Material Damage Reserve Fund Rules";

            model.Deductible = "Varies for each type of cover";

            model.JurisdictionalLimit = "Oceania";

            model.GeographicalLimit = "Worldwide";

            model.LogoUrl =
                "https://insure.rotaryoceania.zone/Image/AON LOGO.jpg";

            // ===============================
            // Named Parties
            // ===============================

            string namedParties = sheet.Owner.Name;
            if (sheet.Organisation.Any(org =>
        org.Removed == false &&
        org.OrganisationType != null &&
        org.OrganisationType.Name != "Private"))
         //       if (sheet.Organisation.Where(org => org.Removed == false && org.OrganisationType.Name != "Private"))

               // if (sheet.Organisation.Any(o => !o.Removed && o.DateDeleted == null))
            {
                var additional = sheet.Organisation
                    .Where(o => !o.Removed && o.DateDeleted == null && o.OrganisationType.Name != "Private")
                    .Select(o => o.Name);

                namedParties = string.Join(", ",
                    additional.Append(sheet.Owner.Name));
            }

            model.NamedParties = namedParties;

            // ===============================
            // Endorsements
            // ===============================


            return model;
        }
    }
}

