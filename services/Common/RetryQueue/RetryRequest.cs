namespace Common.RetryQueue;

public enum RetryType
{
    RETURN_BONUSES = 0,
    UPDATE_BALANCE
}

public class RetryRequest
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public RetryType Type { get; set; }
    public Guid TicketUid { get; set; }
    public string Username { get; set; }
    public int Attempts { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastAttemptAt { get; set; }
}