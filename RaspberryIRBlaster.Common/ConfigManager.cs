using System;
using System.Linq;
using System.IO;
using RaspberryIRBlaster.Common.ConfigObjects;

namespace RaspberryIRBlaster.Common
{
    public class ConfigManager
    {
        private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new System.Text.Json.JsonSerializerOptions()
        {
            IgnoreNullValues = false,
            WriteIndented = true,
            AllowTrailingCommas = true,
            ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip
        };

        private readonly string _generalConfigFilePath;
        private readonly string _remotesDirectoryPath;

        public General GeneralConfig { get; private set; }

        public static bool TryMakeFromCommandLineArgs(string[] args, out ConfigManager configManager)
        {
            var matchingArguments = args
                .Where(arg => arg.StartsWith("--config=", StringComparison.OrdinalIgnoreCase) || args.Any(arg => arg.StartsWith("--config:", StringComparison.OrdinalIgnoreCase)))
                .ToList();

            if (matchingArguments.Count == 0)
            {
                Console.WriteLine("Must specify the config directory location as a command line argument.");
                Console.WriteLine("Example: --config=/etc/RaspberryIRBlaster/");
                configManager = null;
                return false;
            }

            if (matchingArguments.Count > 1)
            {
                Console.WriteLine("Must specify the '--config' option only once.");
                configManager = null;
                return false;
            }

            string path = matchingArguments.Single().Substring(9);
            if (!Directory.Exists(path))
            {
                Console.WriteLine("Config directory does not exist.");
                configManager = null;
                return false;
            }
            configManager = new ConfigManager(path);
            return true;
        }

        public ConfigManager(string configDirectory)
        {
            _remotesDirectoryPath = Path.Combine(configDirectory, "Remotes");
            _generalConfigFilePath = Path.Combine(configDirectory, "General.json");
        }

        /// <summary>
        /// Load/reload the general config into <see cref="GeneralConfig"/>.
        /// </summary>
        public void LoadGeneralConfig()
        {
            var file = new FileInfo(_generalConfigFilePath);
            using (var fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                GeneralConfig = System.Text.Json.JsonSerializer.DeserializeAsync<General>(fileStream, JsonOptions).Result;
            }
        }

        public FileInfo[] GetRemotes()
        {
            var dirListOptions = new EnumerationOptions()
            {
                MatchCasing = MatchCasing.CaseInsensitive,
                MatchType = MatchType.Simple,
                RecurseSubdirectories = false
            };
            DirectoryInfo dir = new DirectoryInfo(_remotesDirectoryPath);
            return dir.GetFiles("*.json", dirListOptions);
        }

        public DirectoryInfo GetRemotesDirectory() => new DirectoryInfo(_remotesDirectoryPath);

        public Remote LoadRemote(string remoteName)
        {
            return LoadRemote(GetRemoteFileInfo(remoteName?.Trim()));
        }

        public Remote LoadRemote(FileInfo file)
        {
            using (var fileStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return System.Text.Json.JsonSerializer.DeserializeAsync<Remote>(fileStream, JsonOptions).Result;
            }
        }

        public FileInfo SaveRemote(Remote remote, string remoteName)
        {
            var file = GetRemoteFileInfo(remoteName);
            SaveRemote(remote, file);
            return file;
        }

        public void SaveRemote(Remote remote, FileInfo file)
        {
            using (var fileStream = file.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
            {
                System.Text.Json.JsonSerializer.SerializeAsync(fileStream, remote, JsonOptions).Wait();
            }
            file.Refresh();
        }

        public FileInfo GetRemoteFileInfo(string remoteName)
        {
            if (string.IsNullOrWhiteSpace(remoteName))
            {
                throw new ArgumentNullException(nameof(remoteName));
            }
            
            if (!Validators.ValidateRemoteName(remoteName))
            {
                throw new ArgumentException("Invalid remote name.");
            }
            string path = Path.Combine(_remotesDirectoryPath, remoteName + ".json");
            return new FileInfo(path);
        }

        public void DeleteRemote(string remoteName)
        {
            var file = GetRemoteFileInfo(remoteName);
            file.Delete();
        }
    }
}
