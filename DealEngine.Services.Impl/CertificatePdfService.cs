using DealEngine.Services.Impl.Documents;
using DealEngine.Services.Interfaces;
using DealEngine.Services.Interfaces.Models;
using Microsoft.AspNetCore.Hosting;
using QuestPDF.Fluent;
using ServiceStack.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealEngine.Services.Impl
{
    public class CertificatePdfService : ICertificatePdfService
    {
        private readonly IWebHostEnvironment _env;
        public CertificatePdfService( IWebHostEnvironment env)
        {
            _env = env;
        }

        public Task<byte[]> GenerateAsync(CertificateAggregateModel model)
        {
            var document = new CertificateDocument(model ,  _env);
            var pdfBytes = document.GeneratePdf();
            return Task.FromResult(pdfBytes);
        }
    }

}
