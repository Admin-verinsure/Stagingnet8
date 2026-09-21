using DealEngine.Services.Interfaces;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Domain.Entities;
using System.Threading.Tasks;
using NHibernate.Linq;
using System;
using AutoMapper;

namespace DealEngine.Services.Impl
{
    public class CloneService : ICloneService
    {
        public Profile GetCloneProfile()
        {
            return CloneProfile(); 
        }

        private Profile CloneProfile()
        {
            return new CloneProfile();
        }

        public Profile GetSerialiseProfile()
        {
            return SerialiseProfile();
        }

        private Profile SerialiseProfile()
        {
            return new SerialiseProfile();
        }
    }
}

public class CloneProfile : Profile
{
    public CloneProfile()
    {        
        //subsystem            
        CreateMap<InformationTemplate, SubInformationTemplate>()
            .ForMember(dest => dest.BaseInformationTemplate, map => map.Ignore())
            .ForMember(dest => dest.Sections, map => map.Ignore())
            .ForMember(dest => dest.Name, map => map.MapFrom(t => t.Name + " Sub"))
            .ForMember(dest => dest.Id, map => map.Ignore());
        CreateMap<ClientInformationSheet, SubClientInformationSheet>()
            .ForMember(dest => dest.BaseClientInformationSheet, map => map.Ignore())
            .ForMember(dest => dest.Id, map => map.Ignore())
            .ForMember(dest => dest.SubClientInformationSheets, map => map.Ignore())
            .ForMember(dest => dest.Answers, map => map.Ignore())
            .ForMember(dest => dest.RoleData, map => map.Ignore())
            .ForMember(dest => dest.Programme, map => map.Ignore())
            .ForMember(dest => dest.ClaimNotifications, map => map.Ignore())
            .ForMember(dest => dest.ReferenceId, map => map.Ignore())
            .ForMember(dest => dest.PreviousInformationSheet, map => map.Ignore())
            .ForMember(dest => dest.CreatedBy, map => map.Ignore())
            .ForMember(dest => dest.SubmittedBy, map => map.Ignore())
            .ForMember(dest => dest.RevenueData, map => map.Ignore())
            .ForMember(dest => dest.NextInformationSheet, map => map.Ignore())
            .ForMember(dest => dest.BusinessContracts, map => map.Ignore())
            .ForMember(dest => dest.Organisation, map => map.Ignore());
        CreateMap<ClientProgramme, SubClientProgramme>()
            .ForMember(dest => dest.BaseClientProgramme, map => map.Ignore())
            .ForMember(dest => dest.BrokerContactUser, map => map.Ignore())
            .ForMember(dest => dest.EGlobalClientStatus, map => map.Ignore())
            .ForMember(dest => dest.SubClientProgrammes, map => map.Ignore())
            .ForMember(dest => dest.ChangeReason, map => map.Ignore())
            .ForMember(dest => dest.Products, map => map.Ignore())
            .ForMember(dest => dest.ClientProgrammeMembershipNumber, map => map.Ignore())
            .ForMember(dest => dest.CreatedBy, map => map.Ignore())
            .ForMember(dest => dest.Payment, map => map.Ignore())
            .ForMember(dest => dest.InformationSheet, map => map.Ignore())
            .ForMember(dest => dest.ClientAgreementEGlobalResponses, map => map.Ignore())
            .ForMember(dest => dest.ClientAgreementEGlobalSubmissions, map => map.Ignore())
            .ForMember(dest => dest.Agreements, map => map.Ignore())
            .ForMember(dest => dest.Id, map => map.Ignore());
        CreateMap<InformationSection, InformationSection>()
            .ForMember(dest => dest.Id, map => map.Ignore());

        //clonesystem
        CreateMap<ClientInformationSheet, ClientInformationSheet>()
            .ForMember(dest => dest.Id, map => map.Ignore())
            .ForMember(dest => dest.SubmittedBy, map => map.Ignore())
            .ForMember(dest => dest.SubClientInformationSheets, map => map.Ignore())
            .ForMember(dest => dest.SubmitDate, map => map.Ignore());

    }
}
public class SerialiseProfile : Profile
{
    public SerialiseProfile()
    {
        CreateMap<Programme, Programme>()
            .ForMember(dest => dest.Id, map => map.Ignore())
            .ForMember(dest => dest.AgreementBoundNotifyUsers, map => map.Ignore())
            .ForMember(dest => dest.AgreementIssueNotifyUsers, map => map.Ignore())
            .ForMember(dest => dest.AgreementReferNotifyUsers, map => map.Ignore())
            .ForMember(dest => dest.BusinessActivityTemplates, map => map.Ignore())
            .ForMember(dest => dest.ClientProgrammes, map => map.Ignore())
            .ForMember(dest => dest.Products, map => map.Ignore())
            .ForMember(dest => dest.SharedDataRoleTemplates, map => map.Ignore())
            .ForMember(dest => dest.EmailTemplates, map => map.Ignore())
            .ForMember(dest => dest.TerritoryTemplates, map => map.Ignore());
    }
}