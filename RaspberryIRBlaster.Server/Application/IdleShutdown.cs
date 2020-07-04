using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RaspberryIRBlaster.Server.Application
{
    public class IdleShutdown : IHostedService
    {
        public static IdleShutdown Instance { get; private set; }


        private readonly System.Diagnostics.Stopwatch _stopwatch = System.Diagnostics.Stopwatch.StartNew();

        private readonly System.Timers.Timer _timer = new System.Timers.Timer(10000);

        private readonly IHost _host;

        private readonly ILogger<IdleShutdown> _logger;

        public IdleShutdown(IHost host)
        {
            _host = host;
            _logger = _host.Services.GetRequiredService<ILogger<IdleShutdown>>();

            _timer.AutoReset = false;
            _timer.Elapsed += TimerElapsed;

            Instance = this;
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            if (_stopwatch.Elapsed.TotalMinutes >= Program.Config.GeneralConfig.IdleShutdownMins)
            {
                _logger.LogInformation("Shutting down on idle.");
                _host.StopAsync();
                _timer.Dispose();
            }
            else
            {
                try
                {
                    _timer.Start();
                }
                catch (ObjectDisposedException)
                {
                    // Means this class is shutting down.
                }
            }
        }

        /// <summary>
        /// Call when there is activity to reset the timeout.
        /// </summary>
        public void Activity()
        {
            _stopwatch.Restart();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (Program.Config.GeneralConfig.IdleShutdownMins > 0)
            {
                _logger.LogDebug("Starting idle timer.");
                _timer.Start();
            }
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer.Dispose();
            return Task.CompletedTask;
        }
    }
}
