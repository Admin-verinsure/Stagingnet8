using DealEngine.Domain.Entities;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DealEngine.Infrastructure.FluentNHibernate
{
    public interface IMapperSession<TEntity> where TEntity : class
    {
        IQueryable<TEntity> FindAll();
        Task<TEntity> GetByIdAsync(string id);
        Task<TEntity> GetByIdAsync(Guid id);
        Task RemoveAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task AddAsync(TEntity entity);
        //Task SaveAsync(TEntity entity);
        Task<List<Object>> QueryHQLAsync(string query);
        Task<bool> GetStoredProcedure(Guid Id, string BoatName);


    }
}
