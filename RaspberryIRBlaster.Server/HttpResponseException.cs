using System;
using System.Net;

namespace RaspberryIRBlaster.Server
{
    public class HttpResponseException : Exception
    {
        public HttpResponseException(HttpStatusCode httpCode, string message) : base(message)
        {
            Status = httpCode;
        }

        public HttpResponseException(HttpStatusCode httpCode, string message, Exception innerException) : base(message, innerException)
        {
            Status = httpCode;
        }

        public HttpResponseException(HttpStatusCode httpCode, Exception innerException) : base(innerException.Message, innerException)
        {
            Status = httpCode;
        }

        public HttpStatusCode Status { get; }

        public int StatusCode => (int)Status;
    }
}
