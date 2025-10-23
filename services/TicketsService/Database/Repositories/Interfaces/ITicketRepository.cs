using Common.Interfaces;
using TicketsService.Database.Models;

namespace TicketsService.Database.Repositories.Interfaces;

public interface ITicketRepository : IRepository<Ticket>
{
    Task DeleteByTicketUid(Guid ticketUid);
    Task<Ticket> GetByTicketUid(Guid ticketUid);
}