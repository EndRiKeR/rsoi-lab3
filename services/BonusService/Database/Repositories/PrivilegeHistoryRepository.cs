using BonusService.Database.Models;
using BonusService.Database.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BonusService.Database.Repositories;

public class PrivilegeHistoryRepository : IPrivilegeHistoryRepository
{
    private readonly PrivilegeContext _context;
    
    public PrivilegeHistoryRepository(PrivilegeContext context)
    {
        _context = context;
    }
    
    public async Task<List<PrivilegeHistory>> GetAll()
    {
        try
        {
            return await _context.PrivilegeHistories
                .Include(ph => ph.Privilege)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<PrivilegeHistory> GetById(long id)
    {
        try
        {
            var history = await _context.PrivilegeHistories
                .Include(ph => ph.Privilege)
                .FirstOrDefaultAsync(ph => ph.Id == id);
            
            if (history == null)
                throw new Exception($"PrivilegeHistory with id {id} not found");
            
            return history;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<PrivilegeHistory> Add(PrivilegeHistory privilegeHistory)
    {
        try
        {
            var exists = await _context.PrivilegeHistories.AnyAsync(ph => ph.Id == privilegeHistory.Id);

            if (exists)
            {
                return await Update(privilegeHistory);
            }

            var newHistory = await _context.PrivilegeHistories.AddAsync(privilegeHistory);
            
            await _context.SaveChangesAsync();
        
            return newHistory.Entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<PrivilegeHistory>> AddList(List<PrivilegeHistory> addHistories)
    {
        try
        {
            var existingIds = await _context.PrivilegeHistories
                .Where(ph => addHistories.Select(x => x.Id).Contains(ph.Id))
                .Select(ph => ph.Id)
                .ToListAsync();

            if (existingIds.Any())
                throw new Exception($"PrivilegeHistories with ids {string.Join(", ", existingIds)} already exist");

            await _context.PrivilegeHistories.AddRangeAsync(addHistories);
            await _context.SaveChangesAsync();
            
            return addHistories;
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
            var history = await _context.PrivilegeHistories.FirstOrDefaultAsync(ph => ph.Id == id);
            
            if (history == null)
                throw new Exception($"PrivilegeHistory with id {id} not found");
            
            _context.PrivilegeHistories.Remove(history);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<PrivilegeHistory> Update(PrivilegeHistory privilegeHistory)
    {
        try
        {
            var existingHistory = await _context.PrivilegeHistories.FirstOrDefaultAsync(ph => ph.Id == privilegeHistory.Id);
            
            if (existingHistory == null)
                throw new Exception($"PrivilegeHistory with id {privilegeHistory.Id} not found");

            existingHistory.PrivilegeId = privilegeHistory.PrivilegeId;
            existingHistory.TicketUid = privilegeHistory.TicketUid;
            existingHistory.Datetime = privilegeHistory.Datetime;
            existingHistory.BalanceDiff = privilegeHistory.BalanceDiff;
            existingHistory.OperationType = privilegeHistory.OperationType;

            await _context.SaveChangesAsync();
            
            return existingHistory;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<PrivilegeHistory>> GetByPrivilegeId(long privilegeId)
    {
        try
        {
            return await _context.PrivilegeHistories
                .Include(ph => ph.Privilege)
                .Where(ph => ph.PrivilegeId == privilegeId)
                .OrderByDescending(ph => ph.Datetime)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<PrivilegeHistory>> GetByTicketUid(Guid ticketUid)
    {
        try
        {
            return await _context.PrivilegeHistories
                .Include(ph => ph.Privilege)
                .Where(ph => ph.TicketUid == ticketUid)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<PrivilegeHistory>> GetByDateRange(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _context.PrivilegeHistories
                .Include(ph => ph.Privilege)
                .Where(ph => ph.Datetime >= startDate && ph.Datetime <= endDate)
                .OrderByDescending(ph => ph.Datetime)
                .ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}