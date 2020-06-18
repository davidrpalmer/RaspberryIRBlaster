using System;

namespace RaspberryIRBlaster.Client.Exceptions
{
    public class IRBlasterTransportException : Exception
    {
        public IRBlasterTransportException(string message) : base(message)
        {
        }

        public IRBlasterTransportException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
