using System.Text.Json.Serialization;
using Common.DtoModels.BonusServiceDto;
using Common.DtoModels.TicketsServiceDto;

namespace Common.DtoModels.GatewayDto
{

    // Gateway DTO
    public class UserInfoResponse
    {
        [JsonPropertyName("tickets")]
        public List<TicketResponse> Tickets { get; set; } = new();
        
        [JsonPropertyName("privilege")]
        public PrivilegeShortInfo Privilege { get; set; } = new();
    }
}