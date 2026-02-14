using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using DealEngine.Services.Interfaces.Models;


namespace DealEngine.Services.Impl.Documents
{
    public class CertificateDocument : IDocument
    {
        private readonly CertificateAggregateModel _model;

        public CertificateDocument(CertificateAggregateModel model)
        {
            _model = model;
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

                    // =========================
                    // HEADER BOX
                    // =========================
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
                                .FontSize(16)
                                .Bold();

                            col.Item().PaddingTop(5)
                                .AlignRight()
                                .Text($"Date of Issue: {_model.IssueDate}")
                                .Bold();

                            col.Item().PaddingTop(5)
                                .AlignRight()
                                .Text("Contact: The District Insurance Officer")
                                .Bold();
                        });
                    });

                    // =========================
                    // CERTIFICATION TEXT
                    // =========================
                    column.Item().PaddingTop(10)
                        .Text("We hereby certify that the under mentioned insurance policy is current as at the date of this certificate, please refer to the important notices below.")
                        .Bold();

                    column.Item().PaddingTop(10);

                    // =========================
                    // MAIN DETAILS TABLE
                    // =========================
                    column.Item().Border(1).Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(200);
                            columns.RelativeColumn();
                        });

                        void Row(string label, string value)
                        {
                            table.Cell().BorderBottom(1).Padding(8).Background("#F3F3F3").Text(label).Bold();
                            table.Cell().BorderBottom(1).Padding(8).Text(value);
                        }

                        Row("Policy Type", _model.PolicyType);
                        Row("Insured as Contributors to the Material Damage Reserve Fund",
                            _model.BusinessDescription);
                        Row("Named Parties", _model.NamedParties);
                        Row("Business Description", _model.BusinessDescription);
                        Row("Insurer", _model.Insurer);
                        Row("Policy Number(s)", _model.PolicyNumber);
                        Row("Period of Cover by Reserve Fund",
                            $"{_model.InceptionDate} - {_model.ExpiryDate}");
                        Row("Interest Insured", _model.InterestInsured);
                        Row("Limits of Liability", _model.LimitsOfLiability);
                        Row("Deductible", _model.Deductible);
                        Row("Jurisdictional Limit", _model.JurisdictionalLimit);
                        Row("Geographical Limit", _model.GeographicalLimit);
                        Row("Endorsements", _model.Endorsements);
                    });

                    // =========================
                    // FURTHER INFORMATION
                    // =========================
                    column.Item().PaddingTop(20)
                        .Text("Further Information")
                        .FontSize(14)
                        .Bold();

                    column.Item().Text("Should you have any queries, please contact us on the details set out at the top of the page.");

                    // =========================
                    // IMPORTANT NOTES
                    // =========================
                    column.Item().PaddingTop(20)
                        .Text("Important notes")
                        .FontSize(14)
                        .Bold();

                    column.Item().PaddingTop(10).Column(notes =>
                    {
                        notes.Spacing(5);

                        notes.Item().Text("• The Not4Profit Foundation does not guarantee that the fund outlined in this Certificate will continue to remain in force.");
                        notes.Item().Text("• The fund may cancel this Contribution Certificate if payment is not made or completed.");
                        notes.Item().Text("• This is NOT a Policy under the Contracts of Insurance Act 2024.");
                        notes.Item().Text("• The Not4Profit Foundation accepts no responsibility or liability to advise any party.");
                        notes.Item().Text("• Subject to full payment of contributions.");
                        notes.Item().Text("• This certificate does not:");
                        notes.Item().Text("    - represent an insurance contract or confer rights to the recipient;");
                        notes.Item().Text("    - amend, extend or alter the Reserve Fund scheme; or");
                        notes.Item().Text("    - contain the full Reserve Fund rules and terms and conditions.");
                    });
                });
            });
        }
        //public void Compose(IDocumentContainer container)
        //{
        //    container.Page(page =>
        //    {
        //        page.Size(PageSizes.A4);
        //        page.Margin(25);
        //        page.DefaultTextStyle(x => x.FontSize(11));

        //        page.Content().Column(column =>
        //        {
        //            column.Spacing(10);

        //            column.Item().Row(row =>
        //            {
        //                row.RelativeItem().Text(_model.CertificateTitle)
        //                    .FontSize(18)
        //                    .Bold();

        //                row.ConstantItem(150).AlignRight().Column(col =>
        //                {
        //                    col.Item().Text($"Issue Date: {_model.IssueDate}");
        //                    col.Item().Text($"Policy: {_model.PolicyNumber}");
        //                });
        //            });

        //            column.Item().LineHorizontal(1);

        //            column.Item().Table(table =>
        //            {
        //                table.ColumnsDefinition(columns =>
        //                {
        //                    columns.ConstantColumn(180);
        //                    columns.RelativeColumn();
        //                });

        //                void Row(string label, string value)
        //                {
        //                    table.Cell().PaddingVertical(5).Text(label).Bold();
        //                    table.Cell().PaddingVertical(5).Text(value);
        //                }

        //                Row("Policy Type", _model.PolicyType);
        //                Row("Named Parties", _model.NamedParties);
        //                Row("Business Description", _model.BusinessDescription);
        //                Row("Insurer", _model.Insurer);
        //                Row("Period", $"{_model.InceptionDate} - {_model.ExpiryDate}");
        //                Row("Interest Insured", _model.InterestInsured);
        //                Row("Limits of Liability", _model.LimitsOfLiability);
        //                Row("Deductible", _model.Deductible);
        //                Row("Jurisdictional Limit", _model.JurisdictionalLimit);
        //                Row("Geographical Limit", _model.GeographicalLimit);
        //                Row("Endorsements", _model.Endorsements);
        //            });

        //            column.Item().PaddingTop(20).Text("Important Notes")
        //                .Bold()
        //                .FontSize(14);
        //        });
        //    });
        //}
    }
}
