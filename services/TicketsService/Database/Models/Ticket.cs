using TicketsService.Database.Enums;

namespace TicketsService.Database.Models;

public class Ticket
{
    public long Id { get; set; }
    public Guid TicketUid { get; set; }
    public string Username { get; set; }
    public string FlightNumber { get; set; }
    public int Price { get; set; }
    public TicketStatus Status { get; set; }
}