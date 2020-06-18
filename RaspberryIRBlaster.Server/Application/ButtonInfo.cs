using System;
using RaspberryIRDotNet;

namespace RaspberryIRBlaster.Server.Application
{
    public class ButtonInfo
    {
        public IReadOnlyPulseSpaceDurationList IRData { get; }

        public int Frequency { get; }

        public int DutyCycle { get; }

        public TimeSpan InterButtonSleep { get; }

        public ButtonInfo(IReadOnlyPulseSpaceDurationList irData, int frequency, int dutyCycle, TimeSpan interButtonSleep)
        {
            IRData = irData ?? throw new ArgumentNullException(nameof(irData));
            Frequency = frequency;
            DutyCycle = dutyCycle;
            InterButtonSleep = interButtonSleep;
        }
    }
}
