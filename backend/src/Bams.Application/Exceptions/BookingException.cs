namespace Bams.Application;

public sealed class BookingException(string message, int statusCode) : Exception(message)
{
    public int StatusCode
    {
        get;
    }
     = statusCode;
}
