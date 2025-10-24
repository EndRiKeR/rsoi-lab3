using System.Security.Cryptography.X509Certificates;
using Common.DtoModels.BonusServiceDto;
using Common.DtoModels.FlightServiceDto;
using Common.DtoModels.TicketsServiceDto;
using Microsoft.Extensions.Primitives;

namespace Common.Fallbacks;

public class ControllersFallbacks
{
    public PaginationResponse GetFlightsFallback()
    {
        return new PaginationResponse
        {
            Page = 0,
            PageSize = 0,
            TotalElements = 0,
            Items = []
        };
    }
    
    public FlightResponse GetFlightDataFallback(string flightNumber)
    {
        return new FlightResponse
        {
            FlightNumber = flightNumber,
            FromAirport = "",
            ToAirport = "0",
            Date = DateTime.Now,
            Price = 0,
        };
    }
    
    public TicketResponse GetTicketsFallback(Guid ticketUid = new Guid())
    {
        return new TicketResponse
        {
            TicketUid = ticketUid,
            FlightNumber = "",
            FromAirport = "",
            ToAirport = "",
            Date = DateTime.Now,
            Price = 0,
            Status = "",
        };
    }
    
    public List<TicketResponse> GetAllTicketsFallback()
    {
        return new();
    }
    
    public PrivilegeShortInfo GetPrivilegeShortInfoFallback()
    {
        return new PrivilegeShortInfo
        {
            Balance = 0,
            Status = "BRONZE"
        };
    }
    
    public PrivilegeInfoResponse GetPrivilegeInfoResponseFallback()
    {
        return new PrivilegeInfoResponse
        {
            Balance = 0,
            Status = "BRONZE",
            History = new()
        };
    }
}