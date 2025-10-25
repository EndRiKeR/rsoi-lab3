using System.Text.Json.Serialization;

namespace Common.DtoModels.FlightServiceDto
{
    public class FlightResponse
    {
        [JsonPropertyName("flightNumber")]
        public string FlightNumber { get; set; } = string.Empty;
        
        [JsonPropertyName("fromAirport")]
        public string FromAirport { get; set; } = string.Empty;
        
        [JsonPropertyName("toAirport")]
        public string ToAirport { get; set; } = string.Empty;
        
        [JsonPropertyName("date")]
        public DateTime Date { get; set; }
        
        [JsonPropertyName("price")]
        public int Price { get; set; }
    }
}