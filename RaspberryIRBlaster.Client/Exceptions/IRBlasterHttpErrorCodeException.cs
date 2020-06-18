using System;
using System.Net;

namespace RaspberryIRBlaster.Client.Exceptions
{
    public class IRBlasterHttpErrorCodeException : Exception
    {
        public HttpStatusCode HttpCode { get; }

        /// <summary>
        /// The error message sent back from the server.
        /// </summary>
        public string Body { get; }

        public IRBlasterHttpErrorCodeException(HttpStatusCode httpCode, string body) : base($"The server returned error '{httpCode}'. See the {nameof(Body)} property for more details.")
        {
            HttpCode = httpCode;
            Body = body;
        }
    }
}
