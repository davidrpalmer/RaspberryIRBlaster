using System;

namespace RaspberryIRBlaster.Client.BatchActions
{
    public class SleepAction : IBatchAction<int>
    {
        public string Type => "Sleep";

        public int Data { get; set; }

        public SleepAction()
        {
        }

        public SleepAction(TimeSpan sleepTime)
        {
            Data = (int)sleepTime.TotalMilliseconds;
        }
    }
}
