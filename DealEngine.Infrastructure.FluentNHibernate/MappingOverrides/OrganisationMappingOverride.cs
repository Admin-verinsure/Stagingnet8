using System;
using DealEngine.Domain.Entities;
using FluentNHibernate.Mapping;
using FluentNHibernate.Automapping.Alterations;
using FluentNHibernate.Automapping;

namespace DealEngine.Infrastructure.FluentNHibernate.MappingOverrides
{

    public class OrganisationMappingOverride : IAutoMappingOverride<Organisation>
    {
        public void Override(AutoMapping<Organisation> mapping)
        {
            mapping.Id(p => p.Id).GeneratedBy.Assigned();
            mapping.References(n => n.OrganisationType).Not.LazyLoad();
            mapping.HasOne(x => x.OrganisationAttribute)
             .Cascade.All()
             .PropertyRef(x => x.Organisation);
        }
    }

    public class PlannerUnitMappingOverride : IAutoMappingOverride<PlannerUnit>
    {
        public void Override(AutoMapping<PlannerUnit> mapping)
        {
            mapping.Map(x => x.Qualifications).Length(10000);
        }
    }

    public class MarinaMappingOverride : IAutoMappingOverride<MarinaUnit>
    {
        public void Override(AutoMapping<MarinaUnit> mapping)
        {
            mapping.References(p => p.WaterLocation).Not.LazyLoad();
        }
    }
    public class InterestedPartyMappingOverride : IAutoMappingOverride<InterestedPartyUnit>
    {
        public void Override(AutoMapping<InterestedPartyUnit> mapping)
        {
            mapping.References(p => p.Location).Not.LazyLoad();
        }
    }
}