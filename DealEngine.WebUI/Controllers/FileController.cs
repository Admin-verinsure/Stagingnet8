using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using SystemDocument = DealEngine.Domain.Entities.Document;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using DealEngine.WebUI.Models;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using HtmlToOpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using ServiceStack;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Storage;
using System.Net;
//using FastReport.Export.PdfSimple.PdfObjects;
using NReco.PdfGenerator;

namespace DealEngine.WebUI.Controllers
{
    [Authorize]
    public class FileController : BaseController
    {
        IUnitOfWork _unitOfWork;
        IFileService _fileService;
        IProgrammeService _programmeService;
        IMapperSession<SystemDocument> _documentRepository;
        IMapperSession<Image> _imageRepository;
        IMapperSession<Product> _productRepository;
        IApplicationLoggingService _applicationLoggingService;
        ILogger<FileController> _logger;
        IAppSettingService _appSettingService;
        //      string _appData = "~/App_Data/";
        //string _uploadFolder = "uploads";

        public FileController(
            ILogger<FileController> logger,
            IProgrammeService programmeService,
            IApplicationLoggingService applicationLoggingService,
            IUserService userRepository,
            IUnitOfWork unitOfWork,
            IFileService fileService,
            IMapperSession<SystemDocument> documentRepository,
            IMapperSession<Image> imageRepository,
            IMapperSession<Product> productRepository,
            IAppSettingService appSettingService
            )
            : base(userRepository)
        {
            _programmeService = programmeService;
            _logger = logger;
            _applicationLoggingService = applicationLoggingService;
            _unitOfWork = unitOfWork;
            _fileService = fileService;
            _documentRepository = documentRepository;
            _imageRepository = imageRepository;
            _productRepository = productRepository;
            _appSettingService = appSettingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetInvoicePDF(Guid Id, Guid ClientProgrammeId, string invoicename)
        {
            ClientProgramme clientprogramme = await _programmeService.GetClientProgrammebyId(ClientProgrammeId);
            ClientInformationSheet clientInformationSheet = clientprogramme.InformationSheet;
            SystemDocument doc = await _documentRepository.GetByIdAsync(Id);

            // DOCX & HTML
            string html = _fileService.FromBytes(doc.Contents);

            if (doc.DocumentType == 8) // Apollo Invoice
            {
                html = html.Insert(0, "<head><meta http-equiv=\"content - type\" content=\"text / html; charset = utf - 8\" /></head>"); // Removed to fix Image 
            }
            else
            {
                html = html.Insert(0, "<head><meta http-equiv=\"content - type\" content=\"text / html; charset = utf - 8\" /><style>img { height:auto; max-width: 300px }</style></head>"); // Ashu old values -> width: 120px; height:120px
            }
            // Test if the below 4 are even necessary by this function, setting above should make these redundant now
            html = html.Replace("“", "&quot");
            html = html.Replace("”", "&quot");
            html = html.Replace(" – ", "--");
            html = html.Replace("&nbsp;", " ");
            html = html.Replace("’", "&#146");
            html = html.Replace("‘", "&#39");
            html = html.Replace("ä" , "&#228;");
            html = html.Replace("ë", "&#235;");
            html = html.Replace("ö", "&#246;");
            html = html.Replace("ü", "&#252;");


            var htmlToPdfConv = new NReco.PdfGenerator.HtmlToPdfConverter();
            htmlToPdfConv.License.SetLicenseKey(
               _appSettingService.NRecoUserName,
               _appSettingService.NRecoLicense
           );            // for Linux/OS-X: "wkhtmltopdf"
            if (_appSettingService.IsLinuxEnv == "True")
            {
                htmlToPdfConv.WkHtmlToPdfExeName = "wkhtmltopdf";
            }
            htmlToPdfConv.PdfToolPath = _appSettingService.NRecoPdfToolPath;
            var margins = new PageMargins();
            margins.Bottom = 10;
            margins.Top = 10;
            margins.Left = 30;
            margins.Right = 10;
            htmlToPdfConv.Margins = margins;

            htmlToPdfConv.PageFooterHtml = "</br>" + $@"page <span class=""page""></span> of <span class=""topage""></span>";

            // Legacy Image Path Fix
            string badURL = "../../../images/";
            var newURL = "https://" + _appSettingService.domainQueryString + "/Image/";
            html = html.Replace(badURL, newURL);

            var pdfBytes = htmlToPdfConv.GeneratePdf(html);
            return File(pdfBytes, "application/pdf", invoicename + ".pdf");

        }

        [HttpGet]
        public async Task<IActionResult> GetPDF(Guid Id, Guid ClientProgrammeId)
        {
            ClientProgramme clientprogramme = await _programmeService.GetClientProgrammebyId(ClientProgrammeId);
            ClientInformationSheet clientInformationSheet = clientprogramme.InformationSheet;
            SystemDocument doc = await _documentRepository.GetByIdAsync(Id);
            var docContents = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
            // DOCX & HTML
            string html = _fileService.FromBytes(doc.Contents);
            html = html.Insert(0, "<head><meta http-equiv=\"content - type\" content=\"text / html; charset = utf - 8\" /><style>img { width: 120px; height:120px}</style></head>");

            var htmlToPdfConv = new HtmlToPdfConverter();
            htmlToPdfConv.License.SetLicenseKey(
               _appSettingService.NRecoUserName,
               _appSettingService.NRecoLicense
           );            // for Linux/OS-X: "wkhtmltopdf"
            if (_appSettingService.IsLinuxEnv == "True")
            {
                htmlToPdfConv.WkHtmlToPdfExeName = "wkhtmltopdf";
            }
            htmlToPdfConv.PdfToolPath = _appSettingService.NRecoPdfToolPath;

            string submittedBy = clientprogramme.InformationSheet.SubmittedBy.FullName;
            if (clientprogramme.InformationSheet.SubmittedBy.PrimaryOrganisation.Name == "TechCertain Ltd.")
            {
                submittedBy = clientprogramme.InformationSheet.Programme.BrokerContactUser.FullName;
            }

            //htmlToPdfConv.PageHeaderHtml = "<p style='padding-top: 60px'>"
            //    + "</br><strong> Title: " + clientprogramme.BaseProgramme.Name + "</strong></br>"
            //    + " <strong> Information Sheet for: " + clientprogramme.Owner.Name + "</strong></br>"
            //    + " <strong> UIS No: " + clientInformationSheet.ReferenceId + "</strong></br>"
            //    + " <strong> Sheet Submitted On: " + clientInformationSheet.SubmitDate.ToShortDateString() + "</strong></br>"
            //    + " <strong> Report Generated On: " + DateTime.Now.ToShortDateString() + "</strong></br>"
            //    + " <strong> Submitted By: " + submittedBy + "</strong></br>"
            //    + "<h2> </br>  </h2> </p>";

            htmlToPdfConv.PageFooterHtml = "</br>" + $@"page <span class=""page""></span> of <span class=""topage""></span>";

            var margins = new PageMargins();
            margins.Bottom = 18;
            margins.Top = 38;
            margins.Left = 15;
            margins.Right = 15;
            htmlToPdfConv.Margins = margins;

            // Legacy Image Path Fix
            string badURL = "../../../images/";
            var newURL = "https://" + _appSettingService.domainQueryString + "/Image/";
            html = html.Replace(badURL, newURL);

            var pdfBytes = htmlToPdfConv.GeneratePdf(html);

            return File(pdfBytes, "application/pdf", "InformationSheetReport.pdf");

        }

        [HttpPost]
        public async Task<IActionResult> covertdoctohtml(string TemplateName, string ActualFileName, string DocumentType)
        {
            // Ashu's function to get local file stored in Web/Template/"templatename".html and create a Document in the database for it
            string htmlbody = string.Empty;
            var path = "./Template/" + TemplateName + ".html";
            using (StreamReader reader = new StreamReader("./Template/" + TemplateName + ".html"))
            {
                htmlbody = reader.ReadToEnd();
            }
            User user = await CurrentUser();
            SystemDocument document = null;
            Product product = null;
            document = new SystemDocument(user, ActualFileName, MediaTypeNames.Text.Html, int.Parse(DocumentType));
            document.Description = TemplateName + ".pdf";
            document.Contents = _fileService.ToBytes(htmlbody);
            document.IsTemplate = true;
            await _documentRepository.AddAsync(document);

            return Json("OK");
        }

        [HttpGet]
        public async Task<IActionResult> GetDocument(Guid id, string format)
        {
            User user = null;
            try
            {
                SystemDocument doc = await _documentRepository.GetByIdAsync(id);
                string extension = "";
                var docContents = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
                // DOCX & HTML
                if (doc.ContentType == MediaTypeNames.Text.Html)
                {
                    extension = ".html";
                    string html = _fileService.FromBytes(doc.Contents);
                    string html2 = html;

                    // Bugfix for images added before Image Path fix
                    string badURL = "../../../images/";
                    var newURL = "https://" + _appSettingService.domainQueryString + "/Image/";
                    html2 = html2.Replace(badURL, newURL);

                    // Image resize/positioning
                    string centerImage = "<figure class=\"image\"><img src=\"";
                    string centerResize = "<figure class=\"image image_resized\" style=";
                    string leftImage = "<figure class=\"image image-style-align-left\"><img src=\"";
                    string leftResize = "<figure class=\"image image-style-align-left image_resized\" style=";
                    string leftResize2 = "<figure class=\"image image_resized image-style-align-left\" style=";
                    string rightImage = "<figure class=\"image image-style-align-right\"><img src=\"";
                    string rightResize = "<figure class=\"image image-style-align-right image_resized\" style=";
                    string rightResize2 = "<figure class=\"image image_resized image-style-align-right\" style=";

                    // Border
                    string showBorder = "<figure class=\"table\"><table style=\"border-bottom:solid;border-left:solid;border-right:solid;border-top:solid;\"><tbody><tr>";
                    string noBorder = "<figure class=\"table\"><table><tbody><tr>";

                    // array of elements that need updated
                    string[] badHtml = { centerResize, leftResize, rightResize, leftResize2, rightResize2, centerImage, leftImage, rightImage };

                    // Reason you check the Content Type and then the format is for when you want a docx from html
                    // HTML
                    if (!(format == "docx") & !(format == "pdf"))
                    {
                        foreach (string ele in badHtml)
                        {
                            int x = CountStringOccurrences(html2, ele);
                            var regex = new Regex(Regex.Escape(ele));

                            for (int j = 0; j < x; j++)
                            {
                                if (ele.Contains("image_resized") == false)
                                {
                                    // Image to HTML use cases
                                    if (ele.Equals(centerImage))
                                    {
                                        html2 = regex.Replace(html2, "<img style=\"display:block;margin-left:auto;margin-right:auto;\" src=\"", 1);
                                    }
                                    else if (ele.Equals(leftImage))
                                    {
                                        html2 = regex.Replace(html2, "<img style=\"display:block;margin-left:0;margin-right:auto;\" src=\"", 1);
                                    }
                                    else if (ele.Equals(rightImage))
                                    {
                                        html2 = regex.Replace(html2, "<img style=\"display:block;margin-left:auto;margin-right:0;\" src=\"", 1);
                                    }
                                }
                                else
                                {
                                    int widthIndex = ele.Length;                                                            // length of figure tag up until style=
                                    string width = html2.Substring(html2.IndexOf(ele) + widthIndex + 7, 5);                 // the actual width value in % plus extra characters sometimes which we delete below                       
                                    width = width.Replace("%", "");
                                    width = width.Replace("\"", "");
                                    width = width.Replace(";", "");
                                    width = width.Replace(">", "");

                                    int srcEndIndex = html2.IndexOf(ele) + widthIndex;
                                    int end = 21 + width.Length;
                                    html2 = html2.Remove(srcEndIndex, end);                                                 // remove the extra src and style tags

                                    if (ele.Equals(centerResize) == true)
                                    {
                                        html2 = regex.Replace(html2, "<img width=\"" + width + "%\"; style=\"display:block;margin-left:auto;margin-right:auto;\" src=\"", 1);
                                    }
                                    else if ((ele.Equals(leftResize) == true) || (ele.Equals(leftResize2) == true))
                                    {
                                        html2 = regex.Replace(html2, "<img width=\"" + width + "%\"; style=\"display:block;margin-left:0;margin-right:auto;\" src=\"", 1);
                                    }
                                    else if ((ele.Equals(rightResize) == true) || (ele.Equals(rightResize2) == true))
                                    {
                                        html2 = regex.Replace(html2, "<img width=\"" + width + "%\"; style=\"display:block;margin-left:auto;margin-right:0;\" src=\"", 1);
                                    }
                                }
                            }
                            html = html2;
                        }

                        #region old code
                        // Resized Image to HTML use cases                  
                        /*
                        if (html.Contains(centerResize) & !(format == "docx"))
                        {
                            // TODO Make work for multiple resized centered images in the html...

                            int widthIndex = centerResize.Length;                                               // length of figure tag up until style=
                            string width = html.Substring(html.IndexOf(centerResize) + widthIndex + 7, 5);      // the actual width value in %                       
                            width = width.Replace("%", "");                                                     // Handle when % is in the string (ie. <10%, 9.99% etc)
                            int srcEndIndex = html.IndexOf(centerResize) + widthIndex;                          // where src ends in original tag
                            html = html.Remove(srcEndIndex, 26);                                                // remove the extra src and style tags

                            // replace with working html
                            html = html.Replace(centerResize, "<img width=\"" + width + "%\"; style=\"display:block;margin-left:auto;margin-right:auto;\" src=\"");
                        }
                        if (html.Contains(rightResize) & !(format == "docx"))
                        {
                            // TODO Make work for multiple resized centered images in the html...

                            int widthIndex = rightResize.Length;                                               // length of figure tag up until style=
                            string width = html.Substring(html.IndexOf(rightResize) + widthIndex + 7, 5);      // the actual width value in %                       
                            width = width.Replace("%", "");                                                     // Handle when % is in the string (ie. <10%, 9.99% etc)
                            int srcEndIndex = html.IndexOf(rightResize) + widthIndex;                          // where src ends in original tag
                            html = html.Remove(srcEndIndex, 26);                                               // remove the extra src and style tags

                            // replace with working html
                            html = html.Replace(rightResize, "<img width=\"" + width + "%\"; style=\"display:block;margin-left:auto;margin-right:0;\" src=\"");
                        }
                        if (html.Contains(leftResize) & !(format == "docx"))
                        {
                            // TODO Make work for multiple resized centered images in the html...

                            int widthIndex = leftResize.Length;                                               // length of figure tag up until style=
                            string width = html.Substring(html.IndexOf(leftResize) + widthIndex + 7, 5);      // the actual width value in %                       
                            width = width.Replace("%", "");                                                   // Handle when % is in the string (ie. <10%, 9.99% etc)
                            int srcEndIndex = html.IndexOf(leftResize) + widthIndex;                          // where src ends in original tag
                            html = html.Remove(srcEndIndex, 26);                                              // remove the extra src and style tags

                            // replace with working html
                            html = html.Replace(leftResize, "<img width=\"" + width + "%\"; style=\"display:block;margin-left:0;margin-right:auto;\" src=\"");
                        }
                        */
                        #endregion

                        // Put document contents back in for HTML output (see return)
                        docContents = _fileService.ToBytes(html);
                    }
                    // DOCX
                    else if (format == "docx")
                    {
                        doc = await _fileService.FormatCKHTMLforConversion(doc);
                        html = _fileService.FromBytes(doc.Contents);

                        using (MemoryStream virtualFile = new MemoryStream())
                        {
                            using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(virtualFile, WordprocessingDocumentType.Document))
                            {
                                MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();
                                new DocumentFormat.OpenXml.Wordprocessing.Document(new Body()).Save(mainPart);
                                HtmlConverter converter = new HtmlConverter(mainPart); // refer to this: https://github.com/onizet/html2openxml/wiki/Tags-Supported
                                converter.ImageProcessing = ImageProcessing.ManualProvisioning;
                                Body body = mainPart.Document.Body;
                                converter.ParseHtml(html);

                                #region CSStesting code
                                // Need to figure out how to add classes to style the document... (adding to the top of HTML document doesn't work, also lots of the table styling css doesn't actually work. Just the old way works where style isn't specified e.g <table width=\"100%\" border=\"0\"><tr style=\"font-weight: bold\"><td>Studio</td><td colspan=\"2\")
                                // converter.HtmlStyles.DefaultStyle = converter.HtmlStyles.GetStyle("testClass");
                                // converter.RefreshStyles();
                                #endregion 
                            }
                            // RETURN DOCX
                            return File(virtualFile.ToArray(), MediaTypeNames.Application.Octet, doc.Name + ".docx");
                        }
                    }
                    else if (format == "pdf")
                    {
                        // This is for ManageDocuments where we haven't hit ProcessRequestConfiguration which Formats and Converts the document
                        if (doc.IsTemplate == true)
                        {
                            doc = await _fileService.FormatCKHTMLforConversion(doc);
                            doc = await _fileService.ConvertHTMLToPDF(doc);
                        }
                        return File(doc.Contents, "application/pdf", doc.Name + ".pdf");
                    }
                }
                // PDF - When is this hit?
                else if (doc.ContentType == MediaTypeNames.Application.Pdf)
                {
                    return PhysicalFile(doc.Path, doc.ContentType, doc.Name);
                }

                return File(docContents, doc.ContentType, doc.Name + extension);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ProductIndex(string Programme)
        {
            var productList = new List<Product>();
            var programme = await _programmeService.GetProgramme(Guid.Parse(Programme));
            productList = programme.Products.ToList();

            return View(productList);
        }


        [HttpPost]
        public async Task<IActionResult> SavePDFFile(String Reportstr, Guid ClientProgrammeId)
        {
            // Is actually saving HTML not PDF.. Bad function name..
            SystemDocument document = null;

            User user = null;
            Product product = null;
            try
            {
                ClientProgramme clientProgramme = await _programmeService.GetClientProgrammebyId(ClientProgrammeId);
                user = await CurrentUser();
                document = new SystemDocument(user, "Information Sheet Report", MediaTypeNames.Text.Html, 99);
                document.Description = "FullProposal Report Pdf";
                document.Contents = _fileService.ToBytes(System.Net.WebUtility.HtmlDecode(Reportstr));
                document.OwnerOrganisation = user.PrimaryOrganisation;
                //document.IsTemplate = true;
                await _documentRepository.AddAsync(document);

                using (var uow = _unitOfWork.BeginUnitOfWork())
                {
                    if (clientProgramme != null)
                    {
                        foreach (ClientAgreement agreement in clientProgramme.Agreements)
                        {
                            if (agreement.Product.IsMasterProduct)
                            {
                                foreach (var doc in agreement.Documents)
                                {
                                    if (doc.Description.EqualsIgnoreCase("FullProposal Report Pdf"))
                                    {
                                        agreement.Documents.Remove(doc);
                                        break;
                                    }
                                }
                                if (document.Description.EqualsIgnoreCase("FullProposal Report Pdf") && clientProgramme.BaseProgramme.EnableFullProposalReport)
                                {
                                    agreement.Documents.Add(document);
                                    agreement.IsPDFgenerated = true;
                                }
                            }
                        }
                        await uow.Commit();
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Json(document.Id);
        }


        [HttpPost]
        public async Task<IActionResult> SaveDocumentHtml(DocumentViewModel model)
        {
            User user = null;
            SystemDocument document = null;
            Product product = null;
            try
            {
                user = await CurrentUser();
                if (model.DocumentId != Guid.Empty)
                {
                    document = await _documentRepository.GetByIdAsync(model.DocumentId);
                    if (document != null)
                    {
                        document.DateDeleted = DateTime.Now;
                        await _documentRepository.AddAsync(document);
                    }

                }

                document = new SystemDocument(user, model.Name, MediaTypeNames.Text.Html, model.DocumentType);
                document.Description = model.Description;
                document.Contents = _fileService.ToBytes(System.Net.WebUtility.HtmlDecode(model.Content));
                document.Name = document.Name;
                document.OwnerOrganisation = user.PrimaryOrganisation;
                document.IsTemplate = true;
                await _documentRepository.AddAsync(document);
                //if (model.ProductId != null)
                //{
                //    product = await _productRepository.GetByIdAsync(Guid.Parse(model.ProductId));
                //    product.Documents.Add(document);
                //    await _productRepository.AddAsync(product);
                //}

                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpGet]
        public async Task<IActionResult> CreateDocument(string id, string productId)
        {
            DocumentViewModel model = new DocumentViewModel();
            User user = null;
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return View(model);

                Guid documentId = Guid.Empty;
                if (!Guid.TryParse(id, out documentId))
                    throw new Exception(id + " is not a valid document Id");

                if (documentId == Guid.Empty)
                    return View(model);

                SystemDocument document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                    throw new Exception("Unable to update document: Could not find document with id " + id);

                model.DocumentId = document.Id;
                model.Name = document.Name;
                model.Description = document.Description;
                model.DocumentType = document.DocumentType;
                model.Content = _fileService.FromBytes(document.Contents);
                model.ProductId = productId;
                //model.Products = _productRepository.FindAll().ToList();

                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }


        [HttpPost]
        public async Task<IActionResult> CreateDocument(DocumentViewModel model)
        {
            User user = null;
            SystemDocument document = null;
            Product product = null;
            try
            {
                user = await CurrentUser();
                if (model.DocumentId != Guid.Empty)
                {
                    document = await _documentRepository.GetByIdAsync(model.DocumentId);
                    if (document != null)
                    {
                        document.DateDeleted = DateTime.Now;
                        await _documentRepository.AddAsync(document);
                    }

                }

                document = new SystemDocument(user, model.Name, MediaTypeNames.Text.Html, model.DocumentType);
                document.Description = model.Description;
                document.Contents = _fileService.ToBytes(System.Net.WebUtility.HtmlDecode(model.Content));
                document.OwnerOrganisation = user.PrimaryOrganisation;
                document.IsTemplate = true;
                document.RenderToPDF = model.RenderToPDF;
                await _documentRepository.AddAsync(document);
                //if (model.ProductId != null)
                //{
                //    product = await _productRepository.GetByIdAsync(Guid.Parse(model.ProductId));
                //    product.Documents.Add(document);
                //    await _productRepository.AddAsync(product);
                //}

                return View(model);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ManageDocuments()
        {
            BaseListViewModel<DocumentInfoViewModel> models = new BaseListViewModel<DocumentInfoViewModel>();
            User user = null;
            try
            {
                user = await CurrentUser();
                List<SystemDocument> docs = _documentRepository.FindAll().Where(d => d.DateDeleted == null && user.PrimaryOrganisation == d.OwnerOrganisation && d.IsTemplate).ToList();

                if (user.PrimaryOrganisation.IsBroker || user.PrimaryOrganisation.IsTC || user.PrimaryOrganisation.IsInsurer)
                {
                    //docs = _documentRepository.FindAll().Where(d => !d.DateDeleted.HasValue && d.IsTemplate);
                    //if(productId != null)
                    //{
                    //    var products = await _productRepository.GetByIdAsync(Guid.Parse(productId));
                    //    docs = products.Documents.ToList();
                    //}

                }

                if (docs.Count != 0)
                {
                    foreach (SystemDocument doc in docs)
                    {
                        string documentType = "";
                        switch (doc.DocumentType)
                        {
                            case 0:
                                {
                                    documentType = "Wording";
                                    break;
                                }
                            case 1:
                                {
                                    documentType = "Certificate";
                                    break;
                                }
                            case 2:
                                {
                                    documentType = "Schedule";
                                    break;
                                }
                            case 3:
                                {
                                    documentType = "Payment Confirmation";
                                    break;
                                }
                            case 4:
                                {
                                    documentType = "EGlobal Credit Card Invoice";
                                    break;
                                }
                            case 5:
                                {
                                    documentType = "Advisory";
                                    break;
                                }
                            case 6:
                                {
                                    documentType = "Sub-Certificate";
                                    break;
                                }
                            case 7:
                                {
                                    documentType = "Premium Advice";
                                    break;
                                }
                            case 9:
                                {
                                    documentType = "Job-Certificate";
                                    break;
                                }
                            case 10:
                                {
                                    documentType = "UIS Invitation Email Attachment";
                                    break;
                                }
                            case 11:
                                {
                                    documentType = "Named Party COC";
                                    break;
                                }
                            case 12:
                                {
                                    documentType = "EGlobal Invoice";
                                    break;
                                }
                            case 99:
                                {
                                    documentType = "Full Proposal Report";
                                    break;
                                }
                            default:
                                {
                                    throw new Exception(string.Format("Can not get Document Type for document", doc.Id));
                                }
                        }
                        //var product = _productRepository.FindAll().Where(prod => !prod.DateDeleted.HasValue && prod.Documents.Contains(doc)).First();
                        models.Add(new DocumentInfoViewModel
                        {
                            DisplayName = doc.Name,
                            //ProductId = productId,
                            Type = documentType,
                            Owner = doc.OwnerOrganisation.Name,
                            Id = doc.Id,
                        });

                        ViewBag.IsTC = user.PrimaryOrganisation.IsTC;
                    }
                }
                else
                {
                    return RedirectToAction("CreateDocument");
                }

                return View(models);
            }
            catch (Exception ex)
            {
                await _applicationLoggingService.LogWarning(_logger, ex, user, HttpContext);
                return RedirectToAction("Error500", "Error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Render(string id)
        {
            throw new Exception("Method will need to be re-written");
            //string serverFile = Path.Combine(_appData, _uploadFolder, id);
            //string filepath = Server.MapPath(serverFile);
            //if (System.IO.File.Exists(filepath))
            //    System.IO.File.Delete(filepath);

            //// Create a document by supplying the filepath. 

            //using (WordprocessingDocument wordDocument = WordprocessingDocument.Create(filepath, WordprocessingDocumentType.Document))
            //{
            //    // Add a main document part. 
            //    MainDocumentPart mainPart = wordDocument.AddMainDocumentPart();

            //    // Create the document structure and add some text.
            //    mainPart.Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
            //    Body body = mainPart.Document.AppendChild(new Body());
            //    Paragraph para = body.AppendChild(new Paragraph());
            //    Run run = para.AppendChild(new Run());
            //    run.AppendChild(new Text("Create text in body - CreateWordprocessingDocument. This is a test."));
            //    run.AppendChild(new Text("Second line?"));
            //    Run run2 = para.AppendChild(new Run());
            //    run2.AppendChild(new Text("Second run"));
            //    Paragraph par2 = body.AppendChild(new Paragraph());
            //    Run run3 = par2.AppendChild(new Run());
            //    run3.AppendChild(new Text("Second paragaph"));
            //}
            //return null;
        }

        // helper
        public static int CountStringOccurrences(string text, string pattern)
        {
            // Loop through all instances of the string 'text'.
            int count = 0;
            int i = 0;
            while ((i = text.IndexOf(pattern, i)) != -1)
            {
                i += pattern.Length;
                count++;
            }
            return count;
        }
    }
}

