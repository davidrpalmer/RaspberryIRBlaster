using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RaspberryIRDotNet.TX;
using RaspberryIRBlaster.Server.Application.Actions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;

namespace RaspberryIRBlaster.Server.Application
{
    public class IRTransmitter : IHostedService
    {
        public static IRTransmitter Instance { get; private set; }

        private readonly ILogger<IRTransmitter> _logger;
        private readonly PulseSpaceTransmitter_ManualOpenClose _irInterface = new PulseSpaceTransmitter_ManualOpenClose();
        private readonly object _locker = new object();
        private readonly ActionContext _actionContext;
        private volatile bool _abort = false;
        private readonly RemoteButtonCache _remoteButtonCache;

        private Task _workerTask;

        public IRTransmitter(IServiceProvider services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            _logger = services.GetRequiredService<ILogger<IRTransmitter>>();
            _remoteButtonCache = new RemoteButtonCache(services.GetRequiredService<ILogger<RemoteButtonCache>>(), Program.Config);

            _actionContext = new ActionContext(this);

            Instance = this;
        }

        public void Run(ICollection<IAction> actions)
        {
            if (actions == null) { throw new ArgumentNullException(nameof(actions)); }

            _abort = false;
            Task taskToWaitOn;
            lock (_locker)
            {
                if (_workerTask != null && _workerTask.Status < TaskStatus.RanToCompletion)
                {
                    throw new HttpResponseException(System.Net.HttpStatusCode.Locked, "IR device is in use.");
                }

                _logger.LogTrace("Begin running actions.");
                if (_abort)
                {
                    _logger.LogDebug("Abort run before started.");
                    return;
                }

                _workerTask = new Task(() => RunWorker(actions));
                taskToWaitOn = _workerTask;
                _workerTask.Start();
            }

            taskToWaitOn.Wait();
        }

        private void RunWorker(ICollection<IAction> actions)
        {
            if (_abort) // See if an abort request came in while we were waiting for the lock.
            {
                _logger.LogDebug("Abort run before start actions.");
                return;
            }

            foreach (var action in actions)
            {
                if (_abort)
                {
                    _logger.LogDebug("Abort run while preparing actions.");
                    break;
                }
                action.Prepare(_actionContext);
            }

            _logger.LogTrace($"Opening IR device '{_irInterface.TransmissionDevice}'.");
            _irInterface.Open();
            try
            {
                foreach (var action in actions)
                {
                    if (_abort)
                    {
                        _logger.LogDebug("Abort run while executing actions.");
                        break;
                    }
                    action.Execute(_actionContext);
                }
            }
            finally
            {
                _irInterface.Close();
                _logger.LogTrace("Closed IR device.");
            }
        }

        public void Abort()
        {
            _logger.LogDebug("Abort request.");
            _abort = true;
            lock (_locker)
            {
                _logger.LogTrace("Got the lock for aborting.");
                if (_workerTask != null && _workerTask.Status < TaskStatus.RanToCompletion)
                {
                    _workerTask.Wait();
                    _logger.LogTrace("Thread has been stopped.");
                }
            }
            _logger.LogDebug("Finished abort.");
        }

        public void ClearRemoteCache()
        {
            _remoteButtonCache.Clear();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Start request.");

            string irDevice = Program.Config.GeneralConfig.IRTXDevice;
            if (string.IsNullOrEmpty(irDevice))
            {
                irDevice = new RaspberryIRDotNet.DeviceAssessment.DeviceAssessor().GetPathToTheTransmitterDevice();
            }
            _irInterface.TransmissionDevice = irDevice;
            _logger.LogInformation($"IR transmission device: {irDevice}");

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stop request.");
            Abort();
            _irInterface.Dispose();
            _logger.LogInformation("Stopped.");
            return Task.CompletedTask;
        }

        class ActionContext : IActionContext
        {
            private readonly IRTransmitter _parent;
            public ActionContext(IRTransmitter parent)
            {
                _parent = parent;
            }

            public ButtonInfo GetRemoteButtonInfo(string remoteName, string buttonName)
            {
                return _parent._remoteButtonCache.GetButton(remoteName, buttonName);
            }

            public void SendRemoteButton(ButtonInfo buttonInfo)
            {
                var irInterface = _parent._irInterface;

                bool changedConfig = false;
                if (irInterface.Frequency != buttonInfo.Frequency)
                {
                    irInterface.Frequency = buttonInfo.Frequency;
                    changedConfig = true;
                }
                if (irInterface.DutyCycle != buttonInfo.DutyCycle)
                {
                    irInterface.DutyCycle = buttonInfo.DutyCycle;
                    changedConfig = true;
                }
                if (changedConfig)
                {
                    irInterface.ApplySettings();
                }
                irInterface.Send(buttonInfo.IRData);
            }
        }
    }
}
