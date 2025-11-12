using DealEngine.Domain.Entities;
using DealEngine.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealEngine.Services.Impl
{
    internal class OdooTaskGateway : IOdooTaskGateway
    {
        public Task<int> CreateTaskAsync(string title, int projectId, string? notes = null, DateTime? deadline = null, int? assigneeUserId = null, IEnumerable<int>? tagIds = null)
        {
            throw new NotImplementedException();
        }
        public Task<int> OdooGatewayconnection(string OdooServerworkingendpoint, string OdooServerDB, string  LoginID, string LoginKey)
        { 
        
             OdooTask OdooTask = new OdooTask{
                 EndPoint= OdooServerworkingendpoint,
                 OdooDB = OdooServerDB,
                 LoginID=LoginID,
                 LoginKey=LoginKey
             };
            throw new NotImplementedException();
        }
        public Task<int?> GetUserIdByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }
    }
}
