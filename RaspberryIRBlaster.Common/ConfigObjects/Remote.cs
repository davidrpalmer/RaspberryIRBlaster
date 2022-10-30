using System;
using System.Collections.Generic;

namespace RaspberryIRBlaster.Common.ConfigObjects
{
    public class Remote
    {
        public int UnitDuration { get; set; }

        #region Learning config - stuff we don't need for playback, but do need to learn new buttons in the future.
        public string LeadIn { get; set; }

        public int MinimumUnitCount { get; set; } = 0;
        #endregion

        #region Playback config - stuff we don't need for learning new buttons, but do need to play them back.
        public int DutyCycle { get; set; } = -1;

        public int Frequency { get; set; } = -1;

        public static readonly int InterButtonSleepMilliseconds_Max = 3000;

        public int InterButtonSleepMilliseconds { get; set; } = 400;
        #endregion

        public List<Button> Buttons { get; set; } = new List<Button>();
    }
}
