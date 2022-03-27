using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DealEngine.Services.Interfaces
{
    public interface IReportBuilderService
    {
        Task<List<List<string>>> GetReportView(Guid ProgrammeId, string IsReport);

    }
}
