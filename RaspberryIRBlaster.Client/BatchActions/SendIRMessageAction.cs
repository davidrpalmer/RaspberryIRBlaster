using System;

namespace RaspberryIRBlaster.Client.BatchActions
{
    public class SendIRMessageAction : IBatchAction<string>
    {
        public string Type => "SendMessage";

        /// <summary>
        /// The name of the IR message to send and the remote it belongs to. Format is Remote.Button. Example: TV.EPG to send the IR code for the EPG button to the TV.
        /// </summary>
        public string Data { get; set; }

        public SendIRMessageAction()
        {
        }

        public SendIRMessageAction(string remoteName, string buttonName)
        {
            if (string.IsNullOrWhiteSpace(remoteName)) { throw new ArgumentNullException(nameof(remoteName)); }
            if (string.IsNullOrWhiteSpace(buttonName)) { throw new ArgumentNullException(nameof(buttonName)); }

            Data = remoteName + "." + buttonName;
        }
    }
}
