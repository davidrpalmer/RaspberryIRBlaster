using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RaspberryIRBlaster.Server
{
    public static class Program
    {
        public static Common.ConfigManager Config { get; private set; }

        public static Application.IRTransmitter IRTransmitter { get; private set; }

        private const int ExitCode_BadArgs = 1;
        private const int ExitCode_BadConfig = 2;

        public static int Main(string[] args)
        {
            Console.WriteLine();
            Console.WriteLine("Raspberry IR Blaster - Server");
            Console.WriteLine();

            if (!Common.ConfigManager.TryMakeFromCommandLineArgs(args, out var configManager))
            {
                return ExitCode_BadArgs;
            }
            Config = configManager;
            Config.LoadGeneralConfig();

            if (string.IsNullOrWhiteSpace(Config.GeneralConfig.ListenAtUrl))
            {
                Console.WriteLine($"Must specify the '{nameof(Config.GeneralConfig.ListenAtUrl)}' option in the config.");
                return ExitCode_BadConfig;
            }

            var appHost = CreateHostBuilder(args).Build();
            IRTransmitter = new Application.IRTransmitter(Config, appHost.Services);
            appHost.Run();
            return 0;
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            string appDir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);

            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                    {
                        // Without this the working directory can affect where appsettings.json is loaded from.
                        config.SetBasePath(appDir);
                    })
                .UseSystemd()
                .UseContentRoot(appDir) // Not using any disk content in this app, but got this incase it is used in the future.
                .ConfigureWebHostDefaults(webBuilder =>
                    {
                        webBuilder.UseStartup<Startup>();
                        webBuilder.UseLibuv(); // Needed for systemd socket activation to work
                        webBuilder.ConfigureKestrel(options =>
                        {
                            options.UseSystemd();
                        });
                        webBuilder.UseUrls(Config.GeneralConfig.ListenAtUrl);
                    });
        }
    }
}
