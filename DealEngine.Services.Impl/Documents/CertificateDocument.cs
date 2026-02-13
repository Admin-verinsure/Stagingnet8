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
                page.Margin(25);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Content().Column(column =>
                {
                    column.Spacing(10);

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Text(_model.CertificateTitle)
                            .FontSize(18)
                            .Bold();

                        row.ConstantItem(150).AlignRight().Column(col =>
                        {
                            col.Item().Text($"Issue Date: {_model.IssueDate}");
                            col.Item().Text($"Policy: {_model.PolicyNumber}");
                        });
                    });

                    column.Item().LineHorizontal(1);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(180);
                            columns.RelativeColumn();
                        });

                        void Row(string label, string value)
                        {
                            table.Cell().PaddingVertical(5).Text(label).Bold();
                            table.Cell().PaddingVertical(5).Text(value);
                        }

                        Row("Policy Type", _model.PolicyType);
                        Row("Named Parties", _model.NamedParties);
                        Row("Business Description", _model.BusinessDescription);
                        Row("Insurer", _model.Insurer);
                        Row("Period", $"{_model.InceptionDate} - {_model.ExpiryDate}");
                        Row("Interest Insured", _model.InterestInsured);
                        Row("Limits of Liability", _model.LimitsOfLiability);
                        Row("Deductible", _model.Deductible);
                        Row("Jurisdictional Limit", _model.JurisdictionalLimit);
                        Row("Geographical Limit", _model.GeographicalLimit);
                        Row("Endorsements", _model.Endorsements);
                    });

                    column.Item().PaddingTop(20).Text("Important Notes")
                        .Bold()
                        .FontSize(14);
                });
            });
        }
    }
}
