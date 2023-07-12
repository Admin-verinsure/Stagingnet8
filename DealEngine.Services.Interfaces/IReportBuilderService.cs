using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DealEngine.Services.Interfaces
{
    public interface IReportBuilderService
    {
        Task GetFile(Guid ProgrammeId, string IsReport);
        //Task<MemoryStream, string, string> GetReportView(Guid ProgrammeId, string IsReport);

    }
}
