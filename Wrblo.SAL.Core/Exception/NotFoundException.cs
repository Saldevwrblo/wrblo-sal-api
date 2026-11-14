namespace Wrblo.SAL.Core.Exception;

public class NotFoundException : System.Exception
{
    public int ErrorCode { get; }

    public NotFoundException(string message, int errorCode = 404) : base(message)
    {
        ErrorCode = errorCode;
    }
}
