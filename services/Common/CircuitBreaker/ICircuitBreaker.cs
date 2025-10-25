using Common.CircuitBreaker.Enums;

namespace Common.CircuitBreaker;

public interface ICircuitBreaker
{
    CircuitState State { get; }
    List<DateTime> FailureTimes { get; }
    
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, Func<T> fallback);
}