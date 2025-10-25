using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.RetryQueue;

public class RetryBackgroundService : BackgroundService
{
    public CancellationToken BackCancellationToken { get; set; }
    
    private readonly RetryQueueService _queueService;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _retryInterval = TimeSpan.FromSeconds(10);

    public RetryBackgroundService(
        RetryQueueService queueService,
        IServiceProvider serviceProvider,
        CancellationTokenSource source)
    {
        _queueService = queueService;
        _serviceProvider = serviceProvider;

        BackCancellationToken = source.Token;
        source.

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_retryInterval, stoppingToken);
                
                var request = await _queueService.DequeueAsync(stoppingToken);
                
                if (request != null)
                    await ProcessRetryRequestAsync(request);
            }
            catch (OperationCanceledException _)
            {
                break;
            }
            catch (Exception _) { }
        }
    }

    private async Task ProcessRetryRequestAsync(RetryRequest request)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            
            switch (request.Type)
            {
                case RetryType.RETURN_BONUSES:
                    await ProcessReturnBonusesAsync(request, scope);
                    break;
                case RetryType.UPDATE_BALANCE:
                    await UpdateBalanceAsync(request, scope);
                    break;
            }
        }
        catch (Exception)
        {
            request.Attempts++;
            request.LastAttemptAt = DateTime.UtcNow;
            _queueService.Enqueue(request);
        }
    }

    private async Task ProcessReturnBonusesAsync(RetryRequest request, IServiceScope scope)
    {
        // var bonusService = scope.ServiceProvider.GetRequiredService<IBonusService>();
        // await bonusService.ReturnBonusesAsync(request.TicketUid, request.Username);
    }

    private async Task UpdateBalanceAsync(RetryRequest request, IServiceScope scope)
    {
        // В реальности будем десериализовать данные из SerializedData
        // и вызывать соответствующий метод BonusService
    }
}