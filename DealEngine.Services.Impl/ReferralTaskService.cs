using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;
using System.Linq.Dynamic.Core;

namespace DealEngine.Services.Impl
{
    public sealed class ReferralTaskService
    {
        private readonly ILocalTaskRepository _repo;
        private readonly IOdooTaskGateway _odoo;

        public ReferralTaskService(ILocalTaskRepository repo, IOdooTaskGateway odoo)
        { _repo = repo; _odoo = odoo; }

        public async Task<int> CreateLocalAndMirrorAsync(CreateTaskDto dto)
        {
            var localId = await _repo.CreateAsync(new OdooTask
            {
                Title = dto.Title,
                Notes = dto.Notes
               // Due = dto.DueDate,
               // ReferralSelected = dto.ReferralSelected
            });

            if (dto.ReferralSelected)
            {
                int? assigneeId = null; // or await _odoo.GetUserIdByEmailAsync("manish@verinsure.online");
                var odooTaskId = await _odoo.CreateTaskAsync(dto.Title, projectId: 33, // ← your Odoo project_id
                                                             notes: dto.Notes, deadline: dto.DueDate,
                                                             assigneeUserId: assigneeId);
                await _repo.SetExternalOdooIdAsync(localId, odooTaskId);
            }
            return localId;
        }
    }

    public sealed class CreateTaskDto
    {
        public string Title { get; init; } = default!;
        public string? Notes { get; init; }
        public DateTime? DueDate { get; init; }
        public bool ReferralSelected { get; init; }
    }
}
