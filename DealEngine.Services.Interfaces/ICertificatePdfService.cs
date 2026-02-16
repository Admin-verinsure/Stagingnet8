using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealEngine.Services.Interfaces
{
    public interface ICertificatePdfService
    {
        /// <summary>
        /// Generates certificate PDF as byte array from aggregate model.
        /// </summary>
        Task<byte[]> GenerateAsync(CertificateAggregateModel model);
        
    }
}
