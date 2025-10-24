namespace Common.Errors;

public class ServerDiedException : Exception
{
    public ServerDiedException(string msg) : base(msg) { }
}

public class FlightDoesntExistException : Exception
{
    public FlightDoesntExistException(string msg) : base(msg) { }
    
}
