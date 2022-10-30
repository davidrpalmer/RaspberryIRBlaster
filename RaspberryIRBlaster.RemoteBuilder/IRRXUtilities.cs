using System;

namespace RaspberryIRBlaster.RemoteBuilder
{
    static class IRRXUtilities
    {
        public static Lazy<string> RxDevicePath { get; } = new Lazy<string>(GetRxDevice);

        private static string GetRxDevice()
        {
            if (!string.IsNullOrWhiteSpace(Program.Config.GeneralConfig.IRRXDevice))
            {
                return Program.Config.GeneralConfig.IRRXDevice;
            }
            else
            {
                return new RaspberryIRDotNet.DeviceAssessment.DeviceAssessor().GetPathToTheReceiverDevice();
            }
        }

        public static void SetUpRxFeedback(RaspberryIRDotNet.RX.IMultipleCapture ir)
        {
            ir.Waiting += (s, e) => Console.Write(">");
            ir.Hit += (s, e) => Console.WriteLine(" +");
            ir.Miss += (s, e) => Console.Write(" -");
        }

        /// <param name="keyInfo">Examples: "any button", the "the 5 button"</param>
        public static void WriteButtonPressInstructions(string keyInfo, bool varyKeys)
        {
            Console.WriteLine($"When the '>' symbol appears press {keyInfo} on the remote.");
            if (varyKeys)
            {
                Console.WriteLine($"Press different keys at random.");
            }
            Console.WriteLine("If there is an error then a '-' will appear, try pressing the button again.");
            Console.WriteLine("Once the signal has been received a '+' will appear. When this happens let");
            Console.WriteLine("go of the button. Do not let go until you see the '+'.");
            Console.WriteLine("Wait for the '>' symbol to appear again and repeat.");
        }

        public static void RunIRConsoleWriter(RaspberryIRDotNet.RX.IIRConsoleWriter receive)
        {
            Console.WriteLine("This will run until you any key to stop it.");
            Console.WriteLine("Press any key to start...");
            Console.ReadKey(true);
            Console.WriteLine();
            Console.WriteLine();
            var cancellationToken = new RaspberryIRDotNet.ReadCancellationToken();
            var thread = new System.Threading.Thread(() =>
            {
                try
                {
                    receive.Start(cancellationToken);
                }
                catch (OperationCanceledException)
                {
                }
            })
            {
                Name = "IR Logger",
                IsBackground = true
            };
            thread.Start();

            Console.ReadKey(true);
            cancellationToken.Cancel(wait: true);
            Console.WriteLine("IR logger stopped.");
        }
    }
}
