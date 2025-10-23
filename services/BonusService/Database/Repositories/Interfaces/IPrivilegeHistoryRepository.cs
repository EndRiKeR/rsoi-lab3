using BonusService.Database.Models;
using Common.Interfaces;

namespace BonusService.Database.Repositories.Interfaces;

public interface IPrivilegeHistoryRepository : IRepository<PrivilegeHistory>
{
    Task<List<PrivilegeHistory>> GetByPrivilegeId(long privilegeId);
    Task<List<PrivilegeHistory>> GetByTicketUid(Guid ticketUid);
    Task<List<PrivilegeHistory>> GetByDateRange(DateTime startDate, DateTime endDate);
}