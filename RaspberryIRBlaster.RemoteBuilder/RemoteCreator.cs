using System;
using System.IO;
using System.Linq;
using static RaspberryIRBlaster.RemoteBuilder.ConsoleUtilities;

namespace RaspberryIRBlaster.RemoteBuilder
{
    class RemoteCreator
    {
        private readonly Common.ConfigManager _config;

        public RemoteCreator(Common.ConfigManager configManager)
        {
            _config = configManager;
        }

        public void Create()
        {
            var remote = MakeNewRemote();
            FileInfo file = AskForNameToSaveWith();
            if (file == null)
            {
                return;
            }
            _config.SaveRemote(remote, file);
            Console.WriteLine("New remote profile saved to " + file.FullName);
            Console.WriteLine("To add buttons to this new remote modify it.");
        }

        private void PressAnyKeyToBegin()
        {
            Console.WriteLine("Press any key to begin...");
            ClearInputBuffer();
            Console.ReadKey(true);
            Console.WriteLine();
        }

        private Common.ConfigObjects.Remote MakeNewRemote()
        {
            Console.WriteLine("Creating a new remote control profile.");
            int unitDuration;
            RaspberryIRDotNet.PulseSpaceUnitList leadIn;
            if (AskYesNo("Do you know the unit duration and lead-in pattern?"))
            {
                Console.WriteLine();
                Console.WriteLine("Specify (in microseconds) how long a unit lasts for.");
                Console.WriteLine("A typical value is something in the region of 200-800.");
                unitDuration = AskForInteger(10, 500000);
                Console.WriteLine();
                Console.WriteLine("Specify the lead-in pattern as the number of units ON, then the number of units");
                Console.WriteLine("OFF. For example if the lead-in pattern is 8 units on, 4 units off, 6 units on,");
                Console.WriteLine("6 units off then enter that as '8,4,6,6'. A lead-in is typically just one ON/OFF");
                Console.WriteLine("cycle like '8,4'.");
                Console.Write(">");
                string leadInText = Console.ReadLine();
                leadIn = RaspberryIRDotNet.PulseSpaceUnitList.LoadFromString(leadInText);
                if (leadIn.Count <= 0)
                {
                    throw new Exception("Need a lead-in.");
                }
            }
            else
            {
                Console.WriteLine("The application will estimate the lead-in and unit duration from the IR signals.");

                Console.WriteLine();

                var leadInLearner = new RaspberryIRDotNet.RX.LeadInLearner()
                {
                    CaptureDevice = IRRXUtilities.RxDevicePath.Value
                };
                leadInLearner.Received += (s, e) => Console.Write("#");

                Console.WriteLine("This step will try to learn the lead-in pattern that prefixes each IR message.");
                Console.WriteLine("Press buttons on the remote at random (but don't hold them). A # will appear");
                Console.WriteLine("each time a signal is recognised (either from the remote or just noise).");
                PressAnyKeyToBegin();

                var leadInDurations = leadInLearner.LearnLeadInDurations();

                Console.WriteLine();
                Console.WriteLine();

                var unitDurationLearner = new RaspberryIRDotNet.RX.UnitDurationLearner()
                {
                    CaptureDevice = IRRXUtilities.RxDevicePath.Value,
                    LeadInPatternDurations = leadInDurations,
                };
                IRRXUtilities.SetUpRxFeedback(unitDurationLearner);

                Console.WriteLine("This step will try to learn the unit duration of the IR message.");
                IRRXUtilities.WriteButtonPressInstructions("any button", true);
                PressAnyKeyToBegin();

                unitDuration = unitDurationLearner.LearnUnitDuration();
                Console.WriteLine();
                Console.WriteLine($"Done, unit duration is: {unitDuration} microseconds.");
                int keepDuration = AskMultipleChoice("Do you want to keep this unit duration, or enter another duration manually?", "Keep learned duration", "Enter another duration manually");
                if (keepDuration == 1)
                {
                    Console.WriteLine("Enter the unit duration in microseconds.");
                    unitDuration = AskForInteger(10, 500000);
                }

                leadIn = new RaspberryIRDotNet.PulseSpaceUnitList(unitDuration, leadInDurations);
            }

            int minUnits, maxUnits;
            while (true)
            {
                Console.WriteLine("Enter the minimum number of units that an IR message can contain. Any received");
                Console.WriteLine("IR signals with fewer than this many units will be discarded as noise.");
                Console.WriteLine("If you don't know what to use then 50 is a good value.");
                minUnits = AskForInteger(1, 9999);
                Console.WriteLine();
                Console.WriteLine("Enter the maximum number of units that an IR message can contain. Any received");
                Console.WriteLine("IR signals with more than this many units will be discarded as noise.");
                Console.WriteLine("This can be equal to the minimum if the number of units does not vary.");
                Console.WriteLine("If you don't know what to use then 200 is a good value.");
                maxUnits = AskForInteger(1, 9999);

                if (minUnits > maxUnits)
                {
                    Console.WriteLine();
                    Console.WriteLine("Minimum must be less than or equal to the maximum.");
                    Console.WriteLine("Try again...");
                    Console.WriteLine();
                }
                else
                {
                    break;
                }
            }



            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine($"Enter the duty cycle (1-99). Leave blank for default ({RaspberryIRDotNet.IRDeviceDefaults.DutyCycle}).");
            Console.WriteLine("Use -1 to indicate that the IR driver should not be told the duty cycle.");
            int dutyCycle = AskForIntegerWithDefault(-1, 99, RaspberryIRDotNet.IRDeviceDefaults.DutyCycle);

            Console.WriteLine();
            Console.WriteLine($"Enter the frequency in Hz. Leave blank for default ({RaspberryIRDotNet.IRDeviceDefaults.Frequency}).");
            Console.WriteLine("Use -1 to indicate that the IR driver should not be told the frequency.");
            int frequency;
            while (true)
            {
                frequency = AskForIntegerWithDefault(-1, 90000, RaspberryIRDotNet.IRDeviceDefaults.Frequency);
                if (frequency > -1 && frequency < 1000)
                {
                    Console.WriteLine("Specify the frequency in Hz, not KHz!");
                }
                else
                {
                    break;
                }
            }

            const int defaultSleepTime = 200;
            Console.WriteLine();
            Console.WriteLine("It is a good idea to add a delay after each button press as the receiving device");
            Console.WriteLine("likely won't be able to handle IR codes sent in very quick succession.");
            Console.WriteLine($"Enter the delay period in milliseconds. Leave blank for default ({defaultSleepTime}).");
            int interButtonSleep = AskForIntegerWithDefault(0, RaspberryIRBlaster.Common.ConfigObjects.Remote.InterButtonSleepMilliseconds_Max, defaultSleepTime);

            return new Common.ConfigObjects.Remote()
            {
                UnitDuration = unitDuration,
                LeadIn = leadIn.SaveToString(),
                MinimumUnitCount = minUnits,
                MaximumUnitCount = maxUnits,
                DutyCycle = dutyCycle,
                Frequency = frequency,
                InterButtonSleepMilliseconds = interButtonSleep,
            };
        }

        private FileInfo AskForNameToSaveWith()
        {
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("The new remote profile is ready to save.");
            Console.WriteLine("Enter a name for the remote, or leave blank to discard it.");
            while (true)
            {
                Console.Write(">");
                ClearInputBuffer();
                string name = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(name))
                {
                    if (AskYesNo("Really discard the new remote?"))
                    {
                        return null;
                    }
                    Console.WriteLine("Enter a name for the remote.");
                }
                else if (!Common.Validators.ValidateRemoteName(name))
                {
                    Console.WriteLine("Name is invalid.");
                }
                else
                {
                    var fileInfo = _config.GetRemoteFileInfo(name);
                    if (fileInfo.Exists)
                    {
                        if (AskYesNo("A remote with this name already exists. Replace it?"))
                        {
                            return fileInfo;
                        }
                        Console.WriteLine("Enter another name for the remote.");
                    }
                    else
                    {
                        return fileInfo;
                    }
                }
            }
        }

    }
}
