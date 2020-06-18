using System;
using System.Threading;

namespace RaspberryIRBlaster.Server.Application.Actions
{
    class SleepAction : IAction
    {
        private readonly TimeSpan _sleepTime;

        public SleepAction(TimeSpan sleepTime)
        {
            _sleepTime = sleepTime;
        }

        public void Execute(IActionContext context)
        {
            Thread.Sleep(_sleepTime);
        }

        public void Prepare(IActionContext context)
        {
        }
    }
}
