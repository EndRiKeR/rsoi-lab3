namespace Common.RetryQueue;

public class RetryRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty;           // "RETURN_BONUSES", "UPDATE_BALANCE"
    public Guid TicketUid { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Attempts { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastAttemptAt { get; set; }
    public string SerializedData { get; set; } = string.Empty; // JSON с дополнительными данными
}