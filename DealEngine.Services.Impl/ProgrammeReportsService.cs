using NHibernate.Linq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using DealEngine.Infrastructure.FluentNHibernate;
using DealEngine.Services.Interfaces;
//using System.Linq.Dynamic.Core;

namespace DealEngine.Services.Impl
{
    public class ProgrammeReportsService : IProgrammeReportsService
    {        
        IMapperSession<ProgrammeReports> _programmeReports;

        public ProgrammeReportsService( IMapperSession<ProgrammeReports> programmeReports)
        {
            _programmeReports = programmeReports;
        }

       

        public async Task<List<ProgrammeReports>> GetAllProgrammeReports()
        {
           
           var allProgrammeReports = await _programmeReports.FindAll().ToListAsync();
           
            return allProgrammeReports;
        }

        public async Task<ProgrammeReports> GetProgrammeReportsById(Guid Id)
        {
            return await _programmeReports.GetByIdAsync(Id);
        }

    }
}
