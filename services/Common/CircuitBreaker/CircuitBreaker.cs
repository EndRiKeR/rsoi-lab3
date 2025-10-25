using Common.CircuitBreaker.Enums;
using Common.Errors;
using Common.LoggerExtensions;
using Microsoft.Extensions.Logging;

namespace Common.CircuitBreaker;

public class CircuitBreaker : ICircuitBreaker
{
    public CircuitState State { get; set; } = CircuitState.Closed;
    public int FailureCount { get; set; } = 0;
    public Services ServiceType { get; set; }
    private DateTime LastFailureTime { get; set; } = DateTime.MinValue;

    private readonly ILogger<CircuitBreaker> _logger;
    private readonly int _maxFailuresBeforeOpen = 5;
    private readonly TimeSpan _openToHalfOpenTimeout = TimeSpan.FromSeconds(10);
    
    public CircuitBreaker(ILogger<CircuitBreaker> logger)
    {
        _logger = logger;
        _logger.LogGoodCircuitBreakerInfo("CircuitBreaker готов к работе!");
    }

    // через этот метод запускается запрос
    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, Func<T> fallback = null)
    {
        _logger.LogGoodCircuitBreakerInfo("Щиток пробует выполнить метод");
        
        // если цель разомкнута...
        if (State == CircuitState.Open)
        {
            _logger.LogGoodCircuitBreakerInfo("Пробуем замкнуть цепь...");
            
            // либо возвращает fallback, либо, через время, пытаемся цепь замкнуть
            if (DateTime.UtcNow - LastFailureTime > _openToHalfOpenTimeout)
            {
                State = CircuitState.HalfOpen;
                _logger.LogBadCircuitBreakerInfo("Пора попробовать снова!");
                _logger.CircuitBreakerStateChange(State, CircuitState.HalfOpen);
            }
            else
            {
                _logger.LogBadCircuitBreakerInfo("Рано, попробуй позже");

                if (fallback != null)
                    return fallback();
                
                throw new ServerDiedException($"{ServiceType} Service unavailable");
            }
        }

        // пробуем выполнить метод
        try
        {
            _logger.LogGoodCircuitBreakerInfo("Пробуем выполнить метод");
            
            var result = await action();
            
            _logger.LogGoodCircuitBreakerInfo("Все ок!");
            
            FailureCount = 0;
            
            // замыкаем цепь при успешном выполнении
            if (State == CircuitState.HalfOpen)
            {
                _logger.CircuitBreakerStateChange(State, CircuitState.Closed);
                State = CircuitState.Closed;
            }
            
            return result;
        }
        catch (Exception)
        {
            FailureCount++;
            
            _logger.LogBadCircuitBreakerInfo($"Ошибка номер {FailureCount}");
            
            LastFailureTime = DateTime.UtcNow;

            if (FailureCount >= _maxFailuresBeforeOpen)
            {
                _logger.LogBadCircuitBreakerInfo($"Слишком много ошибок... Размыкаем цепь.");
                State = CircuitState.Open;
                _logger.CircuitBreakerStateChange(State, CircuitState.Open);
            }
            
            if (fallback != null)
                return fallback();
                
            throw new ServerDiedException($"{ServiceType} Service unavailable");
        }
    }
}