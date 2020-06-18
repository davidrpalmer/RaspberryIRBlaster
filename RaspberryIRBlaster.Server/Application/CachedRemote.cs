using System;
using System.Collections.Generic;

namespace RaspberryIRBlaster.Server.Application
{
    public class CachedRemote
    {
        private readonly Dictionary<string, RaspberryIRDotNet.IRPulseMessage> _cachedButtons = new Dictionary<string, RaspberryIRDotNet.IRPulseMessage>();

        public int Frequency { get; }

        public int DutyCycle { get; }

        public TimeSpan InterButtonSleep { get; }

        public CachedRemote(Common.ConfigObjects.Remote remoteConfig)
        {
            Frequency = remoteConfig.Frequency;
            DutyCycle = remoteConfig.DutyCycle;
            if (remoteConfig.InterButtonSleepMilliseconds > Common.ConfigObjects.Remote.InterButtonSleepMilliseconds_Max)
            {
                throw new Exception("Inter-button sleep period too long.");
            }
            InterButtonSleep = TimeSpan.FromMilliseconds(remoteConfig.InterButtonSleepMilliseconds);

            foreach (var buttonConfig in remoteConfig.Buttons)
            {
                var irData = new RaspberryIRDotNet.IRPulseMessage(buttonConfig.ReadData(), remoteConfig.UnitDuration);
                _cachedButtons.Add(buttonConfig.Name, irData);
            }
        }

        public RaspberryIRDotNet.IRPulseMessage GetButtonData(string buttonName)
        {
            if (_cachedButtons.TryGetValue(buttonName, out var data))
            {
                return data;
            }

            throw new HttpResponseException(System.Net.HttpStatusCode.NotFound, "Button not found.");
        }
    }
}
