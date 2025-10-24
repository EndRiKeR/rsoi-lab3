using Common.CircuitBreaker.Enums;

namespace Common.CircuitBreaker;

public class CircuitBreaker
{
    private CircuitState _state = CircuitState.Closed;
    private int _failureCount = 0;
    private DateTime _lastFailureTime = DateTime.MinValue;
    private readonly TimeSpan _openToHalfOpenTimeout = TimeSpan.FromSeconds(30);
    private readonly int _maxFailuresBeforeOpen = 5;
    
    public CircuitState State => _state;
    public int FailureCount => _failureCount;
    public DateTime LastFailureTime => _lastFailureTime;

    public async Task<T> ExecuteAsync<T>(
        Func<Task<T>> action,
        Func<T> fallback,
        string serviceName)
    {
        if (_state == CircuitState.Open)
        {
            if (DateTime.UtcNow - _lastFailureTime > _openToHalfOpenTimeout)
            {
                _state = CircuitState.HalfOpen;
            }
            else
            {
                return fallback();
            }
        }

        try
        {
            var result = await action();
            
            _failureCount = 0;
            
            if (_state == CircuitState.HalfOpen)
            {
                _state = CircuitState.Closed;
            }
            
            return result;
        }
        catch (Exception)
        {
            _failureCount++;
            _lastFailureTime = DateTime.UtcNow;
            
            if (_failureCount >= _maxFailuresBeforeOpen)
            {
                _state = CircuitState.Open;
            }
            
            return fallback();
        }
    }
}