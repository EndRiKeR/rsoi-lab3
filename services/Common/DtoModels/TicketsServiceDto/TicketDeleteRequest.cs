using System.Text.Json.Serialization;

namespace Common.DtoModels.TicketsServiceDto
{
    public class TicketDeleteRequest
    {
        [JsonPropertyName("ticketUid")]
        public Guid TicketUid { get; set; }
    }
}
