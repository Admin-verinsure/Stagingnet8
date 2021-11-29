using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using SystemDocument = DealEngine.Domain.Entities.Document;

namespace DealEngine.Services.Interfaces
{
	public interface IEmailService
	{
		string EmailEnabled { get; }
		string SmtpServer { get; }
		int SmtpPort { get; }
		string DefaultSender { get; }
		string SystemEmail { get; }
		string CatchAllEmail { get; }
        string BCCEmail { get; }
        string ReplyToEmail { get; }
        Task SendPasswordResetEmail(string recipent, Guid resetToken, string originDomain);
        Task SendEmailViaEmailTemplate(string recipent, EmailTemplate emailTemplate, List<SystemDocument> documents, ClientInformationSheet clientInformationSheet, ClientAgreement clientAgreement);
        Task SendEmailViaEmailTemplateWithCC(string recipent, EmailTemplate emailTemplate, List<SystemDocument> documents, ClientInformationSheet clientInformationSheet, ClientAgreement clientAgreement, string cCRecipent);
        Task SendPremiumAdviceEmail(string recipent, List<SystemDocument> documents, ClientInformationSheet clientInformationSheet, ClientAgreement clientAgreement, string recipentcc);
        Task MarshPleaseCallMe(string sender, string subject, string body);
        Task MarshRsaOneTimePassword(string sender, string subject);
        Task ContactSupport (string sender, string subject, string body);
        Task SendSystemEmailLogin(string recipent);
        Task SendDataEmail(string recipient, Data data);
        Task SendSystemPaymentSuccessConfigEmailUISIssueNotify(User uISIssuer, Programme programme, ClientInformationSheet sheet, Organisation insuredOrg);
        Task SendSystemPaymentFailConfigEmailUISIssueNotify(User uISIssuer, Programme programme, ClientInformationSheet sheet, Organisation insuredOrg);
        Task SendSystemFailedInvoiceConfigEmailUISIssueNotify(User uISIssuer, Programme programme, ClientInformationSheet sheet, Organisation insuredOrg);
        Task SendSystemSuccessInvoiceConfigEmailUISIssueNotify(User uISIssuer, Programme programme, ClientInformationSheet sheet, Organisation insuredOrg);
        Task SendSystemEmailUISIssueNotify(User UISIssuer, Programme programme, ClientInformationSheet sheet, Organisation insuredOrg);
        Task SendSystemEmailUISSubmissionConfirmationNotify(User uISIssued, Programme programme, ClientInformationSheet sheet, Organisation insuredOrg);
        Task SendSystemEmailUISSubmissionNotify(User uISIssued, Programme programme, ClientInformationSheet sheet, Organisation insuredOrg);
        Task SendSystemEmailAgreementReferNotify(User uISIssued, Programme programme, ClientAgreement agreement, Organisation insuredOrg);
        Task SendSystemEmailAgreementIssueNotify(User issuer, Programme programme, ClientAgreement agreement, Organisation insuredOrg);
        Task SendSystemEmailAgreementBoundNotify(User binder, Programme programme, ClientAgreement agreement, Organisation insuredOrg);
        Task SendSystemEmailOtherMarinaTCNotify(User uISIssued, Programme programme, ClientInformationSheet sheet, Organisation insuredOrg);
        Task SendSystemEmailUISUpdateNotify(User uISIssued, Programme programme, ClientInformationSheet sheet, Organisation insuredOrg);
        Task SendSystemEmailClientNumberNotify(User uISIssued, Programme programme, ClientInformationSheet sheet, Organisation insuredOrg);
        Task SendSystemEmailEGlobalTCNotify(string XMLBody);
        Task IssueToBrokerSendEmail(string recipent, string EmailContent, ClientInformationSheet clientInformationSheet, ClientAgreement clientAgreement, User sender);
        Task SendSystemEmailAllSubUISComplete(Organisation insuredOrg, Programme programme, ClientInformationSheet sheet);
        Task SendSystemEmailAllSubUISInstruction(Organisation insuredOrg, Programme programme, ClientInformationSheet sheet);
        Task SendFullProposalReport(string recipent, SystemDocument documents, ClientInformationSheet clientInformationSheet, ClientAgreement clientAgreement, string recipentcc);
        Task EmailHunterPremiumFunding(ClientProgramme clientProgramme);
        Task EmailPaymentFrequency(ClientProgramme clientProgramme);
        Task RsaLogEmail(string recipient, string loginUserUserName, string requestXML, string responseXML);
        Task EGlobalLogEmail(string recipient, string transactionreferenceid, string requestXML, string responseXML);
        Task JoinOrganisationEmail(User organisationUser);
        Task RemoveOrganisationUserEmail(User removedUser, User brokerContactUser, ClientInformationSheet sheet);
        Task RsaNotificationEmail(string recipient, string rsausername);
    }
}

