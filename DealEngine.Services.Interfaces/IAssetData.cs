using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;
using System.Threading.Tasks;
using DealEngine.Services.Interfaces.Models;

namespace DealEngine.Services.Interfaces
{
    public interface IAssetData
    {
        public interface ICertificatePdfService
        {
            Task<byte[]> GenerateAsync(CertificateAggregateModel model);
        }

    }
}
