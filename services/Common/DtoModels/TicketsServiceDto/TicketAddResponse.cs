using System.Text.Json.Serialization;

namespace Common.DtoModels.TicketsServiceDto
{
    public class TicketAddResponse
    {
        [JsonPropertyName("ticketUid")]
        public Guid TicketUid { get; set; }
    }
}