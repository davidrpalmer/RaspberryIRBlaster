using System;
using System.Linq;
using System.Threading;

namespace RaspberryIRBlaster.Server.Application.Actions
{
    class SendMessageAction : IAction
    {
        private readonly string _remoteName;
        private readonly string _buttonName;
        private ButtonInfo _buttonInfo;

        public SendMessageAction(string messageName)
        {
            if (string.IsNullOrWhiteSpace(messageName))
            {
                throw new ArgumentNullException(nameof(messageName));
            }

            if (messageName.Count(c => c == '.') != 1)
            {
                throw new ArgumentException("Message name must contain exactly one dot to separate the remote name from the button name.");
            }
            string[] parts = messageName.Split('.');
            _remoteName = parts[0];
            _buttonName = parts[1];

            if (string.IsNullOrWhiteSpace(_remoteName))
            {
                throw new ArgumentException("The remote name (before the dot) is required.");
            }

            if (string.IsNullOrWhiteSpace(_buttonName))
            {
                throw new ArgumentException("The button name (after the dot) is required.");
            }
        }

        public void Execute(IActionContext context)
        {
            context.SendRemoteButton(_buttonInfo);
            if (_buttonInfo.InterButtonSleep > TimeSpan.Zero)
            {
                Thread.Sleep(_buttonInfo.InterButtonSleep);
            }
        }

        public void Prepare(IActionContext context)
        {
            _buttonInfo = context.GetRemoteButtonInfo(_remoteName, _buttonName);
        }
    }
}
