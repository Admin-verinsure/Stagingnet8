using System;
using DealEngine.Domain.Entities;
using FluentNHibernate.Mapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;

namespace DealEngine.Infrastructure.FluentNHibernate.MappingOverrides
{

    public class ClientProgrammeMappingOverride : IAutoMappingOverride<ClientProgramme>
    {
        public void Override(AutoMapping<ClientProgramme> mapping)
        {
            mapping.References(p => p.InformationSheet).Not.LazyLoad();
            mapping.References(n => n.BaseProgramme).Not.LazyLoad();
            mapping.Map(p => p.InvoiceGenerationFailed).Column("InvoiceSentToOdoo");
        }
    }

}