using BonusService.Database;
using BonusService.Database.Models;
using BonusService.Database.Repositories;
using FlightService.Database;
using FlightService.Database.Models;
using FlightService.Database.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using TicketsService.Database;
using TicketsService.Database.Enums;
using TicketsService.Database.Models;
using TicketsService.Database.Repositories;
using Xunit;

namespace Services.UnitTests;

public class RepositoriesUnitTests
{
    private readonly TicketRepository _ticketRepository;
    private readonly FlightRepository _flightRepository;
    private readonly AirportRepository _airportRepository;
    private readonly PrivilegeRepository _privilegeRepository;
    private readonly PrivilegeHistoryRepository _privilegeHistoryRepository;
    
    private readonly Mock<TicketsContext> _ticketsMockContext = new();
    private readonly Mock<FlightContext> _flightMockContext = new();
    private readonly Mock<PrivilegeContext> _privilegeMockContext = new();

    public RepositoriesUnitTests()
    {
        _ticketRepository = new TicketRepository(_ticketsMockContext.Object);
        _flightRepository = new FlightRepository(_flightMockContext.Object);
        _airportRepository = new AirportRepository(_flightMockContext.Object);
        _privilegeRepository = new PrivilegeRepository(_privilegeMockContext.Object);
        _privilegeHistoryRepository = new PrivilegeHistoryRepository(_privilegeMockContext.Object);
    }

    [Fact]
    public async Task TicketsRepository_GetAllTickets()
    {
        // Arrange
        var tickets = new List<Ticket>
        {
            new() { Id = 1, TicketUid = Guid.NewGuid(), Username = "user1", FlightNumber = "FL001", Price = 1000, Status = TicketStatus.PAID },
            new() { Id = 2, TicketUid = Guid.NewGuid(), Username = "user2", FlightNumber = "FL002", Price = 2000, Status = TicketStatus.PAID }
        };

        _ticketsMockContext.Setup(c => c.Tickets).ReturnsDbSet(tickets);

        // Act
        var result = await _ticketRepository.GetAll();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(tickets, result);
    }
    
    [Fact]
    public async Task TicketsRepository_GetById()
    {
        // Arrange
        var ticketId = 1L;
        var tickets = new List<Ticket>
        {
            new() { Id = 1, TicketUid = Guid.NewGuid(), Username = "user1", FlightNumber = "FL001", Price = 1000, Status = TicketStatus.PAID },
            new() { Id = 2, TicketUid = Guid.NewGuid(), Username = "user2", FlightNumber = "FL002", Price = 2000, Status = TicketStatus.PAID }
        };

        _ticketsMockContext.Setup(c => c.Tickets).ReturnsDbSet(tickets);

        // Act
        var result = await _ticketRepository.GetById(ticketId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticketId, result.Id);
    }

    [Fact]
    public async Task TicketsRepository_GetByTicketUid()
    {
        // Arrange
        var ticketUid = Guid.NewGuid();
        var tickets = new List<Ticket>
        {
            new() { Id = 1, TicketUid = ticketUid, Username = "user1", FlightNumber = "FL001", Price = 1000, Status = TicketStatus.PAID }
        };

        _ticketsMockContext.Setup(c => c.Tickets).ReturnsDbSet(tickets);

        // Act
        var result = await _ticketRepository.GetByTicketUid(ticketUid);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(ticketUid, result.TicketUid);
    }
    
    [Fact]
    public async Task TicketsRepository_Update()
    {
        // Arrange
        var ticketUid = Guid.NewGuid();
        var tickets = new List<Ticket>
        {
            new() { Id = 1, TicketUid = ticketUid, Username = "user1", FlightNumber = "FL001", Price = 1000, Status = TicketStatus.PAID }
        };

        var updatedTicket = new Ticket() { Id = 1, TicketUid = ticketUid, Username = "user1", FlightNumber = "FL001", Price = 1000, Status = TicketStatus.CANCELED };

        _ticketsMockContext.Setup(c => c.Tickets).ReturnsDbSet(tickets);
        _ticketsMockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _ticketRepository.Update(updatedTicket);

        // Assert
        Assert.Equal(updatedTicket.Status, result.Status);
    }

    [Fact]
    public async Task TicketsRepository_DeleteByTicketUid()
    {
        // Arrange
        var ticketUid = Guid.NewGuid();
        var tickets = new List<Ticket>
        {
            new() { Id = 1, TicketUid = ticketUid, Username = "user1", FlightNumber = "FL001", Price = 1000, Status = TicketStatus.PAID }
        };

        _ticketsMockContext.Setup(c => c.Tickets).ReturnsDbSet(tickets);
        _ticketsMockContext.Setup(c => c.Tickets.Remove(It.IsAny<Ticket>()))
            .Callback<Ticket>(t => tickets.Remove(t));
        _ticketsMockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _ticketRepository.DeleteByTicketUid(ticketUid);

        // Assert
        Assert.Empty(tickets);
    }
    
    [Fact]
    public async Task FlightsRepository_GetFlightsByFlightNumber()
    {
        // Arrange
        var flightNumber = "FL001";
        var flights = new List<Flight>
        {
            new() { Id = 1, FlightNumber = flightNumber, DateTime = DateTime.UtcNow, Price = 1000 },
            new() { Id = 2, FlightNumber = "FL002", DateTime = DateTime.UtcNow, Price = 2000 },
            new() { Id = 3, FlightNumber = flightNumber, DateTime = DateTime.UtcNow.AddDays(1), Price = 1500 }
        };

        _flightMockContext.Setup(c => c.Flights).ReturnsDbSet(flights);

        // Act
        var result = await _flightRepository.GetFlightsByFlightNumber(flightNumber);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.Equal(flightNumber, f.FlightNumber));
    }

    [Fact]
    public async Task FlightsRepository_SearchFlights()
    {
        // Arrange
        var fromAirportId = 1;
        var toAirportId = 2;
        var date = DateTime.UtcNow.Date;

        var flights = new List<Flight>
        {
            new() { Id = 1, FlightNumber = "FL001", DateTime = date, FromAirportId = 1, ToAirportId = 2, Price = 1000 },
            new() { Id = 2, FlightNumber = "FL002", DateTime = date, FromAirportId = 1, ToAirportId = 3, Price = 2000 },
            new() { Id = 3, FlightNumber = "FL003", DateTime = date.AddDays(1), FromAirportId = 1, ToAirportId = 2, Price = 1500 }
        };

        _flightMockContext.Setup(c => c.Flights).ReturnsDbSet(flights);

        // Act
        var result = await _flightRepository.SearchFlights(fromAirportId, toAirportId, date);

        // Assert
        Assert.Single(result);
        var flight = result.First();
        Assert.Equal(fromAirportId, flight.FromAirportId);
        Assert.Equal(toAirportId, flight.ToAirportId);
        Assert.Equal(date, flight.DateTime.Date);
    }

    [Fact]
    public async Task AirportRepository_GetById()
    {
        // Arrange
        var airportId = 1L;
        var airports = new List<Airport>
        {
            new() { Id = 1, Name = "Шереметьево", City = "Москва", Country = "Россия" },
            new() { Id = 2, Name = "Пулково", City = "Санкт-Петербург", Country = "Россия" }
        };

        _flightMockContext.Setup(c => c.Airports).ReturnsDbSet(airports);

        // Act
        var result = await _airportRepository.GetById(airportId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(airportId, result.Id);
        Assert.Equal("Шереметьево", result.Name);
    }

    [Fact]
    public async Task PrivilegeRepository_GetByUsername()
    {
        // Arrange
        var username = "testuser";
        var privileges = new List<Privilege>
        {
            new() { Id = 1, Username = username, Status = "GOLD", Balance = 500 },
            new() { Id = 2, Username = "otheruser", Status = "SILVER", Balance = 200 }
        };

        _privilegeMockContext.Setup(c => c.Privileges).ReturnsDbSet(privileges);

        // Act
        var result = await _privilegeRepository.GetByUsername(username);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(username, result.Username);
        Assert.Equal("GOLD", result.Status);
    }

    [Fact]
    public async Task PrivilegeRepository_UpdateBalance()
    {
        // Arrange
        var privilegeId = 1;
        var initialBalance = 100;
        var balanceDiff = 50;
        
        var privileges = new List<Privilege>
        {
            new() { Id = privilegeId, Username = "testuser", Status = "BRONZE", Balance = initialBalance }
        };

        _privilegeMockContext.Setup(c => c.Privileges).ReturnsDbSet(privileges);
        _privilegeMockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()));

        // Act
        await _privilegeRepository.UpdateBalance(privilegeId, balanceDiff);

        // Assert
        var updatedPrivilege = privileges.First();
        Assert.Equal(initialBalance + balanceDiff, updatedPrivilege.Balance);
    }

    [Fact]
    public async Task PrivilegeHistoryRepository_GetByPrivilegeId()
    {
        // Arrange
        var privilegeId = 1;
        var history = new List<PrivilegeHistory>
        {
            new() { Id = 1, PrivilegeId = privilegeId, TicketUid = Guid.NewGuid(), Datetime = DateTime.UtcNow, BalanceDiff = 100, OperationType = "FILL_IN_BALANCE" },
            new() { Id = 2, PrivilegeId = privilegeId, TicketUid = Guid.NewGuid(), Datetime = DateTime.UtcNow.AddDays(-1), BalanceDiff = -50, OperationType = "DEBIT_THE_ACCOUNT" },
            new() { Id = 3, PrivilegeId = 2, TicketUid = Guid.NewGuid(), Datetime = DateTime.UtcNow, BalanceDiff = 200, OperationType = "FILL_IN_BALANCE" }
        };

        _privilegeMockContext.Setup(c => c.PrivilegeHistories).ReturnsDbSet(history);

        // Act
        var result = await _privilegeHistoryRepository.GetByPrivilegeId(privilegeId);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, h => Assert.Equal(privilegeId, h.PrivilegeId));
    }
}