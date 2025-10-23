using System.Text.Json.Serialization;

namespace Common.DtoModels.FlightServiceDto
{
    public class PaginationResponse
    {
        [JsonPropertyName("page")]
        public int Page { get; set; }
        
        [JsonPropertyName("pageSize")]
        public int PageSize { get; set; }
        
        [JsonPropertyName("totalElements")]
        public int TotalElements { get; set; }
        
        [JsonPropertyName("items")]
        public List<FlightResponse> Items { get; set; } = new();
    }
}