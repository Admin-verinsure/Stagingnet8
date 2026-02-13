using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Services.Interfaces;
using DealEngine.Services.Interfaces.Models;
using DealEngine.Services.Impl.Documents;
using QuestPDF.Fluent;

namespace DealEngine.Services.Impl
{
    public class CertificatePdfService : ICertificatePdfService
    {
        public Task<byte[]> GenerateAsync(CertificateAggregateModel model)
        {
            var document = new CertificateDocument(model);
            var pdfBytes = document.GeneratePdf();
            return Task.FromResult(pdfBytes);
        }
    }

}
