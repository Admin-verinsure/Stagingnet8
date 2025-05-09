using System;
using DealEngine.Services.Interfaces;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using System.Threading.Tasks;
using NHibernate.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace DealEngine.Services.Impl
{
	public class TermBuilderService : ITermBuilderService
	{
		IMapperSession<PolicyTermSection> _policyTermRepository;

		#region ITermBuilderService implementation

		public TermBuilderService(IMapperSession<PolicyTermSection> policyTermRepository)
		{
			_policyTermRepository = policyTermRepository;
		}

		public async Task<PolicyTermSection> Create (User createdBy, string name, string description, string version, int revision, string content, Guid creator, Guid territory, Guid jurisdiction)
		{
			PolicyTermSection section = new PolicyTermSection (createdBy);
			section.Clause = Guid.Empty;
			section.Content = content;
			section.Creator = creator;
			section.Description = description;
			section.Jurisdiction = jurisdiction;
			section.Name = name;
			section.Owner = section.Creator;	// for now
			section.Revision = revision;
			section.Territory = territory;
			section.Version = version;

            await _policyTermRepository.AddAsync(section);

            return section;
		}

		public async Task<PolicyTermSection> GetTerm (Guid termId)
		{
			return await _policyTermRepository.GetByIdAsync(termId);
		}

		public async Task<List<PolicyTermSection>> GetTerms()
		{
            return await _policyTermRepository.FindAll().ToListAsync();
		}

		public async Task<List<PolicyTermSection>> GetTerms(string orderField, string direction)
		{
			return await _policyTermRepository.FindAll().OrderBy(orderField + " " + direction).ToListAsync();
		}

		public async Task<bool> Deprecate (User deletedBy, Guid termId)
		{
			PolicyTermSection term = await GetTerm(termId);
            await _policyTermRepository.RemoveAsync(term);
            var terms = await GetTerm(termId);

            return terms.DateDeleted != null;
		}

		#endregion
		
	}
}

