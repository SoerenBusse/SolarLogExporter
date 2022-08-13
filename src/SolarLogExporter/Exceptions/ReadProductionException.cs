using System.Runtime.Serialization;

namespace SolarLogExporter.Exceptions;

public class ReadProductionException : Exception
{
    public ReadProductionException()
    {
    }

    protected ReadProductionException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public ReadProductionException(string? message) : base(message)
    {
    }

    public ReadProductionException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}