using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;
using DealEngine.Services.Interfaces.Enums;
using Microsoft.AspNetCore.Mvc;
using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using SystemDocument = DealEngine.Domain.Entities.Document;

namespace DealEngine.Services.Impl
{
    public class PolicyDocumentService : IPolicyDocumentService
    {
        private readonly IFileService _fileService;
        IUnitOfWork _unitOfWork;
        private readonly ICertificateBuilderService _certificateBuilderService;
        private readonly ICertificatePdfService _certificatePdfService;
        public PolicyDocumentService(
            IFileService fileService,
            IUnitOfWork unitOfWork,
            ICertificateBuilderService certificateBuilderService,
            ICertificatePdfService certificatePdfService)   
        {
            _fileService = fileService;
            _unitOfWork = unitOfWork;  
            _certificateBuilderService = certificateBuilderService; 
            _certificatePdfService = certificatePdfService;
        }

        public async Task<List<SystemDocument>> GetAllPolicyDocumentsForProgrammeAsync(
            ClientProgramme programme,
            User user,
            bool regenerateDocuments = true)
        {
            var allDocuments = new List<SystemDocument>();

            foreach (var agreement in programme.Agreements
                         .Where(a => a.DateDeleted == null))
            {
                var docs = await GetAgreementDocumentsAsync(
                    agreement,
                    programme,
                    user,
                    regenerateDocuments);

                allDocuments.AddRange(docs);
            }

            return allDocuments;
        }

        public async Task<List<SystemDocument>> GetAgreementDocumentsAsync(
            ClientAgreement agreement,
            ClientProgramme programme,
            User user,
            bool regenerateDocuments = true)
        {
            var documents = new List<SystemDocument>();

            if (agreement.Product.IsOptionalCombinedProduct)
                return documents;

            // STEP 1: Add wording docs
            await AddWordingDocumentsAsync(
                agreement,
                documents);

            // STEP 2: Add rendered templates
            await AddRenderedDocumentsAsync(
                agreement,
                programme,
                user,
                documents,
                regenerateDocuments);

            // STEP 3: Add certificates
            await AddCertificateDocumentsAsync(
                agreement,
                programme,
                documents);

            return documents;
        }


        private async Task AddWordingDocumentsAsync(
        ClientAgreement agreement,
        List<SystemDocument> documents)
        {
            var path = agreement.Product.WordingDownloadURL;

            if (string.IsNullOrWhiteSpace(path))
                return;

            if (!System.IO.File.Exists(path))
                return;

            documents.Add(new SystemDocument
            {
                Path = path,
                Name = Path.GetFileName(path),
                ContentType = "application/pdf",
                DocumentType = 0,
                Contents = await System.IO.File.ReadAllBytesAsync(path)
            });

            await Task.CompletedTask;
        }


        private async Task AddRenderedDocumentsAsync(
        ClientAgreement agreement,
        ClientProgramme programme,
        User user,
        List<SystemDocument> documents,
        bool regenerateDocuments)
        {
            var templates = agreement.Product.Documents
                .Where(d =>
                    d.DateDeleted == null &&
                    d.DocumentType != 10 &&
                    d.DocumentType != 7);

            foreach (var template in templates)
            {
                // skip certificate here
                if (template.DocumentType == 0)
                    continue;

                var renderedDoc =
                    await RerenderTemplate(
                        template,
                        agreement,
                        programme,
                        user);

                if (renderedDoc != null)
                {
                    documents.AddRange(renderedDoc);
                }
            }
        }


        private async Task AddCertificateDocumentsAsync(
    ClientAgreement agreement,
    ClientProgramme programme,
    List<SystemDocument> documents)
        {
            var certType = GetCertificateType(agreement.Product.Name);

            if (certType == null)
                return;

            var bytes = await _fileService.GenerateCertificateBytesAsync(
                agreement.Id,
                programme.Id,
                certType);

            documents.Add(new SystemDocument
            {
                Name = $"Certificate-{agreement.Product.Name}.pdf",
                ContentType = "application/pdf",
                DocumentType = 0,
                Contents = bytes
            });
        }




           public async Task<List<SystemDocument>> RerenderTemplate(SystemDocument template, ClientAgreement agreement, ClientProgramme programme,User user)
        {
            Document renderedDoc;
            var documents = new List<SystemDocument>();
            var documentspremiumadvice = new List<SystemDocument>();
            try
            {
                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    List<SystemDocument> agreeDocList = agreement.GetDocuments();
                    foreach (Document doc in agreeDocList.Where(doc => doc.Name == template.Name))
                    {
                        doc.Delete(user);
                    }

                    ClientInformationSheet sheet = agreement.ClientInformationSheet;
                    if (template.ContentType == MediaTypeNames.Application.Pdf)
                    {
                        SystemDocument notRenderedDoc = await _fileService.GetDocumentByID(template.Id);
                        agreement.Documents.Add(notRenderedDoc);
                        documents.Add(notRenderedDoc);
                    }
                    else
                    {
                        //render docs except invoice
                        if (programme.BaseProgramme.IsPdfDoc)
                        {
                                // -------------------------
                                // Determine CertificateType
                                // -------------------------
                                var firstTerm = agreement.ClientAgreementTerms
                                    .FirstOrDefault(t => t.DateDeleted == null);

                                CertificateType certificateType = CertificateType.MD; // default

                                if (firstTerm != null &&
                                    Enum.TryParse(firstTerm.SubTermType, out CertificateType parsedType))
                                {
                                    certificateType = parsedType;
                                }

                                // -------------------------
                                // Generate Certificate PDF
                                // -------------------------
                                renderedDoc = await GenerateCertificate(agreement, programme, certificateType,user);
                                renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                renderedDoc.RenderToPDF = true;
                                agreement.Documents.Add(renderedDoc);
                                documents.Add(renderedDoc);
                                await _fileService.UploadFile(renderedDoc);
                        }
                          

                    }
                        
                        //render job certificate
                        if (template.DocumentType == 9 && !programme.BaseProgramme.IsPdfDoc)
                        {
                            if (sheet.Jobs.Where(sj => sj.DateDeleted == null && !sj.Removed).Count() > 0)
                            {
                                foreach (var job in sheet.Jobs.Where(sj => sj.DateDeleted == null && !sj.Removed))
                                {
                                    renderedDoc = await _fileService.RenderDocument(user, template, agreement, null, job);
                                    renderedDoc.OwnerOrganisation = agreement.ClientInformationSheet.Owner;
                                    agreement.Documents.Add(renderedDoc);
                                    documents.Add(renderedDoc);
                                    await _fileService.UploadFile(renderedDoc);
                                }
                            }
                        }

                   uow.Commit();
                }
            }
            catch (Exception ex)
            {
                return documents;
            }
            return documents;

           }

        public async Task<SystemDocument> GenerateCertificate(ClientAgreement agreement, ClientProgramme programme, 
                                                              CertificateType type, User user)

        {
            var model = await _certificateBuilderService.BuildAsync(agreement, programme, type);
            model.CertificateType = type;
            // 2️⃣ Generate PDF bytes via QuestPDF
            var pdfBytes = await _certificatePdfService.GenerateAsync(model);

            // 3️⃣ Create SystemDocument
            var document = new SystemDocument(
                user,
                model.CertificateTitle,
                "application/pdf",
                8 // your DocumentType
            );

            document.Contents = pdfBytes;
            document.OwnerOrganisation = agreement.ClientInformationSheet.Owner;



            return document;
        }

        private CertificateType? GetCertificateType(string productName)
        {
            if (string.IsNullOrWhiteSpace(productName))
                throw new Exception("Product name is missing");

            productName = productName.Trim();

            if (productName.Contains("Material Damage", StringComparison.OrdinalIgnoreCase))
                return CertificateType.MD;

            if (productName.Contains("Global Guard", StringComparison.OrdinalIgnoreCase))
                return CertificateType.MLGGL;

            if (productName.Contains("Management Liability", StringComparison.OrdinalIgnoreCase))
                return CertificateType.AS;

            // Product does not require certificate
            return null;
        }





    }
}