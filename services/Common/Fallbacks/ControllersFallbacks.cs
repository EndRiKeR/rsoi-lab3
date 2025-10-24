using Common.DtoModels.FlightServiceDto;

namespace FlightService.Controllers.Fallbacks;

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
}