using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Http;


namespace DealEngine.Services.Interfaces
{
    public interface ILocalTaskRepository
    {
        Task<int> CreateAsync(OdooTask task, CancellationToken ct = default);
        Task SetExternalOdooIdAsync(int localId, int odooTaskId, CancellationToken ct = default);
    }
}
