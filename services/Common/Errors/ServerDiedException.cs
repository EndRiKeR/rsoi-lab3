namespace Common.Errors;

public class ServerDiedException : Exception
{
    public ServerDiedException(string msg) : base(msg) { }
}