using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace RaspberryIRBlaster.Server.Application
{
    public class RemoteButtonCache
    {
        private readonly Dictionary<string, CachedRemote> _cachedButtons = new Dictionary<string, CachedRemote>();
        private readonly object _locker = new object();
        private readonly ILogger<RemoteButtonCache> _logger;
        private readonly Common.ConfigManager _configManager;

        public RemoteButtonCache(ILogger<RemoteButtonCache> logger, Common.ConfigManager configManager)
        {
            _logger = logger;
            _configManager = configManager;
        }

        /// <summary>
        /// Get a button from the cache, if the remote is not loaded then load it and cache it for next time.
        /// </summary>
        public ButtonInfo GetButton(string remoteName, string buttonName)
        {
            if (string.IsNullOrWhiteSpace(remoteName))
            {
                throw new ArgumentNullException(nameof(remoteName));
            }
            if (string.IsNullOrWhiteSpace(buttonName))
            {
                throw new ArgumentNullException(nameof(buttonName));
            }

            lock (_locker)
            {
                CachedRemote cachedRemote;
                if (!_cachedButtons.TryGetValue(remoteName, out cachedRemote))
                {
                    _logger.LogDebug($"Remote '{remoteName}' was not in the cache. Trying to load it.");
                    Common.ConfigObjects.Remote remoteConfig;
                    try
                    {
                        remoteConfig = _configManager.LoadRemote(remoteName);
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        throw new HttpResponseException(System.Net.HttpStatusCode.NotFound, "Remote not found.");
                    }
                    cachedRemote = new CachedRemote(remoteConfig);
                    _cachedButtons.Add(remoteName, cachedRemote);
                    _logger.LogInformation($"Remote '{remoteName}' loaded into the cache.");
                }

                var irData = cachedRemote.GetButtonData(buttonName);
                return new ButtonInfo(irData.PulseSpaceDurations, cachedRemote.Frequency, cachedRemote.DutyCycle, cachedRemote.InterButtonSleep);
            }
        }

        /// <summary>
        /// Remove all remotes from the cache.
        /// </summary>
        public void Clear()
        {
            lock (_locker)
            {
                _cachedButtons.Clear();
                _logger.LogInformation("Remote button cache cleared.");
            }
        }

        /// <summary>
        /// Remove a specific remote from the cache, if it is there.
        /// </summary>
        public void RemoveRemote(string remoteName)
        {
            lock (_locker)
            {
                if (_cachedButtons.Remove(remoteName))
                {
                    _logger.LogInformation($"Remote '{remoteName}' removed from the cache.");
                }
                else
                {
                    _logger.LogInformation($"Remote '{remoteName}' was not in the cache.");
                }
            }
        }
    }
}
