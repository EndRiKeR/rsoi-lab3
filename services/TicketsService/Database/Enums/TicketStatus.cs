namespace TicketsService.Database.Enums;

public enum TicketStatus
{
    PAID,
    CANCELED
}

public static class TicketStatusConverter
{
    public static TicketStatus ToStatus(string statusName)
    {
        return statusName switch
        {
            "PAID" => TicketStatus.PAID,
            "CANCELED" => TicketStatus.CANCELED,
            "" => throw new ArgumentException("Status name cannot be empty"),
            _ => throw new ArgumentOutOfRangeException(nameof(statusName), statusName, null)
        };
    }
}