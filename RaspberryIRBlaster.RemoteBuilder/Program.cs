using System;
using System.Linq;
using static RaspberryIRBlaster.RemoteBuilder.ConsoleUtilities;

namespace RaspberryIRBlaster.RemoteBuilder
{
    class Program
    {
        public static Common.ConfigManager Config { get; private set; }

        static void Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("Raspberry IR Blaster - Remote Builder");

            if (!Common.ConfigManager.TryMakeFromCommandLineArgs(args, out var configManager))
            {
                return;
            }
            Config = configManager;
            Config.LoadGeneralConfig();

            while (true)
            {
                Console.WriteLine();
                Console.WriteLine();
                int action = AskMultipleChoice("What do you want to do?", "Exit", "Clear server cache", "List remotes", "Add a new remote", "Modify a remote", "Delete a remote", "View Raw IR");
                switch (action)
                {
                    case 0:
                        Console.WriteLine("Exiting.");
                        return;
                    case 1:
                        HttpRequestMaker.SignalClearCache();
                        break;
                    case 2:
                        Console.WriteLine("Remote profiles:");
                        var remotes = Config.GetRemotes();
                        if (remotes.Length > 0)
                        {
                            foreach (var file in Config.GetRemotes())
                            {
                                Console.WriteLine("  " + file.FullName);
                            }
                        }
                        else
                        {
                            Console.WriteLine($"No remotes found in '{Config.GetRemotesDirectory()}'.");
                        }
                        Console.WriteLine();
                        Console.WriteLine();
                        break;
                    case 3:
                        new RemoteCreator(Config).Create();
                        break;
                    case 4:
                        ModifyRemote();
                        break;
                    case 5:
                        DeleteRemote();
                        break;
                    case 6:
                        LogRawIR();
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        private static void DeleteRemote()
        {
            var file = PickARemote("Which remote do you want to delete?");
            if (file == null)
            {
                return;
            }
            file.Delete();
            Console.WriteLine("Deleted " + file.FullName);
        }

        private static void ModifyRemote()
        {
            var file = PickARemote("Which remote do you want to modify?");
            if (file == null)
            {
                return;
            }
            var remoteConfig = Config.LoadRemote(file);
            var editor = new RemoteEditor(remoteConfig, System.IO.Path.GetFileNameWithoutExtension(file.Name));
            editor.Edit();
            if (AskYesNo("Do you want to save the changes to this remote?"))
            {
                Config.SaveRemote(remoteConfig, file);
                Console.WriteLine("Saved.");
            }
            else
            {
                Console.WriteLine("Changes discarded.");
            }
        }

        private static System.IO.FileInfo PickARemote(string question)
        {
            var remotes = Config.GetRemotes();
            int deleteIndex = AskMultipleChoiceWithCancelAtMinusOne(question, remotes.Select(x => x.Name));
            if (deleteIndex == -1)
            {
                return null;
            }
            return remotes[deleteIndex];
        }

        private static void LogRawIR()
        {
            var receive = new RaspberryIRDotNet.RX.PulseSpaceConsoleWriter()
            {
                CaptureDevice = IRRXUtilities.RxDevicePath.Value,
            };

            Console.WriteLine("This will log raw IR PULSE/SPACE data to the console.");
            Console.WriteLine("This will run until you any key to stop it.");
            Console.WriteLine("Press any key to start...");
            Console.ReadKey(true);
            Console.WriteLine();
            Console.WriteLine();
            IRRXUtilities.RunIRConsoleWriter(receive);
        }
    }
}
