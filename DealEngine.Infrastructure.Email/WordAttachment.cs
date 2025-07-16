using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlToOpenXml;
using System.IO;
using System.Net.Mail;



namespace DealEngine.Infrastructure.Email
{
	public class WordAttachment : Attachment
	{
		public WordAttachment (string filename, byte[] content)
			: base (new MemoryStream(content), filename)
		{
			// This produces invalid docx files for some reason...
			//string html = System.Text.Encoding.UTF8.GetString (content);
			//using (MemoryStream virtualFile = new MemoryStream ()) {
			//	using (WordprocessingDocument wordDocument = WordprocessingDocument.Create (virtualFile, WordprocessingDocumentType.Document)) {
			//		// Add a main document part. 
			//		MainDocumentPart mainPart = wordDocument.AddMainDocumentPart ();
			//		new Document (new Body ()).Save (mainPart);
			//		HtmlConverter converter = new HtmlConverter (mainPart);
			//		converter.ImageProcessing = ImageProcessing.AutomaticDownload;
			//		converter.ParseHtml (html);
			//	}
			//	byte [] bytes = virtualFile.ToArray ();
			//	ContentStream.Write (bytes, 0, bytes.Length);
			//}
		}

		public static WordAttachment FromHtml (string filename, byte [] content)
		{
			string html = System.Text.Encoding.UTF8.GetString (content);
			using (MemoryStream virtualFile = new MemoryStream ()) {
				using (WordprocessingDocument wordDocument = WordprocessingDocument.Create (virtualFile, WordprocessingDocumentType.Document)) {
					// Add a main document part. 
					MainDocumentPart mainPart = wordDocument.AddMainDocumentPart ();
					new Document (new Body ()).Save (mainPart);
                    HtmlConverter converter = new HtmlConverter (mainPart);
					//converter.ImageProcessing = ImageProcessing.AutomaticDownload;
					converter.ParseHtml (html);
				}
				byte [] bytes = virtualFile.ToArray ();
				return new WordAttachment(filename, bytes);
			}
		}
	}
}

