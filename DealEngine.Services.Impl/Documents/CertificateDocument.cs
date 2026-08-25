using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DealEngine.Services.Interfaces.Models;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using DealEngine.Services.Interfaces.Enums;
namespace DealEngine.Services.Impl.Documents
{
    public class CertificateDocument : IDocument
    {
        private readonly CertificateAggregateModel _model;
        private readonly IWebHostEnvironment _env;
        public CertificateDocument(CertificateAggregateModel model , IWebHostEnvironment env)
        {
            _model = model;
            _env = env;
        }

       

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;


        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginVertical(15, Unit.Millimetre);
                page.MarginHorizontal(25, Unit.Millimetre);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    AddLogo(column);
                    AddHeader(column);
                    AddCertificationText(column);   // 👈 THIS IS WHERE IT IS USED
                    switch (_model.CertificateType)
                    {
                        case CertificateType.MD:
                            ComposeMaterialDamage(column);
                            break;

                        case CertificateType.AS:
                            ComposeManagementLiability(column);
                            break;

                        case CertificateType.MLGGL:
                            // ComposePublicLiability(column);
                            ComposeGlobalGuardLiability(column);

                            break;
                    }

                    AddFooterNotes(column);
                });
            });
        }

        private void AddHeader(ColumnDescriptor column)
        {
            column.Item().Border(1).Padding(10).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Rotary Zone Insurance and Risk");
                    col.Item().Text("Management – Oceania ONLY");
                    col.Item().Text("a division of Rotary International Inc");
                    col.Item().Text("Zone 8 (Australasia and Oceania)");
                });

                row.ConstantItem(250).BorderLeft(1).PaddingLeft(10).Column(col =>
                {
                    col.Item().AlignRight().Text("Certificate of Currency")
                        .FontSize(16).Bold();

                    col.Item().AlignRight()
                        .Text($"Date of Issue: {_model.IssueDate}")
                        .Bold();

                    col.Item().AlignRight()
                        .Text("Contact: The District Insurance Officer")
                        .Bold();
                });
            });
        }


        private void ComposeGlobalGuardLiability(ColumnDescriptor column)
        {
            column.Item().Border(1).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(220);   // Left label column
                    columns.RelativeColumn();      // Right value column
                });

                void Row(string label, string value, bool grey = true)
                {
                    table.Cell()
                        .BorderBottom(1)
                        .Padding(8)
                        .Background(grey ? "#F3F3F3" : Colors.White)
                        .Text(label)
                        .Bold();

                    table.Cell()
                        .BorderBottom(1)
                        .Padding(8)
                        .Text(value);
                }

                // ==============================
                // ROWS EXACTLY LIKE YOUR CERT
                // ==============================

                Row("Policy Type", _model.PolicyType);

                Row("Insured",
                    "Rotary International Districts 9910, 9920, 9940, 9930 and 9999, NZ and Pacific Island (Oceania) based Rotary Clubs, Rotaract Clubs, Rotary Community Core Clubs, Rota and Interact Clubs in the District including NZ Charitable Trusts formed within those clubs, including the New Zealand Rotary Clubs Charitable Trust, ROZops Limited and Rotary New Zealand World Community Service Limited");

                Row("Named Parties", _model.NamedParties);

                Row("Business Description", "Charitable organisation including fundraising and events");

                Row("Insurer", "Chubb Group Of Insurance Companies");

                Row("Policy Number(s)", "NZCASA10919");

                table.Cell().BorderBottom(1).Padding(8).Background("#F3F3F3")
                .Text("Period of Insurance").Bold();

                table.Cell().BorderBottom(1).Padding(8).Column(col =>
                {
                    col.Item().Text(
                        $"From 4.00 pm {_model.InceptionDate:dd/MM/yyyy} Local Standard Time");

                    col.Item().Text(
                        $"To 4.00 pm {_model.ExpiryDate:dd/MM/yyyy} Local Standard Time");
                });

                Row("Interest Insured", "The Insured’s Legal Liability to pay compensation in respect of:\r\n(a) Personal Injury\r\n(b) Property Damage\r\n(c) Advertising Injury \r\nOccurring within the Policy Territory during the Policy Period as a result of an Occurrence happening in connection with Business of the Insured.");

                Row("Limits of Liability", "NZD10,000,000 any one Occurrence and in the aggregate in respect of the Products Hazard");

                Row("Deductible", "NZD 500 each and every Occurrence\r\n\r\nNZD2,500 each and every other Occurrence outside New Zealand");

                Row("Jurisdictional Limit", "Worldwide with USA/Canada terms as follows:\r\n\r\nIndemnity Limit to be costs inclusive\r\n\r\nPunitive and Exemplary Damages excluded\r\n\r\nPollution excluded");

                Row("Geographical Limit", "Worldwide");

            });
        }



        private void ComposeManagementLiability(ColumnDescriptor column)
        {
            column.Item().Border(1).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(220);   // Left label column
                    columns.RelativeColumn();      // Right value column
                });

                void Row(string label, string value, bool grey = true)
                {
                    table.Cell()
                        .BorderBottom(1)
                        .Padding(8)
                        .Background(grey ? "#F3F3F3" : Colors.White)
                        .Text(label)
                        .Bold();

                    table.Cell()
                        .BorderBottom(1)
                        .Padding(8)
                        .Text(value);
                }

                // ==============================
                // ROWS EXACTLY LIKE YOUR CERT
                // ==============================

                Row("Policy Type", _model.PolicyType);

                Row("Insured",
                    "Rotary International Districts 9910, 9920, 9940, 9930 and 9999, NZ and Pacific Island (Oceania) based Rotary Clubs, Rotaract Clubs, Rotary Community Core Clubs, Rota and Interact Clubs in the District including NZ Charitable Trusts formed within those clubs, including the New Zealand Rotary Clubs Charitable Trust, ROZops Limited and Rotary New Zealand World Community Service Limited");

                Row("Named Parties", _model.NamedParties);

                Row("Business Description", "Charitable organisation including fundraising and events");

                Row("Insurer", "Chubb Group Of Insurance Companies");

                Row("Policy Number(s)", "NZDAOA09893");

                table.Cell().BorderBottom(1).Padding(8).Background("#F3F3F3")
                .Text("Period of Cover by Reserve Fund").Bold();

                table.Cell().BorderBottom(1).Padding(8).Column(col =>
                {
                    col.Item().Text(
                        $"From 4.00 pm {_model.InceptionDate:dd/MM/yyyy} Local Standard Time");

                    col.Item().Text(
                        $"To 4.00 pm {_model.ExpiryDate:dd/MM/yyyy} Local Standard Time");
                });

                Row("Interest Insured", "The Insured’s Management Legal Liability covering:\r\n\r\nPart 1 Cover A) Directors’ and Officers’ Liability Coverage Section B) Employment Practices Liability Coverage Section C) Miscellaneous Professional Liability Coverage Section D) Crime Coverage Section E) Kidnap, Ransom and Extortion Coverage Section F) Cyber Coverage Section Part 2 Cover A) Statutory Liability Coverage Section B) Employers’ Liability Coverage Section C) Public and Products Liability Coverage Section Part 3 Cover A) Umbrella Defence Costs Coverage Section\r\n\r\nduring the Policy Period as a result of an Occurrence happening in connection with the Business of the Insured, excluding the United States of America, Canada or their respective protectorates and territories.");

                Row("Limits of Liability", "Limits vary but are for any one Occurrence and in the aggregate and otherwise are as detailed on the Schedule.");

                Row("Deductible", "Varies for each type of cover");

                Row("Jurisdictional Limit", "Oceania");

                Row("Geographical Limit", "Worldwide");

                Row("Endorsements", "(a) Event management (b) Molestation (c) Cyber Incidents (d) Insolvency");
            });
        }

        private void ComposeMaterialDamage(ColumnDescriptor column)
        {
            column.Item().Border(1).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(220);   // Left label column
                    columns.RelativeColumn();      // Right value column
                });

                void Row(string label, string value, bool grey = true)
                {
                    table.Cell()
                        .BorderBottom(1)
                        .Padding(8)
                        .Background(grey ? "#F3F3F3" : Colors.White)
                        .Text(label)
                        .Bold();

                    table.Cell()
                        .BorderBottom(1)
                        .Padding(8)
                        .Text(value);
                }

                // ==============================
                // ROWS EXACTLY LIKE YOUR CERT
                // ==============================

                Row("Policy Type", _model.PolicyType);

                Row("Insured as Contributors to the Material Damage Reserve Fund",
                    "Rotary Clubs are widely recognised for their diverse community and humanitarian activities. Their main programs include community service projects, fundraising events for local causes and the Rotary Foundation, youth exchange programs, and collaboration with Rotarian Action Groups.");

                Row("Named Parties", _model.NamedParties);

                Row("Business Description", "Charitable organisation including fundraising and events");

                Row("Insurer", "Not Applicable - This is a self managed reserve fund. See the Material Damage Reserve Fund Rules provided in email to you.");

                Row("Policy Number(s)", "This certificate is issued to Named Parties.");

                table.Cell().BorderBottom(1).Padding(8).Background("#F3F3F3")
                .Text("Period of Cover by Reserve Fund").Bold();

                table.Cell().BorderBottom(1).Padding(8).Column(col =>
                {
                    col.Item().Text(
                        $"From 4.00 pm {_model.InceptionDate:dd/MM/yyyy} Local Standard Time");

                    col.Item().Text(
                        $"To 4.00 pm {_model.ExpiryDate:dd/MM/yyyy} Local Standard Time");
                });

                Row("Interest Insured", "The Insured’s assets (excluding real estate) up to the value of NZD5000.00 as set out in the Material Damage Reserve Fund Rules during the Period of Cover and as a result of an Occurrence happening in connection with the assets (excluding real estate) of the Insured, excluding the United States of America, Canada or their respective protectorates and territories.");

                Row("Limits of Liability", "Limits vary but are for any one Occurrence and in the aggregate and otherwise are as detailed on the Material Damage Reserve Fund Rules");

                Row("Deductible", "Varies for each type of cover");

                Row("Jurisdictional Limit", "Oceania");

                Row("Geographical Limit", "Worldwide");

                Row("Endorsements", "(a) Event management (b) Molestation (c) Cyber Incidents (d) Insolvency");
            });
        }
        private void AddRow(TableDescriptor table, string label, string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            table.Cell().BorderBottom(1)
                .Padding(8)
                .Background("#F3F3F3")
                .Text(label)
                .Bold();

            table.Cell().BorderBottom(1)
                .Padding(8)
                .Text(value);
        }

        private void AddCertificationText(ColumnDescriptor column)
        {
            column.Item().PaddingTop(10)
                .Text("We hereby certify that the under mentioned insurance policy is current as at the date of this certificate, please refer to the important notices below.")
                .Bold();
        }

        private void AddFooterNotes(ColumnDescriptor column)
        {
            column.Spacing(8);

            // =========================
            // Further Information
            // =========================
            column.Item()
                .PaddingTop(20)
                .Text("Further Information")
                .FontSize(14)
                .Bold();

            column.Item()
                .Text("Should you have any queries, please contact us on the details set out at the top of the page.")
                .FontSize(10);

            // =========================
            // Important Notes
            // =========================
            column.Item()
                .PaddingTop(15)
                .Text("Important notes")
                .FontSize(14)
                .Bold();

            //column.Item().Column(notes =>
            //{
            //    notes.Spacing(4);

            //    notes.Item().Text("• Aon does not guarantee that the insurance outlined in this Certificate will continue to remain in force for the period referred to as the Policy.");
            //    notes.Item().Text("• Aon accepts no responsibility or liability to advise any party who may be relying on this Certificate.");
            //    notes.Item().Text("• Subject to full payment of premium.");
            //    notes.Item().Text("• This certificate does not:");
            //    notes.Item().Text("    - represent an insurance contract or confer rights to the recipient;");
            //    notes.Item().Text("    - amend, extend or alter the Policy; or");
            //    notes.Item().Text("    - contain the full policy terms and conditions.");
            //});

            column.Item().Column(notes =>
            {
                notes.Spacing(4);

                notes.Item().Text("• The Not4Profit Foundation, as administrator of the Zone 8 Insurance and Protection Committee - Oceania does not guarantee that the fund outlined in this Certificate will continue to remain in force for the period referred to as the Period of Cover and is limited to both the limit for any one claim and in total the limit of the Reserve Fund.");
                notes.Item().Text("• The fund may cancel this Contribution Certificate if payment is not made or completed or the fund is exhausted by accepted claims against the Fund.");
                notes.Item().Text("• This is NOT a Policy under the Contracts of Insurance Act 2024.");
                notes.Item().Text("• The Not4Profit Foundation accepts no responsibility or liability to advise any party who may be relying on this Certificate of such alteration to or cancellation of the Rules of the Fund.");
                notes.Item().Text("• Subject to full payment of contributions");

                notes.Item().Text("• This certificate does not:");
                notes.Item().Text("    - represent an insurance contract or confer rights to the recipient;");
                notes.Item().Text("    - amend, extend or alter the Policy; or");
                notes.Item().Text("    - contain the full policy terms and conditions.");
            });
        }

        private void AddLogo(ColumnDescriptor column)
        {
          //  var logoPath = @"C:\database\AON LOGO.jpg"; // change later to production path
            var logoPath = Path.Combine(
                   _env.WebRootPath,
                   "Image",
                   "AON LOGO.jpg");

            if (!File.Exists(logoPath))
                return;

            var logoBytes = File.ReadAllBytes(logoPath);

            column.Item()
            .AlignCenter()
            .Width(90)   // match original width
            .Image(logoBytes);
               // adjust size here (60–90 works well)
        }
    }
}
