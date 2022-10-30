using System;
using System.Linq;
using static RaspberryIRBlaster.RemoteBuilder.ConsoleUtilities;

namespace RaspberryIRBlaster.RemoteBuilder
{
    class RemoteEditor
    {
        public Common.ConfigObjects.Remote RemoteConfig { get; }

        private readonly string _remoteName;

        public RemoteEditor(Common.ConfigObjects.Remote remoteConfig, string remoteName)
        {
            if (remoteConfig == null) { throw new ArgumentNullException(nameof(remoteConfig)); }
            RemoteConfig = remoteConfig;
            _remoteName = remoteName;
        }

        public void Edit()
        {
            while (true)
            {
                Console.WriteLine();
                Console.WriteLine();
                Console.WriteLine($"Editing remote '{_remoteName}'.");
                int action = AskMultipleChoice("What do you want to do?", "Exit remote config", "Show remote properties and buttons", "Add/Update button", "Remove button", "View Raw IR (with this remote's filtering options)");
                switch (action)
                {
                    case 0:
                        return;
                    case 1:
                        Show();
                        break;
                    case 2:
                        AddUpdateButton();
                        break;
                    case 3:
                        RemoveButton();
                        break;
                    case 4:
                        LogRawIR();
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        private void Show()
        {
            Console.WriteLine("Lead In Pattern:    " + string.Join(",", RemoteConfig.LeadIn));
            Console.WriteLine("Minimum Unit Count: " + RemoteConfig.MinimumUnitCount);
            Console.WriteLine("Unit Duration:      " + RemoteConfig.UnitDuration + " microseconds");
            Console.WriteLine("Frequency:          " + RemoteConfig.Frequency + " Hz");
            Console.WriteLine("Duty Cycle:         " + RemoteConfig.DutyCycle + "%");
            Console.WriteLine("Inter button sleep: " + RemoteConfig.InterButtonSleepMilliseconds + " milliseconds");

            if (RemoteConfig.Buttons.Count > 0)
            {
                Console.WriteLine($"There are {RemoteConfig.Buttons.Count} buttons in this remote:");
                foreach (var button in RemoteConfig.Buttons.OrderBy(x => x.Name))
                {
                    Console.WriteLine("  " + button.Name);
                }
            }
            else
            {
                Console.WriteLine("There are no buttons in this remote.");
            }
        }

        private void RemoveButton()
        {
            Console.WriteLine();
            Console.Write("Enter button name to remove: ");
            string name = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }
            int count = RemoteConfig.Buttons.RemoveAll(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (count == 0)
            {
                Console.WriteLine("No button with that name was found.");
            }
            else if (count == 1)
            {
                Console.WriteLine("Button removed.");
            }
            else
            {
                Console.WriteLine("Multiple buttons with that name were found and have been removed.");
            }
        }

        private void AddUpdateButton()
        {
            bool firstTime = true;
            while (true)
            {
                string name;
                while (true)
                {
                    Console.WriteLine();
                    if (firstTime)
                    {
                        Console.WriteLine("Enter the button name to add/update. Or leave blank to cancel.");
                    }
                    else
                    {
                        Console.WriteLine("Enter the next button name to add/update. Or leave blank to finish.");
                    }
                    Console.Write(">");
                    ClearInputBuffer();
                    name = Console.ReadLine();
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        Console.WriteLine("Cancelling add/update button.");
                        return;
                    }
                    if (!Common.Validators.ValidateButtonName(name))
                    {
                        Console.WriteLine("Invalid name format.");
                    }
                    else
                    {
                        break;
                    }
                }
                var foundButtons = RemoteConfig.Buttons.FindAll(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
                if (foundButtons.Count == 0)
                {
                    // Add new button
                    Console.WriteLine("Adding new button.");
                    RemoteConfig.Buttons.Add(new Common.ConfigObjects.Button(name, LearnButton().PulseSpaceUnits));
                    Console.WriteLine("Button added.");
                }
                else if (foundButtons.Count == 1)
                {
                    // Update existing button
                    var button = foundButtons.Single();
                    if (!AskYesNo($"Replace existing button '{button.Name}'?"))
                    {
                        return;
                    }
                    button.WriteData(LearnButton().PulseSpaceUnits);
                    Console.WriteLine("Button updated.");
                }
                else
                {
                    throw new Exception("Corrupted config: duplicate button names.");
                }

                firstTime = false;
            }
        }

        private RaspberryIRDotNet.IRPulseMessage LearnButton()
        {
            var ir = new RaspberryIRDotNet.RX.IRMessageLearn(IRRXUtilities.RxDevicePath.Value)
            {
                MinimumPulseSpaceCount = RemoteConfig.MinimumUnitCount,
                UnitDurationMicrosecs = RemoteConfig.UnitDuration,
            };
            ir.SetLeadInPatternFilterByUnits(RaspberryIRDotNet.PulseSpaceUnitList.LoadFromString(RemoteConfig.LeadIn));

            IRRXUtilities.SetUpRxFeedback(ir);
            IRRXUtilities.WriteButtonPressInstructions("the button", false);

            return ir.LearnMessage();
        }

        private void LogRawIR()
        {
            var receive = new RaspberryIRDotNet.RX.FilteredPulseSpaceConsoleWriter(IRRXUtilities.RxDevicePath.Value)
            {
                UnitDurationMicrosecs = RemoteConfig.UnitDuration,
            };
            receive.SetLeadInPatternFilterByUnits(RaspberryIRDotNet.PulseSpaceUnitList.LoadFromString(RemoteConfig.LeadIn));

            Console.WriteLine("This will log & filter the IR to only what appears to be valid for this remote.");
            Console.WriteLine("The signal is first cleaned up (using the unit duration) then it is filtered by");
            Console.WriteLine("the lead-in pattern. The minimum unit count is ignored.");
            Console.WriteLine("From left to right the columns are:");
            Console.WriteLine("  Type (PULSE / SPACE)");
            Console.WriteLine("  Raw duration");
            Console.WriteLine("  Rounded duration");
            Console.WriteLine("  Number of units");
            Console.WriteLine();
            IRRXUtilities.RunIRConsoleWriter(receive);
        }
    }
}
