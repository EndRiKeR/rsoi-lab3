using BonusService.Database.Models;
using Common.Interfaces;

namespace BonusService.Database.Repositories.Interfaces;

public interface IPrivilegeRepository : IRepository<Privilege>
{
    Task<Privilege> GetByUsername(string username);
    Task<bool> ExistsByUsername(string username);
    Task UpdateBalance(long privilegeId, int balanceDiff);
}