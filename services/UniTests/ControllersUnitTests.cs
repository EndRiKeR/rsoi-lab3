using BonusService.Controllers;
using BonusService.Database.Models;
using BonusService.Database.Repositories.Interfaces;
using Common.DtoModels.BonusServiceDto;
using Common.DtoModels.FlightServiceDto;
using FlightService.Controllers;
using FlightService.Database.Models;
using FlightService.Database.Repositories.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TicketsService.Database.Repositories.Interfaces;
using Xunit;

namespace UniTests;

public class ControllersUnitTests
{
    private readonly Mock<ITicketRepository> _mockTicketRepository = new();
    private readonly Mock<IFlightRepository> _mockFlightRepository = new();
    private readonly Mock<IAirportRepository> _mockAirportRepository = new();
    private readonly Mock<IPrivilegeRepository> _mockPrivilegeRepository = new();
    private readonly Mock<IPrivilegeHistoryRepository> _mockPrivilegeHistoryRepository = new();
    
    private readonly Mock<IHttpClientFactory> _mockHttpClientFactory = new();

    [Fact]
    public async Task FlightsController_GetFlights()
    {
        // Arrange
        var flights = new List<Flight>
        {
            new() { Id = 1, FlightNumber = "FL001", DateTime = DateTime.UtcNow, Price = 1000, FromAirportId = 1, ToAirportId = 2 },
            new() { Id = 2, FlightNumber = "FL002", DateTime = DateTime.UtcNow, Price = 2000, FromAirportId = 2, ToAirportId = 1 }
        };

        var airports = new List<Airport>
        {
            new() { Id = 1, Name = "Шереметьево", City = "Москва", Country = "Россия" },
            new() { Id = 2, Name = "Пулково", City = "Санкт-Петербург", Country = "Россия" }
        };

        _mockFlightRepository.Setup(r => r.GetAll()).ReturnsAsync(flights);
        _mockAirportRepository.Setup(r => r.GetById(1)).ReturnsAsync(airports[0]);
        _mockAirportRepository.Setup(r => r.GetById(2)).ReturnsAsync(airports[1]);

        var controller = new FlightsController(_mockFlightRepository.Object, _mockAirportRepository.Object);

        // Act
        var result = await controller.GetFlights(1, 10);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var paginationResponse = Assert.IsType<PaginationResponse>(okResult.Value);
        Assert.Equal(2, paginationResponse.TotalElements);
        Assert.Equal(2, paginationResponse.Items.Count);
    }

    [Fact]
    public async Task FlightsController_GetFlightByFlightNumber()
    {
        // Arrange
        var flightNumber = "FL001";
        var flights = new List<Flight>
        {
            new() { Id = 1, FlightNumber = flightNumber, DateTime = DateTime.UtcNow, Price = 1000, FromAirportId = 1, ToAirportId = 2 }
        };

        var airports = new List<Airport>
        {
            new() { Id = 1, Name = "Шереметьево", City = "Москва", Country = "Россия" },
            new() { Id = 2, Name = "Пулково", City = "Санкт-Петербург", Country = "Россия" }
        };

        _mockFlightRepository.Setup(r => r.GetFlightsByFlightNumber(flightNumber)).ReturnsAsync(flights);
        _mockAirportRepository.Setup(r => r.GetById(1)).ReturnsAsync(airports[0]);
        _mockAirportRepository.Setup(r => r.GetById(2)).ReturnsAsync(airports[1]);

        var controller = new FlightsController(_mockFlightRepository.Object, _mockAirportRepository.Object);

        // Act
        var result = await controller.GetFlightByNumber(flightNumber);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var flightResponse = Assert.IsType<FlightResponse>(okResult.Value);
        Assert.Equal(flightNumber, flightResponse.FlightNumber);
        Assert.Contains("Москва", flightResponse.FromAirport);
        Assert.Contains("Санкт-Петербург", flightResponse.ToAirport);
    }

    [Fact]
    public async Task PrivilegeController_GetPrivilegeInfo()
    {
        // Arrange
        var username = "testuser";
        var privilege = new Privilege 
        { 
            Id = 1, 
            Username = username, 
            Status = "GOLD", 
            Balance = 500 
        };
        var history = new List<PrivilegeHistory>
        {
            new() { Id = 1, PrivilegeId = 1, TicketUid = Guid.NewGuid(), Datetime = DateTime.UtcNow, BalanceDiff = 100, OperationType = "FILL_IN_BALANCE" }
        };

        _mockPrivilegeRepository.Setup(r => r.GetByUsername(username)).ReturnsAsync(privilege);
        _mockPrivilegeHistoryRepository.Setup(r => r.GetByPrivilegeId(1)).ReturnsAsync(history);

        var controller = new PrivilegeController(_mockPrivilegeRepository.Object, _mockPrivilegeHistoryRepository.Object);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-User-Name"] = username;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await controller.GetPrivilegeInfo();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var privilegeInfo = Assert.IsType<PrivilegeInfoResponse>(okResult.Value);
        Assert.Equal(privilege.Balance, privilegeInfo.Balance);
        Assert.Equal(privilege.Status, privilegeInfo.Status);
        Assert.Single(privilegeInfo.History);
    }

    [Fact]
    public async Task PrivilegeController_UpdateBalance()
    {
        // Arrange
        var username = "testuser";
        var historyRequest = new UpdateBalanceHistoryRequest
        {
            TicketUid = Guid.NewGuid(),
            BalanceDiff = 100,
            OperationType = "FILL_IN_BALANCE"
        };
        var privilege = new Privilege 
        { 
            Id = 1, 
            Username = username, 
            Status = "BRONZE", 
            Balance = 0 
        };

        _mockPrivilegeRepository.Setup(r => r.ExistsByUsername(username)).ReturnsAsync(true);
        _mockPrivilegeRepository.Setup(r => r.GetByUsername(username)).ReturnsAsync(privilege);
        _mockPrivilegeRepository.Setup(r => r.UpdateBalance(1, 100)).Returns(Task.CompletedTask);
        _mockPrivilegeHistoryRepository.Setup(r => r.Add(It.IsAny<PrivilegeHistory>()))
            .ReturnsAsync((PrivilegeHistory h) => h);

        var controller = new PrivilegeController(_mockPrivilegeRepository.Object, _mockPrivilegeHistoryRepository.Object);
        
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-User-Name"] = username;
        controller.ControllerContext = new ControllerContext { HttpContext = httpContext };

        // Act
        var result = await controller.UpdateBalance(historyRequest);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        _mockPrivilegeRepository.Verify(r => r.UpdateBalance(1, 100), Times.Once);
        _mockPrivilegeHistoryRepository.Verify(r => r.Add(It.IsAny<PrivilegeHistory>()), Times.Once);
    }
    
        // [Fact]
    // public async Task TicketsController_GetUserTickets()
    // {
    //     // Arrange
    //     var username = "testuser";
    //     var tickets = new List<Ticket>
    //     {
    //         new() { Id = 1, TicketUid = Guid.NewGuid(), Username = username, FlightNumber = "FL001", Price = 1000, Status = TicketStatus.PAID },
    //         new() { Id = 2, TicketUid = Guid.NewGuid(), Username = username, FlightNumber = "FL002", Price = 2000, Status = TicketStatus.PAID }
    //     };
    //
    //     _mockTicketRepository.Setup(r => r.GetAll()).ReturnsAsync(tickets);
    //
    //     var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
    //     var httpClient = new HttpClient(mockHttpMessageHandler.Object);
    //     _mockHttpClientFactory.Setup(f => f.CreateClient("Gateway")).Returns(httpClient);
    //
    //     var controller = new TicketsController(_mockTicketRepository.Object, _mockHttpClientFactory.Object);
    //     
    //     var httpContext = new DefaultHttpContext();
    //     httpContext.Request.Headers["X-User-Name"] = username;
    //     controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    //
    //     // Act
    //     var result = await controller.GetUserTickets();
    //
    //     // Assert
    //     var okResult = Assert.IsType<OkObjectResult>(result);
    //     var returnedTickets = Assert.IsType<List<TicketResponse>>(okResult.Value);
    //     Assert.Equal(2, returnedTickets.Count);
    // }
    //
    // [Fact]
    // public async Task TicketsController_GetTicket()
    // {
    //     // Arrange
    //     var ticketUid = Guid.NewGuid();
    //     var username = "testuser";
    //     var ticket = new Ticket 
    //     { 
    //         Id = 1, 
    //         TicketUid = ticketUid, 
    //         Username = username, 
    //         FlightNumber = "FL001", 
    //         Price = 1000, 
    //         Status = TicketStatus.PAID 
    //     };
    //
    //     var tickets = new List<Ticket> { ticket };
    //     _mockTicketRepository.Setup(r => r.GetAll()).ReturnsAsync(tickets);
    //
    //     var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
    //     var httpClient = new HttpClient(mockHttpMessageHandler.Object);
    //     _mockHttpClientFactory.Setup(f => f.CreateClient("Gateway")).Returns(httpClient);
    //
    //     var controller = new TicketsController(_mockTicketRepository.Object, _mockHttpClientFactory.Object);
    //     
    //     var httpContext = new DefaultHttpContext();
    //     httpContext.Request.Headers["X-User-Name"] = username;
    //     controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    //
    //     // Act
    //     var result = await controller.GetTicket(ticketUid);
    //
    //     // Assert
    //     var okResult = Assert.IsType<OkObjectResult>(result);
    //     var returnedTicket = Assert.IsType<TicketResponse>(okResult.Value);
    //     Assert.Equal(ticketUid, returnedTicket.TicketUid);
    // }
    //
    // [Fact]
    // public async Task TicketsController_BuyTicket()
    // {
    //     // Arrange
    //     var username = "testuser";
    //     var request = new TicketPurchaseRequest 
    //     { 
    //         FlightNumber = "FL001", 
    //         Price = 1500, 
    //         PaidFromBalance = false 
    //     };
    //     var newTicket = new Ticket 
    //     { 
    //         Id = 1, 
    //         TicketUid = Guid.NewGuid(), 
    //         Username = username, 
    //         FlightNumber = "FL001", 
    //         Price = 1500, 
    //         Status = TicketStatus.PAID 
    //     };
    //
    //     _mockTicketRepository.Setup(r => r.Add(It.IsAny<Ticket>())).ReturnsAsync(newTicket);
    //
    //     // Mock HttpClient responses
    //     var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
    //     var httpClient = new HttpClient(mockHttpMessageHandler.Object);
    //     _mockHttpClientFactory.Setup(f => f.CreateClient("Gateway")).Returns(httpClient);
    //
    //     var controller = new TicketsController(_mockTicketRepository.Object, _mockHttpClientFactory.Object);
    //     
    //     var httpContext = new DefaultHttpContext();
    //     httpContext.Request.Headers["X-User-Name"] = username;
    //     controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
    //
    //     // Act
    //     var result = await controller.BuyTicket(request);
    //
    //     // Assert
    //     var okResult = Assert.IsType<OkObjectResult>(result);
    //     var purchaseResponse = Assert.IsType<TicketPurchaseResponse>(okResult.Value);
    //     Assert.Equal(newTicket.TicketUid, purchaseResponse.TicketUid);
    //     Assert.Equal(request.Price, purchaseResponse.Price);
    // }
}