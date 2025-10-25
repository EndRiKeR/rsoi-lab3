namespace Common.RetryQueue;

public class RetryRequest
{
    public HttpClient Client { get; set; }
    public HttpRequestMessage RequestBody { get; set; }
    public int Attempts { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastAttemptAt { get; set; }
}