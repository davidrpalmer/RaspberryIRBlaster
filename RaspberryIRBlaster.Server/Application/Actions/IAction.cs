using System;

namespace RaspberryIRBlaster.Server.Application.Actions
{
    public interface IAction
    {
        void Execute(IActionContext context);

        void Prepare(IActionContext context);
    }

    public interface IActionContext
    {
        ButtonInfo GetRemoteButtonInfo(string remoteName, string buttonName);

        void SendRemoteButton(ButtonInfo button);
    }
}
