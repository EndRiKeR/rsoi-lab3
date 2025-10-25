using System.Net.Http.Json;
using Common.RetryQueue;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class RetryBackgroundService : BackgroundService
{
    private readonly ILogger<RetryBackgroundService> _logger;
    private readonly RetryQueueService _queueService;
    private readonly TimeSpan _retryInterval;

    public RetryBackgroundService(
        RetryQueueService queueService,
        ILogger<RetryBackgroundService> logger)
    {
        _queueService = queueService;
        _logger = logger;
        _retryInterval = TimeSpan.FromSeconds(10);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("🚀 RetryBackgroundService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_retryInterval, stoppingToken);
                
                var request = await _queueService.DequeueAsync(stoppingToken);
                
                if (request != null)
                    await ProcessRetryRequestAsync(request);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("⏹️ RetryBackgroundService stopping");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error in RetryBackgroundService");
            }
        }

        _logger.LogInformation("🛑 RetryBackgroundService stopped");
    }

    private async Task ProcessRetryRequestAsync(RetryRequest request)
    {
        try
        {
            HttpRequestMessage msg = new HttpRequestMessage(request.HttpMethod, request.Api)
            {
                Content = JsonContent.Create(request.Body)
            };
            msg.Headers.Add("X-User-Name", request.Username);
            
            var bonusResponse = await request.Client.SendAsync(msg);

            if (!bonusResponse.IsSuccessStatusCode)
                throw new Exception();
            
            _logger.LogInformation("✅ Successfully processed retry request");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to process retry request");
            
            request.Attempts++;
            request.LastAttemptAt = DateTime.UtcNow;
            
            if (request.Attempts < 30)
            {
                _queueService.Enqueue(request);
                _logger.LogWarning("🔄 Re-queued request {Type} (attempt {Attempt})", 
                    request.RequestBody, request.Attempts);
            }
            else
            {
                _logger.LogError("💥 Max retry attempts exceeded for request");
            }
        }
    }
}