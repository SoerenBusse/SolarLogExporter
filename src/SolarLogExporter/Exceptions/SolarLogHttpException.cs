using System.Runtime.Serialization;

namespace SolarLogExporter.Exceptions;

public class SolarLogHttpException : Exception
{
    public SolarLogHttpException()
    {
    }

    protected SolarLogHttpException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public SolarLogHttpException(string? message) : base(message)
    {
    }

    public SolarLogHttpException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}