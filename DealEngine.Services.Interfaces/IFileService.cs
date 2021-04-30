using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using System;

namespace DealEngine.Services.Interfaces
{
	public interface IFileService
	{
		string FileDirectory { get; }

		bool IsApplication (byte[] buffer, string contentType, string fileName);

		bool IsImageFile (byte[] buffer, string contentType, string fileName);

		bool IsTextFile (byte[] buffer, string contentType, string fileName);

		Task UploadFile (Document document);

		Task UploadFile (Image image);

        Task<Document> GetDocument (string documentName);

		Task<Document> GetDocumentByID(Guid documentID);

		Task<Image> GetImage (string imageName);

        Task<T> RenderDocument<T> (User renderedBy, T template, ClientAgreement agreement, ClientInformationSheet clientInformation) where T : Document;

		Task<Document> ConvertHTMLToPDF(Document document);

		Task<Document> FormatCKHTMLforConversion(Document document);
		byte [] ToBytes (string contents);
		string FromBytes (byte [] bytes);
        Task<Document> GetDocumentByType(Organisation primaryOrganisation, int documentType);
		Task<List<Document>> GetDocumentByOwner(Organisation Owner);
		//Task<IActionResult> GetPDF(Guid id);
	}
}

