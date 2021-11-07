using System;
using System.Linq;
using System.Net.Mime;
using System.IO;
using DealEngine.Services.Interfaces;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Domain.Entities;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using NHibernate.Linq;
using SystemDocument = DealEngine.Domain.Entities.Document;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using HtmlToOpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Extensions.Logging;
using Document = DealEngine.Domain.Entities.Document;
using System.Text.RegularExpressions;
using NReco.PdfGenerator;

namespace DealEngine.Services.Impl
{
	public class FileService : IFileService
	{
		const uint _jpegMagicNumber = 0xD8FF;
		const uint _jfifAPP0Marker = 0xE0FF;
		const uint _exifAPP1Marker = 0xE1FF;
		const uint _jfifMarker = 0x4649464A;
		const uint _exifMarker = 0x66697845;

		const uint _gifMagicNumber = 0x38464947;//0x47494638;
		const ulong _pngMagicNumber = 0x0A1A0A0D474E5089;//0x89504E470D0A1A0A;
		const uint _tiffMagicNumberIntel = 0x002A4949;
		const uint _tiffMagicNumberMotorola = 0x2A004D4D;

		IMapperSession<Image> _imageRepository;
		IMapperSession<Document> _documentRepository;
		IClientAgreementMVTermService _clientAgreementMVTermService;
        IClientAgreementBVTermService _clientAgreementBVTermService;
        IProgrammeService _programmeService;
        IProductService _productService;
        IAppSettingService _appSettingService;



        public FileService(IMapperSession<Image> imageRepository, IMapperSession<Document> documentRepository,
        IProgrammeService programmeService, IClientAgreementMVTermService clientAgreementMVTermService, IClientAgreementBVTermService clientAgreementBVTermService, IProductService productService, IAppSettingService appSettingService)
		{
			_imageRepository = imageRepository;
			_documentRepository = documentRepository;
			_clientAgreementMVTermService = clientAgreementMVTermService;
            _clientAgreementBVTermService = clientAgreementBVTermService;
            _programmeService = programmeService;
            _productService = productService;
            _appSettingService = appSettingService;

            FileDirectory = Path.Combine (
				Directory.GetCurrentDirectory (),
				"App_Data",
				"uploads"
			);
		}

        #region IFileService implementation

        public static bool IsLinux
        {
            get
            {
                int p = (int)Environment.OSVersion.Platform;
                return (p == 4) || (p == 6) || (p == 128);
            }
        }

        public string UserTimeZone
        {
            get { return IsLinux ? "NZ" : "New Zealand Standard Time"; } //Pacific/Auckland
        }

        public string FileDirectory { get; protected set; }

		public bool IsApplication(byte [] buffer, string contentType, string fileName)
		{
			throw new NotImplementedException ();
		}

		public bool IsImageFile(byte [] buffer, string contentType, string fileName)
		{
			// references
            //can we make this a task async -- potentially screwing up the system if its not converted..
			// https://en.wikipedia.org/wiki/Magic_number_%28programming%29
			// http://stackoverflow.com/a/8755028

			string extension = Path.GetExtension (fileName);
			BinaryReader br = new BinaryReader (new MemoryStream (buffer));

			if ((extension == ".jpg" || extension == ".jpeg") && contentType == MediaTypeNames.Image.Jpeg) {
				bool isJpeg = br.ReadUInt16 () == _jpegMagicNumber;
				uint appMarker = br.ReadUInt16 ();
				br.ReadUInt16 ();   // need to skip ahead 2 bytes
				uint format = br.ReadUInt32 ();
				return isJpeg && (
					(appMarker == _jfifAPP0Marker && format == _jfifMarker) ||
					(appMarker == _exifAPP1Marker && format == _exifMarker)
				);
			}
			if (extension == ".gif" && contentType == MediaTypeNames.Image.Gif)
				return br.ReadUInt32 () == _gifMagicNumber;
			if (extension == ".tiff" && contentType == MediaTypeNames.Image.Tiff) {
				uint magicNumber = br.ReadUInt32 ();
				return magicNumber == _tiffMagicNumberIntel || magicNumber == _tiffMagicNumberMotorola;
			}
			if (extension == ".png" && contentType == "image/png")
				return br.ReadUInt64 () == _pngMagicNumber;
			return false;
		}

		public bool IsTextFile(byte [] buffer, string contentType, string fileName)
		{
			// references
			// http://stackoverflow.com/a/14587821

			string extension = Path.GetExtension (fileName);
			if (extension == ".html" && contentType == MediaTypeNames.Text.Html)
				return true;
			if (extension == ".txt" && contentType == MediaTypeNames.Text.Plain)
				return true;
			if (extension == ".rtf" && contentType == MediaTypeNames.Text.RichText)
				return true;
			if (extension == ".xml" && contentType == MediaTypeNames.Text.Xml)
				return true;
			return false;
		}

		public async Task UploadFile(Document document)
		{
            await _documentRepository.AddAsync(document);
		}

        public async Task UploadFile(Image image)
		{
		    _imageRepository.AddAsync(image);
		}

		public async Task<Document> GetDocument(string documentName)
		{
			return await _documentRepository.FindAll().FirstOrDefaultAsync(i => i.Name == documentName);
        }

        public async Task<Document> GetDocumentByID(Guid documentID)
        {
            return await _documentRepository.FindAll().FirstOrDefaultAsync(i => i.Id == documentID);
        }

        public async Task<Document> GetDocumentByType(Organisation primaryOrganisation, int DocumentType)
        {
            Document document = await _documentRepository.FindAll().FirstOrDefaultAsync(i => i.OwnerOrganisation == primaryOrganisation && i.DocumentType == DocumentType);
            return document;
        }

        public async Task<Image> GetImage(string imageName)
		{
			return await _imageRepository.FindAll().FirstOrDefaultAsync(i => i.Name == imageName);
		}

        public async Task<T> RenderDocument<T>(User renderedBy, T template, ClientAgreement agreement, ClientInformationSheet clientInformationSheet, Job job) where T : Document
        {
			Document doc = new Document (renderedBy, template.Name, template.ContentType, template.DocumentType);

            // This is for Locally Saved PDF Wording Documents which don't need to be rendered.
            if (template.Path != null && template.ContentType == "application/pdf" && template.DocumentType == 0)
            {
                return (T)template;
            }
            // store all the fields to be merged

            if (clientInformationSheet != null)
            {
                List<KeyValuePair<string, string>> mergeFields = new List<KeyValuePair<string, string>>();
                mergeFields.Add(new KeyValuePair<string, string>("[[ClientName]]", clientInformationSheet.Owner.Name));
                mergeFields.Add(new KeyValuePair<string, string>("[[Date]]", DateTime.UtcNow.ToShortDateString()));
                mergeFields.Add(new KeyValuePair<string, string>("[[BrokerName]]", clientInformationSheet.Programme.BaseProgramme.BrokerContactUser.FirstName +
                    clientInformationSheet.Programme.BaseProgramme.BrokerContactUser.LastName));

                // merge the configured merge feilds into the document
                string content = FromBytes(template.Contents);
                try
                {
                    foreach (KeyValuePair<string, string> field in mergeFields)
                        if (field.Value != null && field.Value.Contains("&"))
                        {
                            content = content.Replace(field.Key, field.Value.Replace("&", "&amp;"));
                        }
                        else
                        {
                            content = content.Replace(field.Key, field.Value);
                        }

                }
                catch (Exception ex)
                {

                }
                // save the merged content
                doc.Contents = ToBytes(content);

            } else if (agreement != null)
            {
                List<KeyValuePair<string, string>> mergeFields = GetMergeFields(agreement, clientInformationSheet);

                NumberFormatInfo currencyFormat = new CultureInfo(CultureInfo.CurrentCulture.ToString()).NumberFormat;
                currencyFormat.CurrencyNegativePattern = 2;
                Decimal PremiumTotal = 0.0m;

                int intMonthlyInstalmentNumber = 1;
                if (agreement.ClientInformationSheet.Programme.BaseProgramme.EnableMonthlyPremiumDisplay)
                {
                    intMonthlyInstalmentNumber = agreement.ClientInformationSheet.Programme.BaseProgramme.MonthlyInstalmentNumber;
                }

                //set default merge feilds
                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[PolicyStatus_PI]]", ""), "Not Insured"));
                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[PolicyStatus_PL]]", ""), "Not Insured"));
                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[PolicyStatus_SL]]", ""), "Not Insured"));
                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[PolicyStatus_DO]]", ""), "Not Insured"));
                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[PolicyStatus_EL]]", ""), "Not Insured"));
                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[PolicyStatus_ED]]", ""), "Not Insured"));
                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[PolicyStatus_LPD]]", ""), "Not Insured"));
                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[PolicyStatus_FID]]", ""), "Not Insured"));
                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[PolicyStatus_IL]]", ""), "Not Insured"));
                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[PolicyStatus_CL]]", ""), "Not Insured"));

                // loop over terms and set merge feilds
                foreach (var agreementlist in agreement.ClientInformationSheet.Programme.Agreements.Where(a => a.DateDeleted == null))
                {
                    foreach (var term in agreementlist.ClientAgreementTerms)
                    {
                        if (term.Bound)
                        {

                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundLimit_{0}]]", term.SubTermType), term.TermLimit.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundLimitx2_{0}]]", term.SubTermType), (term.TermLimit * 2).ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundLimitx3_{0}]]", term.SubTermType), (term.TermLimit * 3).ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundLimitx4_{0}]]", term.SubTermType), (term.TermLimit * 4).ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundLimitx5_{0}]]", term.SubTermType), (term.TermLimit * 5).ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundExcess_{0}]]", term.SubTermType), term.Excess.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[ProgrammeBoundLimit_{0}]]", term.SubTermType), term.TermLimit.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[ProgrammeBoundExcess_{0}]]", term.SubTermType), term.Excess.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));

                            mergeFields.Remove(new KeyValuePair<string, string>(string.Format("[[PolicyStatus_{0}]]", term.SubTermType), "Not Insured"));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[PolicyStatus_{0}]]", term.SubTermType), "Insured"));

                            if (term.SubTermType == "IL")
                            {
                                var CLAgreement = agreement.ClientInformationSheet.Programme.Agreements.Where(a => a.DateDeleted == null).FirstOrDefault(p => p.ClientAgreementTerms.Any(i => i.SubTermType == "CL"));
                                if (CLAgreement != null)
                                {
                                    if (CLAgreement.ClientAgreementTerms.Where(cla => cla.DateDeleted == null && cla.Bound).Count() > 0)
                                    {
                                        mergeFields.Remove(new KeyValuePair<string, string>(string.Format("[[PolicyStatus_{0}]]", term.SubTermType), "Insured"));
                                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[PolicyStatus_{0}]]", term.SubTermType), "Not Insured"));
                                    }
                                }
                            }

                            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
                            {
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumAdjustment_{0}]]", term.SubTermType), (term.PremiumDiffer - term.FSLDiffer).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremium_{0}]]", term.SubTermType), term.PremiumDiffer.ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFee_{0}]]", term.SubTermType), (term.PremiumDiffer + agreement.BrokerFee).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundGST_{0}]]", term.SubTermType), ((term.PremiumDiffer) * agreement.Product.TaxRate).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFSL_{0}]]", term.SubTermType), term.FSLDiffer.ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFeeGST_{0}]]", term.SubTermType), ((term.PremiumDiffer + agreement.BrokerFee) * agreement.Product.TaxRate).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFeeInclGST_{0}]]", term.SubTermType), ((term.PremiumDiffer + agreement.BrokerFee) * (1 + agreement.Product.TaxRate)).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[CreditCardSurcharge_{0}]]", term.SubTermType), ((term.PremiumDiffer + agreement.BrokerFee) * (0.013m)).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFeeCCSurchargeGST_{0}]]", term.SubTermType), ((term.PremiumDiffer + agreement.BrokerFee) * (1 + 0.013m) * agreement.Product.TaxRate).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclGSTCreditCardCharge_{0}]]", term.SubTermType), ((term.PremiumDiffer + agreement.BrokerFee) * (1 + agreement.Product.TaxRate) * 1.013m).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[ProgrammeBoundPremium_{0}]]", term.SubTermType), term.Excess.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumFAP_{0}]]", term.SubTermType), term.FAPPremium.ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFeeInclGSTMonthly_{0}]]", term.SubTermType), ((term.PremiumDiffer + agreement.BrokerFee) * (1 + agreement.Product.TaxRate) / intMonthlyInstalmentNumber).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                if (agreementlist.Status == "Bound" || agreementlist.Status == "Bound and pending payment" || agreementlist.Status == "Bound and invoice pending" || agreementlist.Status == "Bound and invoiced; Bound")
                                    PremiumTotal += term.PremiumDiffer;
                            }
                            else
                            {
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumAdjustment_{0}]]", term.SubTermType), (term.Premium - term.FSL).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremium_{0}]]", term.SubTermType), term.Premium.ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFee_{0}]]", term.SubTermType), (term.Premium + agreement.BrokerFee).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundGST_{0}]]", term.SubTermType), ((term.Premium) * agreement.Product.TaxRate).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFSL_{0}]]", term.SubTermType), term.FSL.ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFeeGST_{0}]]", term.SubTermType), ((term.Premium + agreement.BrokerFee) * agreement.Product.TaxRate).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFeeInclGST_{0}]]", term.SubTermType), ((term.Premium + agreement.BrokerFee) * (1 + agreement.Product.TaxRate)).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[CreditCardSurcharge_{0}]]", term.SubTermType), ((term.Premium + agreement.BrokerFee) * 0.013m).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFeeCCSurchargeGST_{0}]]", term.SubTermType), ((term.Premium + agreement.BrokerFee) * (1 + 0.013m) * agreement.Product.TaxRate).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclGSTCreditCardCharge_{0}]]", term.SubTermType), ((term.Premium + agreement.BrokerFee) * (1 + agreement.Product.TaxRate) * 1.013m).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[ProgrammeBoundPremium_{0}]]", term.SubTermType), term.Excess.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumFAP_{0}]]", term.SubTermType), term.FAPPremium.ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFeeInclGSTMonthly_{0}]]", term.SubTermType), ((term.Premium + agreement.BrokerFee) * (1 + agreement.Product.TaxRate) / intMonthlyInstalmentNumber).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                                if (agreementlist.Status == "Bound" || agreementlist.Status == "Bound and pending payment" || agreementlist.Status == "Bound and invoice pending" || agreementlist.Status == "Bound and invoiced; Bound")
                                    PremiumTotal += term.Premium;
                            }

                            //Endorsements
                            if (agreementlist.ClientAgreementEndorsements.Where(ce => ce.DateDeleted == null && !ce.Removed).Count() > 0)
                            {
                                DataTable dt9 = new DataTable();
                                dt9.Columns.Add("Endorsement Name");
                                dt9.Columns.Add("Product Name");
                                dt9.Columns.Add("Endorsement Text");

                                foreach (ClientAgreementEndorsement ClientAgreementEndorsement in agreementlist.ClientAgreementEndorsements)
                                {
                                    if (ClientAgreementEndorsement.DateDeleted == null && !ClientAgreementEndorsement.Removed)
                                    {
                                        DataRow dr9 = dt9.NewRow();

                                        dr9["Endorsement Name"] = ClientAgreementEndorsement.Name;
                                        if (agreementlist.Product != null)
                                        {
                                            dr9["Product Name"] = agreementlist.Product.Name;
                                        }

                                        dr9["Endorsement Text"] = ClientAgreementEndorsement.Value;

                                        dt9.Rows.Add(dr9);
                                    }

                                }

                                dt9.TableName = "EndorsementTable";

                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[EndorsementTable_{0}]]", term.SubTermType), ConvertDataTableToHTML(dt9)));
                            }
                            else
                            {
                                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[EndorsementTable_{0}]]", term.SubTermType), ""));
                            }

                            if (term.SubTermType == "CL")
                            {
                                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").Count() == 0 ||
                                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").Count() == 0 ||
                                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalCLEOptions").Count() == 0)
                                {
                                    mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CL]]", "Extension NOT Included"));
                                }
                                else
                                {
                                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").First().Value == "1" &&
                                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").First().Value == "1" &&
                                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalCLEOptions").First().Value == "1")
                                    {
                                        mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CL]]", "Extension Included"));
                                    }
                                    else
                                    {
                                        mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CL]]", "Extension NOT Included"));
                                    }
                                }

                                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").Count() == 0 ||
                                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").Count() == 0)
                                {
                                    mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CL1]]", "Extension NOT Included"));
                                }
                                else
                                {
                                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").First().Value == "1" &&
                                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").First().Value == "1")
                                    {
                                        mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CL1]]", "Extension Included"));
                                    }
                                    else
                                    {
                                        mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CL1]]", "Extension NOT Included"));
                                    }
                                }

                                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").Count() == 0 ||
                                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").Count() == 0 ||
                                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalCLEOptions").Count() == 0)
                                {
                                    mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CLExt]]", "Extension NOT Included"));
                                }
                                else
                                {
                                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").First().Value == "1" &&
                                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").First().Value == "1" &&
                                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalCLEOptions").First().Value == "1")
                                    {
                                        bool clextensioninclude = false;
                                        foreach (var cltermExtension in agreement.ClientAgreementTermExtensions.Where(aecl => aecl.DateDeleted == null))
                                        {
                                            if (cltermExtension.Bound && !clextensioninclude)
                                            {
                                                clextensioninclude = true;
                                            }
                                        }
                                        if (clextensioninclude)
                                        {
                                            mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CLExt]]", "Extension Included"));
                                        } else
                                        {
                                            mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CLExt]]", "Extension NOT Included"));
                                        }
                                    }
                                    else
                                    {
                                        mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CLExt]]", "Extension NOT Included"));
                                    }
                                }
                            }
                        }
                    }

                    foreach (var termExtension in agreementlist.ClientAgreementTermExtensions.Where(ae => ae.DateDeleted == null))
                    {
                        if (termExtension.Bound)
                        {
                            if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
                            {
                                PremiumTotal += termExtension.PremiumDiffer;
                            }
                            else
                            {
                                PremiumTotal += termExtension.Premium;
                            }
                        }
                    }

                    if (agreementlist.Product.DefaultEnableBrokerFee)
                    {
                        PremiumTotal += agreementlist.BrokerFee;
                    }
                }

                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[ProgrammeBoundPremium_Total]]", ""), PremiumTotal.ToString("C2", CultureInfo.CreateSpecificCulture("en-NZ"))));
                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[ProgrammeBoundPremium_GST]]", ""), (PremiumTotal * (decimal)0.15).ToString("C2", CultureInfo.CreateSpecificCulture("en-NZ"))));

                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[ProgrammeBoundPremiuminclGst_Total]]", ""), (PremiumTotal * (decimal)1.15).ToString("C2", CultureInfo.CreateSpecificCulture("en-NZ"))));
                mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[ProgrammeBoundPremiuminclGstMonthly_Total]]", ""), (PremiumTotal * (decimal)1.15 / intMonthlyInstalmentNumber).ToString("C2", CultureInfo.CreateSpecificCulture("en-NZ"))));

                if (agreement.ClientInformationSheet.Locations.Any())
                {
                    mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[InsuredPostalAddress]]", ""),
                agreement.ClientInformationSheet.Locations.FirstOrDefault().Street + " " + agreement.ClientInformationSheet.Locations.FirstOrDefault().Suburb));
                    mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[InsuredCity]]", ""),
                    agreement.ClientInformationSheet.Locations.FirstOrDefault().City));

                    mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[InsuredPostCode]]", ""),
                   agreement.ClientInformationSheet.Locations.FirstOrDefault().Postcode));
                }

                //render job cert job detail
                if (job != null)
                {
                    string jobdetail = "";
                    jobdetail = "<strong>Job Detail</strong>" +
                     "<br />" + "Job Description:                   " + job.JobDescription +
                     "<br />" + "Job Number:                         " + job.JobNumber +
                     "<br />" + "Start Date:                             " + job.StartDate.ToString("dd/MM/yyyy") +
                     "<br />" + "End Date:                               " + job.EndDate.ToString("dd/MM/yyyy") +
                     "<br />" + "RA Client Named Party:      " + job.RACLientNamedParty;

                    mergeFields.Add(new KeyValuePair<string, string>("[[JobDetail]]", jobdetail));

                }
                else
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[JobDetail]]", ""));
                }

                //MV Details
                if (agreement.ClientAgreementTerms.Any(cat => cat.SubTermType == "MV"))
                {
                    int intMVNumberOfUnits = 0;
                    string strFinancialIPList = null;
                    int intFinancialIPCount = 0;

                    DataTable dt = new DataTable();
                    dt.Columns.Add("Category");
                    dt.Columns.Add("Year");
                    dt.Columns.Add("Make");
                    dt.Columns.Add("Model");
                    dt.Columns.Add("Fleet No.");
                    dt.Columns.Add("Registration");
                    dt.Columns.Add("Interest Parties");
                    dt.Columns.Add("Sum Insured");

                    var AgreementMVTerms = await _clientAgreementMVTermService.GetAllAgreementMVTermFor(agreement.ClientAgreementTerms.FirstOrDefault(at => at.SubTermType == "MV"));
                    AgreementMVTerms.OrderBy(camvt => camvt.Registration);

                    foreach (ClientAgreementMVTerm mVTerm in AgreementMVTerms)
                    {
                        DataRow dr = dt.NewRow();

                        dr["Category"] = mVTerm.VehicleCategory;
                        dr["Year"] = mVTerm.Year;
                        dr["Make"] = mVTerm.Make;
                        dr["Model"] = mVTerm.Model;
                        dr["Fleet No."] = mVTerm.FleetNumber;
                        dr["Registration"] = mVTerm.Registration;

                        string strInterestPartiesNamesList = null;
                        int intIPCount = 0;
                        if (mVTerm.Vehicle.InterestedParties.Count > 0)
                        {
                            foreach (Organisation InterestParty in mVTerm.Vehicle.InterestedParties)
                            {
                                if (intIPCount == 0)
                                {
                                    strInterestPartiesNamesList = InterestParty.Name;
                                }
                                else
                                {
                                    strInterestPartiesNamesList = strInterestPartiesNamesList + ", " + InterestParty.Name;
                                }

                                intIPCount += 1;

                                if (InterestParty.OrganisationType.Name == "financial")
                                {
                                    if (intFinancialIPCount == 0)
                                    {
                                        strFinancialIPList = InterestParty.Name;
                                    }
                                    else
                                    {
                                        strFinancialIPList = strInterestPartiesNamesList + ", " + InterestParty.Name;
                                    }
                                    intFinancialIPCount += 1;
                                }
                            }
                        }
                        dr["Interest Parties"] = strInterestPartiesNamesList;

                        dr["Sum Insured"] = mVTerm.TermLimit.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"));

                        dt.Rows.Add(dr);

                        intMVNumberOfUnits += 1;
                    }

                    dt.TableName = "MVDetailsTable";

                    DataTable dt2 = new DataTable();
                    dt2.Columns.Add("Year");
                    dt2.Columns.Add("Make");
                    dt2.Columns.Add("Model");
                    dt2.Columns.Add("Registration");
                    dt2.Columns.Add("Sum Insured");
                    //dt2.Columns.Add ("End Date");

                    AgreementMVTerms = await _clientAgreementMVTermService.GetAllAgreementMVTermFor(agreement.ClientAgreementTerms.FirstOrDefault(at => at.SubTermType == "MV"));
                    AgreementMVTerms.OrderBy(camvt => camvt.Registration);

                    foreach (ClientAgreementMVTerm mVTerm2 in AgreementMVTerms)
                    {
                        DataRow dr2 = dt2.NewRow();

                        dr2["Year"] = mVTerm2.Year;
                        dr2["Make"] = mVTerm2.Make;
                        dr2["Model"] = mVTerm2.Model;
                        dr2["Registration"] = mVTerm2.Registration;
                        dr2["Sum Insured"] = mVTerm2.TermLimit.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"));
                        //dr2 ["End Date"] = "01/09/2018";

                        dt2.Rows.Add(dr2);

                    }

                    dt2.TableName = "MVDetailsTable2";

                    mergeFields.Add(new KeyValuePair<string, string>("[[MVNumberOfUnits]]", intMVNumberOfUnits.ToString()));
                    mergeFields.Add(new KeyValuePair<string, string>("[[MVDetailsTable]]", ConvertDataTableToHTML(dt)));
                    mergeFields.Add(new KeyValuePair<string, string>("[[MVDetailsTable2]]", ConvertDataTableToHTML(dt2)));

                    //if (intFinancialIPCount > 1)
                    //{
                    //    strFinancialIP = strFinancialIPList + " are";
                    //} else
                    //{
                    //    strFinancialIP = strFinancialIPList + " is";
                    //}
                    mergeFields.Add(new KeyValuePair<string, string>("[[FinancialIP]]", strFinancialIPList));
                }

                //BV Details
                if (agreement.ClientAgreementTerms.Any(cat => cat.SubTermType == "BV"))
                {
                    int intBVNumberOfUnits = 0;

                    DataTable dtbv = new DataTable();
                    dtbv.Columns.Add("Name");
                    dtbv.Columns.Add("Year");
                    dtbv.Columns.Add("Make");
                    dtbv.Columns.Add("Model");

                    /*foreach (ClientAgreementBVTerm bVTerm in _clientAgreementBVTermService.GetAllAgreementBVTermFor(agreement.ClientAgreementTerms.FirstOrDefault(at => at.SubTermType == "BV")).OrderBy(cabvt => cabvt.BoatName))
                    {
                        DataRow drbv = dtbv.NewRow();

                        drbv["Name"] = bVTerm.BoatName;
                        drbv["Year"] = bVTerm.YearOfManufacture;
                        drbv["Make"] = bVTerm.BoatMake;
                        drbv["Model"] = bVTerm.BoatModel;

                        string strBVInterestPartiesNamesList = null;
                        int intBVIPCount = 0;
                        if (bVTerm.Boat.InterestedParties.Count > 0)
                        {
                            foreach (Organisation InterestParty in bVTerm.Boat.InterestedParties)
                            {
                                if (intBVIPCount == 0)
                                {
                                    strBVInterestPartiesNamesList = InterestParty.Name;
                                }
                                else
                                {
                                    strBVInterestPartiesNamesList = strBVInterestPartiesNamesList + ", " + InterestParty.Name;
                                }

                                intBVIPCount += 1;
                            }
                        }
                        //drbv["Interest Parties"] = strBVInterestPartiesNamesList;

                        dtbv.Rows.Add(drbv);


                    }*/

                    DataTable dtbv2 = new DataTable();
                    dtbv2.Columns.Add("Name");
                    dtbv2.Columns.Add("Year");
                    dtbv2.Columns.Add("Make");
                    dtbv2.Columns.Add("Model");
                    dtbv2.Columns.Add("Sum Insured");
                    dtbv2.Columns.Add("Effective Date");

                    DataTable dtbv3 = new DataTable();
                    dtbv3.Columns.Add("Name");
                    //dtbv3.Columns.Add("Year");
                    //dtbv3.Columns.Add("Make");
                    //dtbv3.Columns.Add("Model");
                    dtbv3.Columns.Add("Excess");

                    DataTable dtbv4 = new DataTable();
                    dtbv4.Columns.Add("Name");
                    //dtbv4.Columns.Add("Year");
                    //dtbv4.Columns.Add("Make");
                    //dtbv4.Columns.Add("Model");
                    dtbv4.Columns.Add("Excess");

                    DataTable dtbv5 = new DataTable();
                    dtbv5.Columns.Add("Name");
                    //dtbv4.Columns.Add("Year");
                    //dtbv4.Columns.Add("Make");
                    //dtbv4.Columns.Add("Model");
                    dtbv5.Columns.Add("Interest Party");

                    var AgreementBVTerms = await _clientAgreementBVTermService.GetAllAgreementBVTermFor(agreement.ClientAgreementTerms.FirstOrDefault(at => at.SubTermType == "BV"));
                    AgreementBVTerms.OrderBy(cabvt => cabvt.BoatName);

                    foreach (ClientAgreementBVTerm bVTerm in AgreementBVTerms)
                    {
                        intBVNumberOfUnits += 1;

                        DataRow drbv = dtbv.NewRow();
                        DataRow drbv2 = dtbv2.NewRow();
                        DataRow drbv3 = dtbv3.NewRow();
                        DataRow drbv4 = dtbv4.NewRow();
                        DataRow drbv5 = dtbv5.NewRow();

                        drbv["Name"] = bVTerm.BoatName;
                        drbv["Year"] = bVTerm.YearOfManufacture;
                        drbv["Make"] = bVTerm.BoatMake;
                        drbv["Model"] = bVTerm.BoatModel;

                        dtbv.Rows.Add(drbv);

                        drbv2["Name"] = bVTerm.BoatName;
                        drbv2["Year"] = bVTerm.YearOfManufacture;
                        drbv2["Make"] = bVTerm.BoatMake;
                        drbv2["Model"] = bVTerm.BoatModel;
                        drbv2["Sum Insured"] = bVTerm.TermLimit.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"));
                        drbv2["Effective Date"] = bVTerm.Boat.BoatEffectiveDate.ToShortDateString();

                        dtbv2.Rows.Add(drbv2);

                        drbv3["Name"] = bVTerm.BoatName;
                        //drbv3["Year"] = bVTerm.YearOfManufacture;
                        //drbv3["Make"] = bVTerm.BoatMake;
                        //drbv3["Model"] = bVTerm.BoatModel;
                        drbv3["Excess"] = bVTerm.Excess.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ")) + " any one Accident or Occurrence";

                        dtbv3.Rows.Add(drbv3);

                        drbv4["Name"] = bVTerm.BoatName;
                        //drbv4["Year"] = bVTerm.YearOfManufacture;
                        //drbv4["Make"] = bVTerm.BoatMake;
                        //drbv4["Model"] = bVTerm.BoatModel;
                        drbv4["Excess"] = "Not Included";
                        if (bVTerm.Boat.BoatType1 == "YachtsandCatamarans" && bVTerm.Boat.BoatUses.Where(ycbu => ycbu.BoatUseCategory == "Race" && !ycbu.Removed && ycbu.DateDeleted == null).Count() > 0)
                        {
                            foreach (BoatUse boatuse in bVTerm.Boat.BoatUses)
                            {
                                drbv4["Excess"] = "$2,500";
                            }
                        }
                        dtbv4.Rows.Add(drbv4);

                        drbv5["Name"] = bVTerm.BoatName;
                        //drbv4["Year"] = bVTerm.YearOfManufacture;
                        //drbv4["Make"] = bVTerm.BoatMake;
                        //drbv4["Model"] = bVTerm.BoatModel;
                        string strBVInterestPartiesNamesList = null;
                        int intBVIPCount = 0;
                        if (bVTerm.Boat.InterestedParties.Count > 0)
                        {
                            foreach (Organisation InterestParty in bVTerm.Boat.InterestedParties)
                            {
                                if (intBVIPCount == 0)
                                {
                                    strBVInterestPartiesNamesList = InterestParty.Name;
                                }
                                else
                                {
                                    strBVInterestPartiesNamesList = strBVInterestPartiesNamesList + ", " + InterestParty.Name;
                                }

                                intBVIPCount += 1;
                            }
                        }
                        else
                        {
                            strBVInterestPartiesNamesList = "None";
                        }
                        drbv5["Interest Party"] = strBVInterestPartiesNamesList;
                        dtbv5.Rows.Add(drbv5);

                    }

                    dtbv.TableName = "BVDetailsTable";
                    dtbv2.TableName = "BVDetailsTable2";
                    dtbv3.TableName = "BVExcessDetailsTable";
                    dtbv4.TableName = "BVRaceDetailsTable";
                    dtbv5.TableName = "BVInterestPartyTable";

                    mergeFields.Add(new KeyValuePair<string, string>("[[BVNumberOfUnits]]", intBVNumberOfUnits.ToString()));
                    mergeFields.Add(new KeyValuePair<string, string>("[[BVDetailsTable]]", ConvertDataTableToHTML(dtbv)));
                    mergeFields.Add(new KeyValuePair<string, string>("[[BVDetailsTable2]]", ConvertDataTableToHTML(dtbv2)));
                    mergeFields.Add(new KeyValuePair<string, string>("[[BVExcessDetailsTable]]", ConvertDataTableToHTML(dtbv3)));
                    mergeFields.Add(new KeyValuePair<string, string>("[[BVRaceDetailsTable]]", ConvertDataTableToHTML(dtbv4)));
                    mergeFields.Add(new KeyValuePair<string, string>("[[BVInterestPartyTable]]", ConvertDataTableToHTML(dtbv5)));

                    var clientAgreementMVTerm = await _clientAgreementMVTermService.GetAllAgreementMVTermFor(agreement.ClientAgreementTerms.FirstOrDefault(at => at.SubTermType == "BV" && at.DateDeleted == null));
                    if (clientAgreementMVTerm.Count() > 0)
                    {
                        var AgreementMVTerm = await _clientAgreementMVTermService.GetAllAgreementMVTermFor(agreement.ClientAgreementTerms.FirstOrDefault(at => at.SubTermType == "BV" && at.DateDeleted == null));
                        AgreementMVTerm.OrderBy(camvt => camvt.Registration);

                        foreach (ClientAgreementMVTerm mVTerm in AgreementMVTerm)
                        {

                            DataTable dtmv1 = new DataTable();
                            dtmv1.Columns.Add("Year");
                            dtmv1.Columns.Add("Make");
                            dtmv1.Columns.Add("Model");
                            dtmv1.Columns.Add("Registration");
                            dtmv1.Columns.Add("Sum Insured");
                            dtmv1.Columns.Add("Effective Date");

                            AgreementMVTerm = await _clientAgreementMVTermService.GetAllAgreementMVTermFor(agreement.ClientAgreementTerms.FirstOrDefault(at => at.SubTermType == "BV"));
                            AgreementMVTerm.OrderBy(camvt => camvt.Registration);

                            foreach (ClientAgreementMVTerm bVMVTerm in AgreementMVTerm)
                            {
                                DataRow drmv1 = dtmv1.NewRow();

                                drmv1["Year"] = bVMVTerm.Year;
                                drmv1["Make"] = bVMVTerm.Make;
                                drmv1["Model"] = bVMVTerm.Model;
                                drmv1["Registration"] = bVMVTerm.Registration;
                                drmv1["Sum Insured"] = bVMVTerm.TermLimit.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"));
                                drmv1["Effective Date"] = bVMVTerm.Vehicle.VehicleEffectiveDate.ToShortDateString();

                                dtmv1.Rows.Add(drmv1);

                            }

                            dtmv1.TableName = "BVMVDetailsTable";

                            mergeFields.Add(new KeyValuePair<string, string>("[[BVMVDetailsTable]]", ConvertDataTableToHTML(dtmv1)));
                        }
                    }
                    else
                    {
                        mergeFields.Add(new KeyValuePair<string, string>("[[BVMVDetailsTable]]", "No Trailer insured under this policy."));
                    }
                }

                string stradvisorlist = "";
                string stradvisorlist1 = "";
                string stradvisorlist2 = "";
                string strnominatedrepresentative = "";
                string strotherconsultingbusiness = "";
                string strmentoradvisorlist = "";

                if (agreement.ClientInformationSheet.Organisation.Count > 0)
                {

                    foreach (var uisorg in agreement.ClientInformationSheet.Organisation)
                    {
                        if (!uisorg.Removed)
                        {
                            var unit = (AdvisorUnit)uisorg.OrganisationalUnits.FirstOrDefault(u => u.Name == "Advisor");
                            if (unit != null)
                            {
                                if (string.IsNullOrEmpty(stradvisorlist))
                                {
                                    stradvisorlist = "Advisor:                                           " + uisorg.Name +
                                        "<br />" + "Retroactive Date:                          " + unit.PIRetroactivedate;
                                }
                                else
                                {
                                    stradvisorlist += "<br />" + "Advisor:                                           " + uisorg.Name +
                                        "<br />" + "Retroactive Date:                          " + unit.PIRetroactivedate;
                                }
                                if (string.IsNullOrEmpty(stradvisorlist1))
                                {
                                    stradvisorlist1 = "Advisor:                                           " + uisorg.Name +
                                        "<br />" + "Retroactive Date:                          " + unit.DORetroactivedate;
                                }
                                else
                                {
                                    stradvisorlist1 += "<br />" + "Advisor:                                           " + uisorg.Name +
                                        "<br />" + "Retroactive Date:                          " + unit.DORetroactivedate;
                                }
                                if (string.IsNullOrEmpty(stradvisorlist2))
                                {
                                    stradvisorlist2 = uisorg.Name;
                                }
                                else
                                {
                                    stradvisorlist2 += ", " + uisorg.Name;
                                }
                            }

                            var unit1 = (AdvisorUnit)uisorg.OrganisationalUnits.FirstOrDefault(u => u.Name == "Nominated Representative");
                            if (unit1 != null)
                            {
                                if (string.IsNullOrEmpty(strnominatedrepresentative))
                                {
                                    strnominatedrepresentative = "Nominated Representative:       " + uisorg.Name;
                                }
                                else
                                {
                                    strnominatedrepresentative += ", " + uisorg.Name;
                                }
                            }

                            var unit2 = (AdvisorUnit)uisorg.OrganisationalUnits.FirstOrDefault(u => u.Name == "Other Consulting Business");
                            if (unit2 != null)
                            {
                                if (string.IsNullOrEmpty(strotherconsultingbusiness))
                                {
                                    strotherconsultingbusiness = uisorg.Name;
                                }
                                else
                                {
                                    strotherconsultingbusiness += ", " + uisorg.Name;
                                }
                            }

                            var unit3 = (AdvisorUnit)uisorg.OrganisationalUnits.FirstOrDefault(u => u.Name == "Mentored Advisor");
                            if (unit3 != null)
                            {
                                string mentoredadvisorexpirydate = "";
                                //if (unit3.DateofCommencement.Value.AddMonths(6) > agreement.ExpiryDate)
                                //{
                                //    mentoredadvisorexpirydate = 
                                //        TimeZoneInfo.ConvertTimeFromUtc(agreement.ExpiryDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"));
                                //} else
                                //{
                                //    mentoredadvisorexpirydate =
                                //        TimeZoneInfo.ConvertTimeFromUtc(unit3.DateofCommencement.Value.AddMonths(6), TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"));
                                //}
                                mentoredadvisorexpirydate =
                                        TimeZoneInfo.ConvertTimeFromUtc(agreement.ExpiryDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"));
                                if (string.IsNullOrEmpty(strmentoradvisorlist))
                                {
                                    strmentoradvisorlist = "Name:            " + uisorg.Name +
                                        "<br />" + "Expiry Date:  " + mentoredadvisorexpirydate;
                                }
                                else
                                {
                                    strmentoradvisorlist += "<br />" + "Name:            " + uisorg.Name +
                                        "<br />" + "Expiry Date:  " + mentoredadvisorexpirydate;
                                }
                            }


                        }

                    }

                    if (!string.IsNullOrEmpty(strnominatedrepresentative))
                    {
                        stradvisorlist += "<br /><br />" + strnominatedrepresentative;
                    }

                    if (string.IsNullOrEmpty(strotherconsultingbusiness))
                    {
                        strotherconsultingbusiness = "No Additional Insureds.";
                    }

                    if (string.IsNullOrEmpty(strmentoradvisorlist))
                    {
                        strmentoradvisorlist = "No Mentored Advisor insured under this policy.";
                    }
                    if (string.IsNullOrEmpty(stradvisorlist2))
                    {
                        stradvisorlist2 = "No Advisor insured under this policy.";
                    }

                    mergeFields.Add(new KeyValuePair<string, string>("[[AdvisorDetailsTablePI]]", stradvisorlist));
                    mergeFields.Add(new KeyValuePair<string, string>("[[AdvisorDetailsTableDO]]", stradvisorlist1));
                    mergeFields.Add(new KeyValuePair<string, string>("[[OtherConsultingBusiness]]", strotherconsultingbusiness));
                    mergeFields.Add(new KeyValuePair<string, string>("[[MontoredAdvisorDetails]]", strmentoradvisorlist));
                    mergeFields.Add(new KeyValuePair<string, string>("[[AdvisorNames]]", stradvisorlist2));

                }
                else
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[AdvisorDetailsTablePI]]", "No Advisor insured under this policy."));
                    mergeFields.Add(new KeyValuePair<string, string>("[[AdvisorDetailsTableDO]]", "No Advisor insured under this policy."));
                    mergeFields.Add(new KeyValuePair<string, string>("[[OtherConsultingBusiness]]", "No Additional Insured insureds."));
                    mergeFields.Add(new KeyValuePair<string, string>("[[MontoredAdvisorDetails]]", "No Mentored Advisor insured under this policy."));
                    mergeFields.Add(new KeyValuePair<string, string>("[[AdvisorNames]]", "No Advisor insured under this policy."));
                }

                //Advisor list with FAP Number

                string stradvisornominatedrepresentative = "";
                string strprincipleadvisorname = "";
                string strInceptionDateForFAP = "";
                int intadvisornumber = 1;

                strInceptionDateForFAP = TimeZoneInfo.ConvertTimeFromUtc(agreement.InceptionDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"));

                if (agreement.ClientInformationSheet.Organisation.Count > 0)
                {
                    foreach (var uisorg in agreement.ClientInformationSheet.Organisation)
                    {
                        var principleadvisorunit = (AdvisorUnit)uisorg.OrganisationalUnits.FirstOrDefault(u => u.Name == "Advisor" && u.DateDeleted == null);

                        if (principleadvisorunit != null)
                        {
                            if (principleadvisorunit.IsPrincipalAdvisor && uisorg.DateDeleted == null && !uisorg.Removed)
                            {
                                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "FAPViewModel.TransitionalLicenseNum").Any())
                                {
                                    stradvisornominatedrepresentative = uisorg.Name + "(FAP number: " +
                                        agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "FAPViewModel.TransitionalLicenseNum").First().Value + ")";
                                }
                                else
                                {
                                    stradvisornominatedrepresentative = uisorg.Name + "(FAP number: None)";
                                }

                                strprincipleadvisorname = uisorg.Name;
                            }
                        }

                    }


                    if (agreement.ClientInformationSheet.SubClientInformationSheets.Where(advisorsubuis => advisorsubuis.DateDeleted == null).Count() > 0)
                    {
                        if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "FAPViewModel.CoverStartDate").Any())
                        {
                            strInceptionDateForFAP = agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "FAPViewModel.CoverStartDate").First().Value.ToString();
                        }
                    }

                    foreach (var advsubuis in agreement.ClientInformationSheet.SubClientInformationSheets.Where(advisorsubuis => advisorsubuis.DateDeleted == null))
                    {
                        intadvisornumber += 1;

                        if (string.IsNullOrEmpty(stradvisornominatedrepresentative))
                        {
                            if (advsubuis.Answers.Where(sa => sa.ItemName == "FAPViewModel.TransitionalLicenseNum").Any())
                            {
                                stradvisornominatedrepresentative = advsubuis.Owner.Name + "(FAP number: " +
                                    advsubuis.Answers.Where(sa => sa.ItemName == "FAPViewModel.TransitionalLicenseNum").First().Value + ")";
                            }
                            else
                            {
                                stradvisornominatedrepresentative = advsubuis.Owner.Name + "(FAP number: None)";
                            }
                        }
                        else
                        {
                            if (advsubuis.Answers.Where(sa => sa.ItemName == "FAPViewModel.TransitionalLicenseNum").Any())
                            {
                                stradvisornominatedrepresentative += "<br />" + advsubuis.Owner.Name + "(FAP number: " +
                                    advsubuis.Answers.Where(sa => sa.ItemName == "FAPViewModel.TransitionalLicenseNum").First().Value + ")";
                            }
                            else
                            {
                                stradvisornominatedrepresentative += "<br />" + advsubuis.Owner.Name + "(FAP number: None)";
                            }
                        }
                    }
                }

                mergeFields.Add(new KeyValuePair<string, string>("[[InceptionDateForFAP]]", strInceptionDateForFAP));

                if (string.IsNullOrEmpty(stradvisornominatedrepresentative))
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[AdvisorsList]]", "No Advisor insured under this policy"));
                }
                else
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[AdvisorsList]]", stradvisornominatedrepresentative));
                }
                if (string.IsNullOrEmpty(strprincipleadvisorname))
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[PrincipleAdvisorName]]", "No Principle Advisor insured under this policy"));
                }
                else
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[PrincipleAdvisorName]]", strprincipleadvisorname));
                }
                mergeFields.Add(new KeyValuePair<string, string>("[[AdvisorsNumber]]", intadvisornumber.ToString()));

                //OT merge fields
                string OTAdvisorList = "";
                Product OTProduct = await _productService.GetProductById(new Guid("feb30dcf-c2d1-43e4-92df-d9c0fb555e5c"));
                if (OTProduct != null)
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == OTProduct.OptionalProductRequiredAnswer).Any())
                    {
                        if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == OTProduct.OptionalProductRequiredAnswer).First().Value == "1")
                        {
                            if (agreement.ClientInformationSheet.Organisation.Count > 0)
                            {
                                foreach (var uisorg in agreement.ClientInformationSheet.Organisation)
                                {
                                    var principleadvisorunit = (AdvisorUnit)uisorg.OrganisationalUnits.FirstOrDefault(u => u.Name == "Advisor" && u.DateDeleted == null);

                                    if (principleadvisorunit != null)
                                    {
                                        if (principleadvisorunit.IsPrincipalAdvisor && uisorg.DateDeleted == null && !uisorg.Removed && string.IsNullOrEmpty(OTAdvisorList))
                                        {
                                            OTAdvisorList = uisorg.Name;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (agreement.ClientInformationSheet.SubClientInformationSheets.Where(subuis => subuis.DateDeleted == null).Count() > 0)
                    {
                        foreach (var prodsubuis in agreement.ClientInformationSheet.SubClientInformationSheets.Where(prossubuis => prossubuis.DateDeleted == null))
                        {
                            if (prodsubuis.Answers.Where(sa => sa.ItemName == OTProduct.OptionalProductRequiredAnswer).Any())
                            {
                                if (prodsubuis.Answers.Where(sa => sa.ItemName == OTProduct.OptionalProductRequiredAnswer).First().Value == "1")
                                {
                                    if (string.IsNullOrEmpty(OTAdvisorList))
                                    {
                                        OTAdvisorList += prodsubuis.Owner.Name;
                                    }
                                    else
                                    {
                                        OTAdvisorList += ", " + prodsubuis.Owner.Name;
                                    }
                                }
                            }
                        }
                    }
                }
                if (string.IsNullOrEmpty(OTAdvisorList))
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[AdvisorTrustees_OT]]", "No Advisor insured under the Outside Trustees policy"));
                }
                else
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[AdvisorTrustees_OT]]", OTAdvisorList));
                }

                // merge the configured merge feilds into the document
                string content = FromBytes(template.Contents);
                try
                {
                    foreach (KeyValuePair<string, string> field in mergeFields)
                        if (field.Value != null && field.Value.Contains("&"))
                        {
                            content = content.Replace(field.Key, field.Value.Replace("&", "&amp;"));
                        }
                        else
                        {
                            content = content.Replace(field.Key, field.Value);
                        }

                }
                catch (Exception ex)
                {

                }

                // save the merged content
                doc.Contents = ToBytes(content);

            }

            return (T)doc;
        }

       
        private List<KeyValuePair<string, string>> GetMergeFields(ClientAgreement agreement, ClientInformationSheet clientInformationSheet)
        {
            List<KeyValuePair<string, string>> mergeFields = new List<KeyValuePair<string, string>>();
            mergeFields.Add(new KeyValuePair<string, string>("[[InsuredName]]", agreement.InsuredName));
            mergeFields.Add(new KeyValuePair<string, string>("[[NameOfInsured]]", agreement.InsuredName));
            mergeFields.Add(new KeyValuePair<string, string>("[[Reference]]", agreement.ClientInformationSheet.ReferenceId));
            mergeFields.Add(new KeyValuePair<string, string>("[[BrokerName]]", agreement.ClientInformationSheet.Programme.BrokerContactUser.FullName));
            mergeFields.Add(new KeyValuePair<string, string>("[[BrokerJobTitle]]", agreement.ClientInformationSheet.Programme.BrokerContactUser.JobTitle));
            mergeFields.Add(new KeyValuePair<string, string>("[[BrokerPhone]]", agreement.ClientInformationSheet.Programme.BrokerContactUser.Phone));
            mergeFields.Add(new KeyValuePair<string, string>("[[BrokerEmail]]", agreement.ClientInformationSheet.Programme.BrokerContactUser.Email));
            mergeFields.Add(new KeyValuePair<string, string>("[[BrokerAddress]]", agreement.ClientInformationSheet.Programme.BrokerContactUser.Address));
            mergeFields.Add(new KeyValuePair<string, string>("[[ClientBranchCode]]", agreement.ClientInformationSheet.Programme.EGlobalBranchCode));

            mergeFields.Add(new KeyValuePair<string, string>("[[ProgrammeClassOfInsurance]]", agreement.ClientInformationSheet.Programme.BaseProgramme.ProgMergeClassOfInsurance));
            mergeFields.Add(new KeyValuePair<string, string>("[[ProgrammeInsurer]]", agreement.ClientInformationSheet.Programme.BaseProgramme.ProgMergeInsurer));
            mergeFields.Add(new KeyValuePair<string, string>("[[ProgrammeInsurerRating]]", agreement.ClientInformationSheet.Programme.BaseProgramme.ProgMergeInsurerRating));
            mergeFields.Add(new KeyValuePair<string, string>("[[ProgrammePolicyNumber]]", agreement.ClientInformationSheet.Programme.BaseProgramme.ProgMergePolicyNumber));

            if (agreement.ClientInformationSheet.Programme.BaseProgramme.NamedPartyUnitName == "Apollo Programme" ||
                agreement.ClientInformationSheet.Programme.BaseProgramme.NamedPartyUnitName == "Apollo ML Programme" ||
                agreement.ClientInformationSheet.Programme.BaseProgramme.NamedPartyUnitName == "Apollo Run Off Programme" ||
                agreement.ClientInformationSheet.Programme.BaseProgramme.NamedPartyUnitName == "NZPI Programme")
            {
                mergeFields.Add(new KeyValuePair<string, string>("[[ClientNumber]]", agreement.ClientInformationSheet.Programme.EGlobalClientNumber != null
                                                                                    ?agreement.ClientInformationSheet.Programme.EGlobalClientNumber
                                                                                    : agreement.ClientInformationSheet.ReferenceId));
            }
            else
            {
                mergeFields.Add(new KeyValuePair<string, string>("[[ClientNumber]]", agreement.ClientInformationSheet.Programme.EGlobalClientNumber));
            }
            mergeFields.Add(new KeyValuePair<string, string>("[[ClientProgrammeMembershipNumber]]", agreement.ClientInformationSheet.Programme.ClientProgrammeMembershipNumber));
            mergeFields.Add(new KeyValuePair<string, string>("[[SubmissionDate]]", agreement.DateCreated.GetValueOrDefault().ToString("dd/MM/yyyy")));
            //mergeFields.Add(new KeyValuePair<string, string>("[[SubmissionDate]]",
            //    TimeZoneInfo.ConvertTimeFromUtc(agreement.DateCreated.GetValueOrDefault(), TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"))));
            mergeFields.Add(new KeyValuePair<string, string>("[[RetroactiveDate]]", agreement.RetroactiveDate));
            mergeFields.Add(new KeyValuePair<string, string>("[[Jurisdiction]]", agreement.Jurisdiction));
            mergeFields.Add(new KeyValuePair<string, string>("[[Territory]]", agreement.TerritoryLimit));
            mergeFields.Add(new KeyValuePair<string, string>("[[ProfessionalBusiness]]", agreement.ProfessionalBusiness));
            
            if (clientInformationSheet != null)
            {
                mergeFields.Add(new KeyValuePair<string, string>("[[SubClientName]]", clientInformationSheet.Owner.Name));

            }
            if (agreement.ClientInformationSheet != null)
            {
                mergeFields.Add(new KeyValuePair<string, string>("[[UISSubmittedByName]]", agreement.ClientInformationSheet.SubmittedBy.FirstName + " " + agreement.ClientInformationSheet.SubmittedBy.LastName));
                mergeFields.Add(new KeyValuePair<string, string>("[[UISSubmittedByEmail]]", agreement.ClientInformationSheet.SubmittedBy.Email));

                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "EPLViewModel.TotalEmployees").Any())
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[EmployeeNumber]]", Convert.ToInt32(agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "EPLViewModel.TotalEmployees").First().Value).ToString()));
                }
                else
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[EmployeeNumber]]", " "));
                }

                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "FAPViewModel.CoverStartDate").Any())
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[FAPCoverStartDate]]", agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "FAPViewModel.CoverStartDate").First().Value.ToString()));
                }
                else
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[FAPCoverStartDate]]", " "));
                }

                if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "SLViewModel.HasReportingEntityOptions").Count() == 0 ||
                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "SLViewModel.HasTrainingOptions").Count() == 0 ||
                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "SLViewModel.HasManageAMLOptions").Count() == 0 ||
                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "SLViewModel.HasAMLCFTExtensionOptions").Count() == 0)
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[RequiresAML_SL]]", "Extension NOT Included"));
                }
                else
                {
                    if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "SLViewModel.HasReportingEntityOptions").First().Value == "1" &&
                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "SLViewModel.HasTrainingOptions").First().Value == "1" &&
                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "SLViewModel.HasManageAMLOptions").First().Value == "1" &&
                    agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "SLViewModel.HasAMLCFTExtensionOptions").First().Value == "1")
                    {
                        mergeFields.Add(new KeyValuePair<string, string>("[[RequiresAML_SL]]", "Extension Included"));
                    }
                    else
                    {
                        mergeFields.Add(new KeyValuePair<string, string>("[[RequiresAML_SL]]", "Extension NOT Included"));
                    }
                }

                if (agreement.ClientInformationSheet.RevenueData != null)
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[FeeIncomeCurrentYear]]", Convert.ToDecimal(agreement.ClientInformationSheet.RevenueData.CurrentYearTotal).ToString("C2")));
                    mergeFields.Add(new KeyValuePair<string, string>("[[FeeIncomeLastYear]]", Convert.ToDecimal(agreement.ClientInformationSheet.RevenueData.LastFinancialYearTotal).ToString("C2")));
                    mergeFields.Add(new KeyValuePair<string, string>("[[FeeIncomeNextYear]]", Convert.ToDecimal(agreement.ClientInformationSheet.RevenueData.NextFinancialYearTotal).ToString("C2")));
                }

                if (agreement.ClientInformationSheet.Programme.Owner != null)
                {
                    //var principalUnit = (PrincipalUnit)agreement.ClientInformationSheet.Programme.Owner.OrganisationalUnits.FirstOrDefault(o => o.Name == "Principal");
                    //if(principalUnit != null)
                    //{
                    //    mergeFields.Add(new KeyValuePair<string, string>("[[TradingName]]", principalUnit.TradingName));
                    //}
                    mergeFields.Add(new KeyValuePair<string, string>("[[TradingName]]", agreement.ClientInformationSheet.Programme.Owner.TradingName));
                    mergeFields.Add(new KeyValuePair<string, string>("[[InsuredEmail]]", agreement.ClientInformationSheet.Programme.Owner.Email));
                }
            }

            //Eglobal merge fields
            if (agreement.ClientInformationSheet.Programme.ClientAgreementEGlobalResponses.Count > 0)
            {
                EGlobalResponse eGlobalResponse = agreement.ClientInformationSheet.Programme.ClientAgreementEGlobalResponses.Where(er => er.DateDeleted == null && er.ResponseType == "update").OrderByDescending(er => er.VersionNumber).FirstOrDefault();
                if (eGlobalResponse != null)
                {
                    if (agreement.MasterAgreement && (agreement.ReferenceId == eGlobalResponse.MasterAgreementReferenceID))
                    {
                        mergeFields.Add(new KeyValuePair<string, string>("[[InvoiceDate]]", eGlobalResponse.DateCreated.GetValueOrDefault().ToString("dd/MM/yyyy")));
                        //mergeFields.Add(new KeyValuePair<string, string>("[[InvoiceDate]]",
                        //    TimeZoneInfo.ConvertTimeFromUtc(eGlobalResponse.DateCreated.GetValueOrDefault(), TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>("[[InvoiceReference]]", eGlobalResponse.InvoiceNumber.ToString()));
                        mergeFields.Add(new KeyValuePair<string, string>("[[CoverNo]]", eGlobalResponse.CoverNumber.ToString()));
                        mergeFields.Add(new KeyValuePair<string, string>("[[Version]]", eGlobalResponse.VersionNumber.ToString().PadLeft(3, '0')));
                    }
                }
            }

            foreach (var term in agreement.ClientAgreementTerms)
            {

                if (term.Bound)
                {
                    //mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[RetroactiveDate_{0}]]", term.SubTermType), ""));
                    mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundLimit_{0}]]", term.SubTermType), term.TermLimit.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                    mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundLimitx2_{0}]]", term.SubTermType), (term.TermLimit * 2).ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                    mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundLimitx3_{0}]]", term.SubTermType), (term.TermLimit * 3).ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                    mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundLimitx4_{0}]]", term.SubTermType), (term.TermLimit * 4).ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                    mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundLimitx5_{0}]]", term.SubTermType), (term.TermLimit * 5).ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                    mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundExcess_{0}]]", term.SubTermType), term.Excess.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));

                    if (agreement.ClientInformationSheet.IsChange && agreement.ClientInformationSheet.PreviousInformationSheet != null)
                    {
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumAdjustment_{0}]]", term.SubTermType), (term.PremiumDiffer - term.FSLDiffer).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremium_{0}]]", term.SubTermType), term.PremiumDiffer.ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFee_{0}]]", term.SubTermType), (term.PremiumDiffer + agreement.BrokerFee).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundGST_{0}]]", term.SubTermType), ((term.PremiumDiffer) * agreement.Product.TaxRate).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFSL_{0}]]", term.SubTermType), term.FSLDiffer.ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFeeGST_{0}]]", term.SubTermType), ((term.PremiumDiffer + agreement.BrokerFee) * agreement.Product.TaxRate).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFeeInclGST_{0}]]", term.SubTermType), ((term.PremiumDiffer + agreement.BrokerFee) * (1 + agreement.Product.TaxRate)).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[CreditCardSurcharge_{0}]]", term.SubTermType), ((term.PremiumDiffer + agreement.BrokerFee) * (0.013m)).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFeeCCSurchargeGST_{0}]]", term.SubTermType), ((term.PremiumDiffer + agreement.BrokerFee) * (1 + 0.013m) * agreement.Product.TaxRate).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclGSTCreditCardCharge_{0}]]", term.SubTermType), ((term.PremiumDiffer + agreement.BrokerFee) * (1 + agreement.Product.TaxRate) * 1.013m).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumFAP_{0}]]", term.SubTermType), term.FAPPremium.ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                    }
                    else
                    {
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumAdjustment_{0}]]", term.SubTermType), (term.Premium - term.FSL).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremium_{0}]]", term.SubTermType), term.Premium.ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFee_{0}]]", term.SubTermType), (term.Premium + agreement.BrokerFee).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundGST_{0}]]", term.SubTermType), ((term.Premium) * agreement.Product.TaxRate).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFSL_{0}]]", term.SubTermType), term.FSL.ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFeeGST_{0}]]", term.SubTermType), ((term.Premium + agreement.BrokerFee) * agreement.Product.TaxRate).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFeeInclGST_{0}]]", term.SubTermType), ((term.Premium + agreement.BrokerFee) * (1 + agreement.Product.TaxRate)).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[CreditCardSurcharge_{0}]]", term.SubTermType), ((term.Premium + agreement.BrokerFee) * 0.013m).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclFeeCCSurchargeGST_{0}]]", term.SubTermType), ((term.Premium + agreement.BrokerFee) * (1 + 0.013m) * agreement.Product.TaxRate).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumInclGSTCreditCardCharge_{0}]]", term.SubTermType), ((term.Premium + agreement.BrokerFee) * (1 + agreement.Product.TaxRate) * 1.013m).ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundPremiumFAP_{0}]]", term.SubTermType), term.FAPPremium.ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
                    }

                    if (term.SubTermType == "PIFAP")
                    {
                        if (term.TermLimit == 0 && term.Excess == 0 && term.Premium == 0)
                        {
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFAPLimit_{0}]]", term.SubTermType), "Same as Professional Indemnity"));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFAPLimitx2_{0}]]", term.SubTermType), "Same as Professional Indemnity"));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFAPLimitx3_{0}]]", term.SubTermType), "Same as Professional Indemnity"));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFAPLimitx4_{0}]]", term.SubTermType), "Same as Professional Indemnity"));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFAPLimitx5_{0}]]", term.SubTermType), "Same as Professional Indemnity"));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFAPExcess_{0}]]", term.SubTermType), "Same as Professional Indemnity"));
                        }
                        else
                        {
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFAPLimit_{0}]]", term.SubTermType), term.TermLimit.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFAPLimitx2_{0}]]", term.SubTermType), (term.TermLimit * 2).ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFAPLimitx3_{0}]]", term.SubTermType), (term.TermLimit * 3).ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFAPLimitx4_{0}]]", term.SubTermType), (term.TermLimit * 4).ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFAPLimitx5_{0}]]", term.SubTermType), (term.TermLimit * 5).ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                            mergeFields.Add(new KeyValuePair<string, string>(string.Format("[[BoundFAPExcess_{0}]]", term.SubTermType), term.Excess.ToString("C0", CultureInfo.CreateSpecificCulture("en-NZ"))));
                        }
                    }

                    if (term.SubTermType == "CL")
                    {
                        //Extension Without Ultra Option
                        if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").Count() == 0 ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").Count() == 0 ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalCLEOptions").Count() == 0)
                        {
                            mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CL]]", "Extension NOT Included"));
                        } else
                        {
                            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").First().Value == "1" &&
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").First().Value == "1" &&
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalCLEOptions").First().Value == "1")
                            {
                                mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CL]]", "Extension Included"));
                            }
                            else
                            {
                                mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CL]]", "Extension NOT Included"));
                            }
                        }

                        if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").Count() == 0 ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").Count() == 0)
                        {
                            mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CL1]]", "Extension NOT Included"));
                        }
                        else
                        {
                            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").First().Value == "1" &&
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").First().Value == "1")
                            {
                                mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CL1]]", "Extension Included"));
                            }
                            else
                            {
                                mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CL1]]", "Extension NOT Included"));
                            }
                        }

                        //Extension With Ultra Option
                        if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").Count() == 0 ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").Count() == 0 ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalUltraOptions").Count() == 0 || 
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalCLEOptions").Count() == 0)
                        {
                            mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CLUltra]]", "Extension NOT Included"));
                        }
                        else
                        {
                            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").First().Value == "1" &&
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").First().Value == "1" &&
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalUltraOptions").First().Value == "1" && 
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalCLEOptions").First().Value == "1")
                            {
                                mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CLUltra]]", "Extension Included"));
                            }
                            else
                            {
                                mergeFields.Add(new KeyValuePair<string, string>("[[RequiresSEE_CLUltra]]", "Extension NOT Included"));
                            }
                        }

                        //Ultra vs Base differences
                        if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").Count() == 0 ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").Count() == 0 ||
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalUltraOptions").Count() == 0)
                        {
                            mergeFields.Add(new KeyValuePair<string, string>("[[CLPolciyNumber]]", "-CYB"));
                            mergeFields.Add(new KeyValuePair<string, string>("[[CLWording]]", "Cyber CYB0316"));
                            mergeFields.Add(new KeyValuePair<string, string>("[[CLSublimitOpt1]]", "$50,000"));
                            mergeFields.Add(new KeyValuePair<string, string>("[[CLSublimitOpt2]]", "$50,000"));
                            mergeFields.Add(new KeyValuePair<string, string>("[[CLSublimitOpt3]]", "$50,000"));
                        }
                        else
                        {
                            if (agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasApprovedVendorsOptions").First().Value == "1" &&
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasProceduresOptions").First().Value == "1" &&
                            agreement.ClientInformationSheet.Answers.Where(sa => sa.ItemName == "CLIViewModel.HasOptionalUltraOptions").First().Value == "1")
                            {
                                mergeFields.Add(new KeyValuePair<string, string>("[[CLPolciyNumber]]", "-CYU"));
                                mergeFields.Add(new KeyValuePair<string, string>("[[CLWording]]", "Cyber CYU0316"));
                                mergeFields.Add(new KeyValuePair<string, string>("[[CLSublimitOpt1]]", "$100,000"));
                                mergeFields.Add(new KeyValuePair<string, string>("[[CLSublimitOpt2]]", "$250,000"));
                                mergeFields.Add(new KeyValuePair<string, string>("[[CLSublimitOpt3]]", "25% of the Limit of Indemnity"));
                            }
                            else
                            {
                                mergeFields.Add(new KeyValuePair<string, string>("[[CLPolciyNumber]]", "-CYB"));
                                mergeFields.Add(new KeyValuePair<string, string>("[[CLWording]]", "Cyber CYB0316"));
                                mergeFields.Add(new KeyValuePair<string, string>("[[CLSublimitOpt1]]", "$50,000"));
                                mergeFields.Add(new KeyValuePair<string, string>("[[CLSublimitOpt2]]", "$50,000"));
                                mergeFields.Add(new KeyValuePair<string, string>("[[CLSublimitOpt3]]", "$50,000"));
                            }
                        }
                    }
                }
            }
           
            //Address needs re-work
            //mergeFields.Add(new KeyValuePair<string, string>("[[InceptionDate]]", agreement.InceptionDate.ToString("dd/MM/yyyy")));
            mergeFields.Add(new KeyValuePair<string, string>("[[InceptionDate]]", 
                TimeZoneInfo.ConvertTimeFromUtc(agreement.InceptionDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"))));
            //mergeFields.Add(new KeyValuePair<string, string>("[[ExpiryDate]]", agreement.ExpiryDate.ToString("dd/MM/yyyy")));
            mergeFields.Add(new KeyValuePair<string, string>("[[ExpiryDate]]",
                TimeZoneInfo.ConvertTimeFromUtc(agreement.ExpiryDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"))));
            if (agreement.Bound == true)
            {
                mergeFields.Add(new KeyValuePair<string, string>("[[BoundOrQuoteDate]]", agreement.BoundDate.ToString("dd/MM/yyyy")));
                //mergeFields.Add(new KeyValuePair<string, string>("[[BoundOrQuoteDate]]",
                //    TimeZoneInfo.ConvertTimeFromUtc(agreement.BoundDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"))));
            }
            else
            {
                mergeFields.Add(new KeyValuePair<string, string>("[[BoundOrQuoteDate]]", agreement.QuoteDate.ToString("dd/MM/yyyy")));
                //mergeFields.Add(new KeyValuePair<string, string>("[[BoundOrQuoteDate]]",
                //    TimeZoneInfo.ConvertTimeFromUtc(agreement.QuoteDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"))));
            }
            mergeFields.Add(new KeyValuePair<string, string>("[[BoundDate]]", agreement.BoundDate.ToString("dd/MM/yyyy")));
            mergeFields.Add(new KeyValuePair<string, string>("[[QuoteDate]]", agreement.QuoteDate.ToString("dd/MM/yyyy")));
            //mergeFields.Add(new KeyValuePair<string, string>("[[BoundDate]]",
            //    TimeZoneInfo.ConvertTimeFromUtc(agreement.BoundDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"))));
            //mergeFields.Add(new KeyValuePair<string, string>("[[QuoteDate]]",
            //    TimeZoneInfo.ConvertTimeFromUtc(agreement.QuoteDate, TimeZoneInfo.FindSystemTimeZoneById(UserTimeZone)).ToString("d", System.Globalization.CultureInfo.CreateSpecificCulture("en-NZ"))));
            mergeFields.Add(new KeyValuePair<string, string>("[[PolicyNumber]]", agreement.PolicyNumber));
            mergeFields.Add(new KeyValuePair<string, string>("[[Brokerage]]", (agreement.Brokerage / 100).ToString("P2", CultureInfo.CreateSpecificCulture("en-NZ"))));
            mergeFields.Add(new KeyValuePair<string, string>("[[AdministrationFee]]", agreement.BrokerFee.ToString("C", CultureInfo.CreateSpecificCulture("en-NZ"))));
            if (agreement.Status == "Bound" || agreement.Status == "Bound and invoice pending" || agreement.Status == "Bound and invoiced")
            {
                if (agreement.ClientInformationSheet.Programme.Payment != null)
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[CreditCardType]]", agreement.ClientInformationSheet.Programme.Payment.CreditCardType));
                    mergeFields.Add(new KeyValuePair<string, string>("[[CreditCardNumber]]", agreement.ClientInformationSheet.Programme.Payment.CreditCardNumber));
                }
                else
                {
                    mergeFields.Add(new KeyValuePair<string, string>("[[CreditCardType]]", "No Credit Card Payment"));
                    mergeFields.Add(new KeyValuePair<string, string>("[[CreditCardNumber]]", "No Credit Card Payment"));
                }

            }

            //Client Agreement Rule
            if (agreement.ClientAgreementRules.Count > 0)
            {
                if (agreement.ClientAgreementRules.FirstOrDefault(cr => cr.Name == "PaymentPremium") != null)
                {
                    string strPaymentPremium = agreement.ClientAgreementRules.FirstOrDefault(cr => cr.Name == "PaymentPremium").Value;
                    mergeFields.Add(new KeyValuePair<string, string>("[[PremiumInclusive]]", strPaymentPremium));
                }

            }
            else
            {
                mergeFields.Add(new KeyValuePair<string, string>("[[PremiumInclusive]]", ""));
            }

            //Endorsements
            if (agreement.ClientAgreementEndorsements.Where(ce => ce.DateDeleted == null && !ce.Removed).Count() > 0)
            {
                DataTable dt = new DataTable();
                dt.Columns.Add("Endorsement Name");
                //dt.Columns.Add("Product Name");
                dt.Columns.Add("Endorsement Text");

                foreach (ClientAgreementEndorsement ClientAgreementEndorsement in agreement.ClientAgreementEndorsements)
                {
                    if (ClientAgreementEndorsement.DateDeleted == null && !ClientAgreementEndorsement.Removed)
                    {
                        DataRow dr = dt.NewRow();

                        dr["Endorsement Name"] = ClientAgreementEndorsement.Name;
                        //if (agreement.ClientInformationSheet.Product != null)
                        //{
                        //    dr["Product Name"] = agreement.ClientInformationSheet.Product.Name;
                        //}

                        dr["Endorsement Text"] = ClientAgreementEndorsement.Value;

                        dt.Rows.Add(dr);
                    }
                   
                }

                dt.TableName = "EndorsementTable";

                mergeFields.Add(new KeyValuePair<string, string>("[[EndorsementTable]]", ConvertDataTableToHTML(dt)));
            }
            else
            {
                mergeFields.Add(new KeyValuePair<string, string>("[[EndorsementTable]]", ""));
            }


            return mergeFields;
        }

        public byte [] ToBytes (string contents)
		{
			return System.Text.Encoding.UTF8.GetBytes (contents);
		}

		public string FromBytes (byte [] bytes)
		{
			return System.Text.Encoding.UTF8.GetString (bytes);
		}

        public string ConvertDataTableToHTML (DataTable dt)
		{
			decimal deccolumnwidth = 100 / dt.Columns.Count;

			string html = "<table border=\"0\" cellpadding=\"0\" cellspacing=\"5\" style=\"width: 100 % \">";
			//add header row
			html += "<tr valign=\"top\">";
			for (int i = 0; i < dt.Columns.Count; i++)
				html += "<td width=\"" + deccolumnwidth + "%\"><strong>" + dt.Columns [i].ColumnName + "</strong></td>";
			html += "</tr>";
			//add rows
			for (int i = 0; i < dt.Rows.Count; i++) {
				html += "<tr valign=\"top\">";
				for (int j = 0; j < dt.Columns.Count; j++)
					html += "<td>" + dt.Rows [i] [j].ToString () + "</td>";
				html += "</tr>";
			}
			html += "</table>";
			return html;
		}

        public async Task<List<Document>> GetDocumentByOwner(Organisation Owner)
        {
            return await _documentRepository.FindAll().Where(d => d.OwnerOrganisation == Owner && d.DateDeleted == null).ToListAsync();
        }
        public async Task<Document> ConvertHTMLToPDF(Document doc)
        {

            string html = FromBytes(doc.Contents);
            //html = html.Insert(0, "<meta http-equiv=\"Content-Type\" content=\"text/html; charset=utf-8\"/>");
            //// Test if the below 4 are even necessary by this function, setting above should make these redundant now
            //html = html.Replace("“", "&quot");
            //html = html.Replace("”", "&quot");
            //html = html.Replace(" – ", "--");
            //html = html.Replace("&nbsp;", " ");
            html = html.Replace("“", "&quot");
            html = html.Replace("”", "&quot");
            html = html.Replace(" – ", "--");
            html = html.Replace("&nbsp;", " ");
            html = html.Replace("'", "&#39");
            User user = null; 
            var htmlToPdfConv = new NReco.PdfGenerator.HtmlToPdfConverter();
            htmlToPdfConv.License.SetLicenseKey(_appSettingService.NRecoUserName,_appSettingService.NRecoLicense);
            if (_appSettingService.IsLinuxEnv == "True")
            {
                htmlToPdfConv.WkHtmlToPdfExeName = "wkhtmltopdf";
            }
            htmlToPdfConv.PdfToolPath = _appSettingService.NRecoPdfToolPath;

            var margins = new PageMargins();
            margins.Bottom = 15;
            margins.Top = 15;
            margins.Left = 25;
            margins.Right = 25;
            htmlToPdfConv.Margins = margins;
            htmlToPdfConv.PageFooterHtml = "</br>" + $@"page <span class=""page""></span> of <span class=""topage""></span>";



            var pdfBytes = htmlToPdfConv.GeneratePdf(html);
            Document document = new Document(user, doc.Name+".pdf", "application/pdf", doc.DocumentType);
            document.Contents = pdfBytes;

            //var output = htmlToPdfConv.GeneratePdf(html);
            //doc.Contents = output;

            return document;
        }
        public async Task<Document> FormatCKHTMLforConversion(Document doc)
        {
            User user = null;
                
            string html = FromBytes(doc.Contents);
                
            // Bugfix for images added before Image Path fix
            string badURL = "../../../images/";
            var newURL = "https://" + _appSettingService.domainQueryString + "/Image/";
            html = html.Replace(badURL, newURL);

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

            // If re-writing this to adjust for behavior caused by lack of closing tags for elements i.e divs for images - might pay to serialize all of the elements (we didn't have serializer when this was written - and process them that way so you can get the full elements)

            // Border Fix (show & no border use cases)
            if (html.Contains(showBorder))
            {
                html = html.Replace(showBorder, "<table border=\"1\"><tbody><tr>");
            }
            if (html.Contains(noBorder))
            {
                html = html.Replace(noBorder, "<table border=\"0\"><tbody><tr>");
            }
            if (html.Contains("\r\n"))
            {
                html = html.Replace("\r\n", string.Empty);
            }
            foreach (string ele in badHtml)
            {
                int x = CountStringOccurrences(html, ele);
                var regex = new Regex(Regex.Escape(ele));
                var regex2 = new Regex(Regex.Escape("g\"></figure>"));

                for (int j = 0; j < x; j++)
                {
                    if (ele.Contains("image_resized") == false)
                    {
                        if (ele.Equals(centerImage))
                        {
                            html = regex.Replace(html, "<div style=\"text-align:center\"</div> <img src=\"", 1);
                            html = regex2.Replace(html, "g\"></div>", 1);
                        }
                        else if (ele.Equals(leftImage))
                        {
                            html = regex.Replace(html, "<div style=\"text-align:left\"</div> <img src=\"", 1);
                            html = regex2.Replace(html, "g\"></div>", 1);
                        }
                        else if (ele.Equals(rightImage))
                        {
                            html = regex.Replace(html, "<div style=\"text-align:right\"</div> <img src=\"", 1);
                            html = regex2.Replace(html, "g\"></div>", 1);
                        }
                    }
                    else
                    {
                        int widthIndex = ele.Length;
                        string width = html.Substring(html.IndexOf(ele) + widthIndex + 7, 5);
                        width = width.Replace("%", "");
                        width = width.Replace("\"", "");
                        width = width.Replace(";", "");
                        width = width.Replace(">", "");

                        int srcEndIndex = html.IndexOf(ele) + widthIndex;
                        int end = 21 + width.Length;
                        html = html.Remove(srcEndIndex, end);

                        string url = html.Substring(srcEndIndex);

                        if (url.Contains(".jpg") == true)
                        {
                            if (url.Contains(".png") == true)
                            {
                                if (url.IndexOf(".jpg") < url.IndexOf(".png"))
                                {
                                    url = url.Substring(0, url.IndexOf(".jpg") + 4);
                                }
                                else
                                {
                                    url = url.Substring(0, url.IndexOf(".png") + 4);
                                }
                            }
                            else
                            {
                                url = url.Substring(0, url.IndexOf(".jpg") + 4);
                            }
                        }
                        else if (url.Contains(".png") == true)
                        {
                            url = url.Substring(0, url.IndexOf(".png") + 4);
                        }
                        else
                        {
                            // we shouldn't get here ever as there should always be an image when we get here either .jpg or .png
                        }

                        decimal widthPercent = decimal.Parse(width);
                        widthPercent = decimal.Divide(widthPercent, 100);
                        decimal pixelWidth = 100 * widthPercent; // 500 is pretty much 100% width in the .docx documents so treating 500 as 100% and the ck value to adjust how big it should be
                        int pixelWidthZeroDP = Convert.ToInt32(pixelWidth);
                        string pixelWidthStr = pixelWidthZeroDP.ToString();

                        #region 
                        //get the actual images width
                        // note: Not useful at moment as the % CK gives you is of the page not the images actual width

                        //byte[] imageData = new WebClient().DownloadData(url);
                        //MemoryStream imgStream = new MemoryStream(imageData);
                        //System.Drawing.Image img = System.Drawing.Image.FromStream(imgStream);
                        //decimal pixelWidth = img.Width;
                        #endregion

                        if (ele.Equals(centerResize) == true)
                        {
                            html = regex.Replace(html, "<div style=\"text-align:center;\"> <img width=\"" + pixelWidthStr + "\" src=\"", 1);
                            html = regex2.Replace(html, "g\"></div>", 1); //supposed to find end of src=https://...../file.png> but alt tag exists in some images so doesn't close, works either way even if you don't close div - you'd have to write code to remove the alt tag if you wanted this to work every time
                        }
                        else if ((ele.Equals(leftResize) == true) || (ele.Equals(leftResize2) == true))
                        {
                            html = regex.Replace(html, "<div style=\"text-align:left;\"> <img width=\"" + pixelWidthStr + "\" src=\"", 1);
                            html = regex2.Replace(html, "g\"></div>", 1);
                        }
                        else if ((ele.Equals(rightResize) == true) || (ele.Equals(rightResize2) == true))
                        {
                            html = regex.Replace(html, "<div style=\"text-align:right;\"> <img width=\"" + pixelWidthStr + "\" src=\"", 1);
                            html = regex2.Replace(html, "g\"></div>", 1);
                        }
                    }
                }
            }

            doc.Contents = ToBytes(html);
            return doc;
        }

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
        //public async Task<IActionResult> GetPDF(Guid id)
        //{
        //    User user = null;

        //    SystemDocument doc = await _documentRepository.GetByIdAsync(id);
        //    string extension = "";
        //    var docContents = new byte[] { 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 };
        //    // DOCX & HTML
        //    string html = FromBytes(doc.Contents);


        //    var htmlToPdfConv = new NReco.PdfGenerator.HtmlToPdfConverter();
        //    htmlToPdfConv.License.SetLicenseKey(
        //       "PDF_Generator_Src_Examples_Pack_250473855326",
        //       "iES8O5aKZQacEPEDg3tX5ouIxQ7lmPUZ1QsTMppGWDF2jJ50HIVh1PwkigtKyxquPDKs8hdf5wm2Zn2CEjMUwquXiB3uRpPBWTIAlloLpaLAmYAQOFV7OVu2LXp5f1MWOd5Jg8PD2pEtX6n8c70rHsTLSAIGQDwSCNM4g7AOuQ4="
        //   );            // for Linux/OS-X: "wkhtmltopdf"

        //    var pdfBytes = htmlToPdfConv.GeneratePdf(html);

        //    return File(pdfBytes, "application/pdf", "FullProposalReport.pdf");
        //}

        #endregion
    }
}

