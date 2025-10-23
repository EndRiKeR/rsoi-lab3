using Microsoft.EntityFrameworkCore;
using TicketsService.Database.Models;
using TicketsService.Database.Repositories.Interfaces;

namespace TicketsService.Database.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly TicketsContext _context;
    
    public TicketRepository(TicketsContext context)
    {
        _context = context;
    }
    
    public async Task<List<Ticket>> GetAll()
    {
        try
        {
            return await _context.Tickets.ToListAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Ticket> GetById(long id)
    {
        try
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            
            if (ticket == null)
                throw new Exception($"Ticket with id {id} not found");
            
            return ticket;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Ticket> GetByTicketUid(Guid ticketUid)
    {
        try
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketUid == ticketUid);
            
            if (ticket == null)
                throw new Exception($"Ticket with UID {ticketUid} not found");
            
            return ticket;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Ticket> Add(Ticket ticket)
    {
        try
        {
            var existingTicket = await _context.Tickets
                .FirstOrDefaultAsync(t => t.TicketUid == ticket.TicketUid);
            
            if (existingTicket != null)
                throw new Exception($"Ticket with UID {ticket.TicketUid} already exists");

            var newTicket = await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();
            
            return newTicket.Entity;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<List<Ticket>> AddList(List<Ticket> addTickets)
    {
        try
        {
            var ticketUids = addTickets.Select(t => t.TicketUid).ToList();
            var existingTickets = await _context.Tickets
                .Where(t => ticketUids.Contains(t.TicketUid))
                .Select(t => t.TicketUid)
                .ToListAsync();

            if (existingTickets.Any())
                throw new Exception($"Tickets with UIDs {string.Join(", ", existingTickets)} already exist");

            await _context.Tickets.AddRangeAsync(addTickets);
            await _context.SaveChangesAsync();
            
            return addTickets;
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
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == id);
            
            if (ticket == null)
                throw new Exception($"Ticket with id {id} not found");
            
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task DeleteByTicketUid(Guid ticketUid)
    {
        try
        {
            var ticket = await _context.Tickets.FirstOrDefaultAsync(t => t.TicketUid == ticketUid);
            
            if (ticket == null)
                throw new Exception($"Ticket with UID {ticketUid} not found");
            
            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public async Task<Ticket> Update(Ticket ticket)
    {
        try
        {
            var existingTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.Id == ticket.Id);
            
            if (existingTicket == null)
                throw new Exception($"Ticket with id {ticket.Id} not found");

            existingTicket.Status = ticket.Status;
            existingTicket.Price = ticket.Price;
            existingTicket.FlightNumber = ticket.FlightNumber;
            existingTicket.Username = ticket.Username;
            existingTicket.TicketUid = ticket.TicketUid;

            await _context.SaveChangesAsync();
            
            return existingTicket;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}