using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Common.RetryQueue;

public class RetryBackgroundService : BackgroundService
{
    private readonly RetryQueueService _queueService;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _retryInterval = TimeSpan.FromSeconds(10);

    public RetryBackgroundService(
        RetryQueueService queueService,
        IServiceProvider serviceProvider)
    {
        _queueService = queueService;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Ждем между проверками очереди
                await Task.Delay(_retryInterval, stoppingToken);
                
                // Пытаемся достать запрос из очереди
                var request = await _queueService.DequeueAsync(stoppingToken);
                
                if (request != null)
                {
                    await ProcessRetryRequestAsync(request);
                }
            }
            catch (OperationCanceledException)
            {
                // Корректное завершение при остановке приложения
                break;
            }
            catch (Exception)
            {
                // Игнорируем ошибки и продолжаем работу
            }
        }
    }

    private async Task ProcessRetryRequestAsync(RetryRequest request)
    {
        try
        {
            // Создаем scope для получения сервисов
            using var scope = _serviceProvider.CreateScope();
            
            // Обрабатываем запрос в зависимости от типа
            switch (request.Type)
            {
                case "RETURN_BONUSES":
                    await ProcessReturnBonusesAsync(request, scope);
                    break;
                case "UPDATE_BALANCE":
                    await UpdateBalanceAsync(request, scope);
                    break;
                default:
                    // Неизвестный тип запроса - просто удаляем из очереди
                    break;
            }
        }
        catch (Exception)
        {
            // Если обработка не удалась - возвращаем запрос в очередь
            if (request.Attempts < 5) // Максимум 5 попыток
            {
                request.Attempts++;
                request.LastAttemptAt = DateTime.UtcNow;
                _queueService.Enqueue(request);
            }
            // Если превышено максимальное количество попыток - запрос удаляется
        }
    }

    private async Task ProcessReturnBonusesAsync(RetryRequest request, IServiceScope scope)
    {
        // Здесь будет логика возврата бонусов через BonusService
        // Пока заглушка - имитируем успешное выполнение
        await Task.Delay(100);
        
        // В реальности:
        // var bonusService = scope.ServiceProvider.GetRequiredService<IBonusService>();
        // await bonusService.ReturnBonusesAsync(request.TicketUid, request.Username);
    }

    private async Task UpdateBalanceAsync(RetryRequest request, IServiceScope scope)
    {
        // Здесь будет логика обновления баланса через BonusService
        // Пока заглушка - имитируем успешное выполнение
        await Task.Delay(100);
        
        // В реальности будем десериализовать данные из SerializedData
        // и вызывать соответствующий метод BonusService
    }
}