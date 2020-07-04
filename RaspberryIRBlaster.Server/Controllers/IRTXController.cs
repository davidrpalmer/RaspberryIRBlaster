using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RaspberryIRBlaster.Server.Models;

namespace RaspberryIRBlaster.Server.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class IRTXController : ControllerBase
    {
        private readonly ILogger<IRTXController> _logger;

        private Application.IRTransmitter GetIRTransmitter()
        {
            return Application.IRTransmitter.Instance;
        }

        public IRTXController(ILogger<IRTXController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Send a predefined IR message.
        /// </summary>
        [HttpPost]
        public void SendBatch(SendBatchRequestModel data)
        {
            _logger.LogInformation("Send batch start.");
            var actions = new List<Application.Actions.IAction>();

            foreach (var item in data.Items)
            {
                if (item.Type.Equals(SendBatchItemRequestModel.Type_Sleep, StringComparison.OrdinalIgnoreCase))
                {
                    if (item.Data.ValueKind == System.Text.Json.JsonValueKind.Null)
                    {
                        throw new ArgumentNullException(nameof(item.Data));
                    }

                    TimeSpan sleepTime;
                    if (item.Data.ValueKind == System.Text.Json.JsonValueKind.Number)
                    {
                        sleepTime = TimeSpan.FromMilliseconds(item.Data.GetInt32());
                    }
                    else if (item.Data.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        string str = item.Data.GetString();
                        if (!int.TryParse(str, out int sleepTimeInt))
                        {
                            throw new ArgumentException("The data for a sleep action must be a number of milliseconds.");
                        }
                        sleepTime = TimeSpan.FromMilliseconds(sleepTimeInt);
                    }
                    else
                    {
                        throw new ArgumentException($"Unexpected type for data. Type={item.Data.ValueKind}");
                    }

                    if (sleepTime <= TimeSpan.Zero)
                    {
                        throw new ArgumentOutOfRangeException(nameof(item.Data), "Sleep time cannot be zero or negative.");
                    }
                    if (sleepTime > TimeSpan.FromSeconds(2))
                    {
                        throw new ArgumentOutOfRangeException(nameof(item.Data), "Sleep time cannot be more than 2 seconds.");
                    }
                    actions.Add(new Application.Actions.SleepAction(sleepTime));
                }
                else if (item.Type.Equals(SendBatchItemRequestModel.Type_SendMessage, StringComparison.OrdinalIgnoreCase))
                {
                    if (item.Data.ValueKind == System.Text.Json.JsonValueKind.Null)
                    {
                        throw new ArgumentNullException(nameof(item.Data));
                    }

                    if (item.Data.ValueKind == System.Text.Json.JsonValueKind.String)
                    {
                        string messageName = item.Data.GetString();
                        if (string.IsNullOrWhiteSpace(messageName))
                        {
                            throw new ArgumentNullException(nameof(item.Data));
                        }

                        actions.Add(new Application.Actions.SendMessageAction(messageName));
                    }
                    else
                    {
                        throw new ArgumentException($"Unexpected type for data. Type={item.Data.ValueKind}");
                    }
                }
                else
                {
                    throw new ArgumentException("Unknown batch item type.");
                }
            }

            GetIRTransmitter().Run(actions);
            _logger.LogInformation("Send batch end.");
        }

        /// <summary>
        /// Cancel any transmit operations that are on going.
        /// </summary>
        [HttpPost]
        public void Abort()
        {
            _logger.LogInformation("Abort batch start.");
            GetIRTransmitter().Abort();
            _logger.LogInformation("Abort batch end.");
        }
    }
}
