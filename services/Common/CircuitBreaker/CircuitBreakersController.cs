using Common.CircuitBreaker.Enums;
using Common.LoggerExtensions;
using Microsoft.Extensions.Logging;

namespace Common.CircuitBreaker;

public class CircuitBreakersController
{
    private readonly ILogger<CircuitBreakersController> _controllerLogger;
    private readonly ILogger<CircuitBreaker> _logger;
    
    private Dictionary<Services, CircuitBreaker> _circuitBreakers = new();
    
    public CircuitBreakersController(
        ILogger<CircuitBreakersController> controllerLogger,
        ILogger<CircuitBreaker> logger)
    {
        _controllerLogger = controllerLogger;
        _logger = logger;
        _logger.LogGoodCircuitBreakerInfo("CircuitBreakersController создан и готов к работе!");
    }

    public async Task<T> ExecuteSoftAsync<T>(Services service, Func<Task<T>> action, Func<T> fallback)
    {
        CircuitBreaker circuitBreaker = GetCircuitBreaker(service);
        
        return await circuitBreaker.ExecuteAsync(action, fallback);
    }
    
    public async Task<T> ExecuteAsync<T>(Services service, Func<Task<T>> action)
    {
        CircuitBreaker circuitBreaker = GetCircuitBreaker(service);
        
        return await circuitBreaker.ExecuteAsync(action);
    }

    private CircuitBreaker GetCircuitBreaker(Services service)
    {
        if (_circuitBreakers.TryGetValue(service, out var circuitBreakerInDict))
        {
            _controllerLogger.LogGoodCircuitBreakerInfo($"Щиток для {service} найден!");
            
            return circuitBreakerInDict;
        }
        else
        {
            _controllerLogger.LogBadCircuitBreakerInfo($"Не нашел щиток для {service}. Создаю новый.");
            
            _circuitBreakers[service] = new CircuitBreaker(_logger);
            return _circuitBreakers[service];
        }
    }
}