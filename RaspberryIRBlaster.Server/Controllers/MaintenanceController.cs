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
    public class MaintenanceController : ControllerBase
    {
        private readonly ILogger<MaintenanceController> _logger;

        private Application.IRTransmitter GetIRTransmitter()
        {
            return Program.IRTransmitter;
        }

        public MaintenanceController(ILogger<MaintenanceController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Clear caches - e.g. loaded remote configs.
        /// </summary>
        [HttpPost]
        public void ClearCache()
        {
            GetIRTransmitter().ClearRemoteCache();
        }
    }
}
