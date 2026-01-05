using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DealEngine.Domain.Entities;

namespace DealEngine.Services.Interfaces
{
    public interface IOdooTaskGateway
    {

        Task<int> OdooGatewayconnection(string OdooServerworkingendpoint, string OdooServerDB, string LoginID, string LoginKey);
      //  Task<int> CreateTaskAsync(string title, int projectId, string? notes = null, DateTime? deadline = null, int? assigneeUserId = null,
                              // IEnumerable<int>? tagIds = null);
       // Task<int?> GetUserIdByEmailAsync(string email);
       Task<int[]> CreateTasksAsync(IEnumerable<OdooTaskSpec> tasks);

        Task<string> SendInvoiceAsync(object rpcPayload);

        Task<string> SendInvoiceAsync(
        ClientInformationSheet sheet,
        ClientProgramme programme,
        decimal invoiceAmount);

    }
}
