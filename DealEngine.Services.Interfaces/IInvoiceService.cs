using DealEngine.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DealEngine.Services.Interfaces.Models;

namespace DealEngine.Services.Interfaces
{
    public interface IInvoiceService
    {
        Task<InvoiceGenerationResult> GenerateInvoiceAsync( ClientInformationSheet sheet, ClientProgramme programme);

        Task<InvoiceGenerationResult> SendInvoicePayloadPOC(
            ClientInformationSheet sheet,
            ClientProgramme programme,
            decimal materialDamageQty,
            decimal globalGuardPremium,
            decimal adminFeeQty);

    }
}
