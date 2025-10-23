using BonusService.Database.Models;
using BonusService.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BonusService.Database.Repositories;

public class PrivilegeRepository : IPrivilegeRepository
{
    private readonly PrivilegeContext _context;
    
    public PrivilegeRepository(PrivilegeContext context)
    {
        _context = context;
    }
    
    public async Task<List<Privilege>> GetAll()
    {
        try
        {
            return await _context.Privileges
                .Include(p => p.History)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Privilege> GetById(long id)
    {
        try
        {
            var privilege = await _context.Privileges
                .Include(p => p.History)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (privilege == null)
                throw new Exception($"Privilege with id {id} not found");
            
            return privilege;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Privilege> Add(Privilege privilege)
    {
        try
        {
            var exists = await _context.Privileges.AnyAsync(p => p.Id == privilege.Id);
            
            if (exists)
                throw new Exception("Privilege already exists");
            
            var newPrivilege = await _context.Privileges.AddAsync(privilege);
            await _context.SaveChangesAsync();
            
            return newPrivilege.Entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<Privilege>> AddList(List<Privilege> addPrivileges)
    {
        try
        {
            var existingIds = await _context.Privileges
                .Where(p => addPrivileges.Select(x => x.Id).Contains(p.Id))
                .Select(p => p.Id)
                .ToListAsync();

            if (existingIds.Any())
                throw new Exception($"Privileges with ids {string.Join(", ", existingIds)} already exist");

            await _context.Privileges.AddRangeAsync(addPrivileges);
            await _context.SaveChangesAsync();
            
            return addPrivileges;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task Delete(long id)
    {
        try
        {
            var privilege = await _context.Privileges.FirstOrDefaultAsync(p => p.Id == id);
            
            if (privilege == null)
                throw new Exception($"Privilege with id {id} not found");
            
            _context.Privileges.Remove(privilege);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Privilege> Update(Privilege privilege)
    {
        try
        {
            var existingPrivilege = await _context.Privileges.FirstOrDefaultAsync(p => p.Id == privilege.Id);
            
            if (existingPrivilege == null)
                throw new Exception($"Privilege with id {privilege.Id} not found");

            existingPrivilege.Username = privilege.Username;
            existingPrivilege.Status = privilege.Status;
            existingPrivilege.Balance = privilege.Balance;

            await _context.SaveChangesAsync();
            
            return existingPrivilege;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Privilege> GetByUsername(string username)
    {
        try
        {
            var privilege = await _context.Privileges
                .Include(p => p.History)
                .FirstOrDefaultAsync(p => p.Username == username);
            
            if (privilege == null)
                throw new Exception($"Privilege with username {username} not found");
            
            return privilege;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<bool> ExistsByUsername(string username)
    {
        try
        {
            return await _context.Privileges.AnyAsync(p => p.Username == username);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task UpdateBalance(long privilegeId, int balanceDiff)
    {
        try
        {
            var privilege = await _context.Privileges.FirstOrDefaultAsync(p => p.Id == privilegeId);
            
            if (privilege == null)
                throw new Exception($"Privilege with id {privilegeId} not found");

            privilege.Balance = (privilege.Balance ?? 0) + balanceDiff;
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}