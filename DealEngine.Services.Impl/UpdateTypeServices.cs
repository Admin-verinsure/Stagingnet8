using System;
using System.Collections.Generic;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Infrastructure.Ldap.Interfaces;
using DealEngine.Services.Interfaces;
using NHibernate.Linq;
using System.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace DealEngine.Services.Impl
{
    public class UpdateTypeServices : IUpdateTypeService
    {
		IMapperSession<UpdateType> _updateType;

		public UpdateTypeServices(IMapperSession<UpdateType> updateType){ _updateType = updateType;}
		public async Task AddUpdateType (User createdBy, string nameType, string valueType, bool typeIsTc, bool typeIsBroker, bool typeIsInsurer, bool typeIsClient)
        {
			if (string.IsNullOrWhiteSpace(nameType.ToString()))
				throw new ArgumentNullException(nameof(nameType));
			if (string.IsNullOrWhiteSpace(nameType.ToString()))
				throw new ArgumentNullException(nameof(valueType));
			await _updateType.AddAsync(new UpdateType(createdBy, nameType, valueType, typeIsTc, typeIsBroker, typeIsInsurer, typeIsClient));

		}
		public async Task<List<UpdateType>> GetAllUpdateTypes()
		{
			return await _updateType.FindAll().ToListAsync();
		}
		public async Task<UpdateType> GetUpdateType(Guid updateTypeId)
		{
			return await _updateType.GetByIdAsync(updateTypeId);
		}


	}
}
