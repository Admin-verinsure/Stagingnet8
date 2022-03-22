using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface IProgrammeReportsService
    {
        Task<List<ProgrammeReports>> GetAllProgrammeReports();
        Task<ProgrammeReports> GetProgrammeReportsById(Guid Id);

    }

}
