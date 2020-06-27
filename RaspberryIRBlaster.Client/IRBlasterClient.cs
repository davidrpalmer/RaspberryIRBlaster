using System;
using System.Collections.Generic;
using RestSharp;
using RaspberryIRBlaster.Client.BatchActions;
using RaspberryIRBlaster.Client.Exceptions;

namespace RaspberryIRBlaster.Client
{
    public class IRBlasterClient
    {
        private readonly IRestClient _client;

        private readonly Func<IRestRequest> _createRequestFunc;

        public IRBlasterClient(string baseUrl) : this(new RestClient(baseUrl), () => new RestRequest())
        {
        }

        public IRBlasterClient(IRestClient restClient, Func<IRestRequest> requestFactory)
        {
            _client = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _createRequestFunc = requestFactory ?? throw new ArgumentNullException(nameof(requestFactory));
        }

        private void ThrowIfRequestFailed(IRestResponse response)
        {
            if (response == null) { throw new ArgumentNullException(nameof(response)); }

            switch (response.ResponseStatus)
            {
                case ResponseStatus.Completed:
                    if (!response.IsSuccessful)
                    {
                        throw new IRBlasterHttpErrorCodeException(response.StatusCode, response.Content);
                    }
                    break;
                case ResponseStatus.Aborted:
                    throw new IRBlasterTransportException("Request was aborted.", response.ErrorException);
                case ResponseStatus.TimedOut:
                    throw new IRBlasterTransportException("Request timed out.", response.ErrorException);
                case ResponseStatus.Error:
                    throw new IRBlasterTransportException("There was an error while making the request.", response.ErrorException);
                default:
                    throw new Exception($"Unknown response status '{response.ResponseStatus}'.");
            }
        }

        public void SendBatch(params IBatchAction[] actions) => SendBatch((ICollection<IBatchAction>)actions);

        public void SendBatch(ICollection<IBatchAction> actions)
        {
            if (actions == null) { throw new ArgumentNullException(nameof(actions)); }

            var request = _createRequestFunc();
            request.Method = Method.POST;
            request.Resource = "IRTX/SendBatch";

            request.AddJsonBody(new { Items = actions });

            var response = _client.Execute(request);
            ThrowIfRequestFailed(response);
        }

        /// <summary>
        /// Request the server abort the current IR transmission.
        /// </summary>
        public void Abort()
        {
            var request = _createRequestFunc();
            request.Method = Method.POST;
            request.Resource = "IRTX/Abort";

            var response = _client.Execute(request);
            ThrowIfRequestFailed(response);
        }

        /// <summary>
        /// Request the server clear the cache of remote profiles so any changes to the JSON files are picked up.
        /// </summary>
        public void ClearCache()
        {
            var request = _createRequestFunc();
            request.Method = Method.POST;
            request.Resource = "Maintenance/ClearCache";

            var response = _client.Execute(request);
            ThrowIfRequestFailed(response);
        }
    }
}
