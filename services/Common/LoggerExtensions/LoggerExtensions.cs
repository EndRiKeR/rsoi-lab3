using Common.CircuitBreaker.Enums;
using Microsoft.Extensions.Logging;

namespace Common.LoggerExtensions;

public static class LoggerExtensions
{
    public static void CircuitBreakerStateChange(
        this ILogger logger, 
        CircuitState fromState, 
        CircuitState toState)
    {
        logger.LogInformation("\u26a1 Circuit state changed: from {FromState} to {ToState}",
            fromState, toState);
    }

    public static void LogGoodCircuitBreakerInfo(this ILogger logger, string info)
    {
        logger.LogInformation("\u2705 {info}", info);
    }
    
    public static void LogBadCircuitBreakerInfo(this ILogger logger, string info)
    {
        logger.LogInformation("\u274c {info}", info);
    }
}