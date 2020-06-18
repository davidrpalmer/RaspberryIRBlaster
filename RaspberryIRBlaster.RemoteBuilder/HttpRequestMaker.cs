using System;
using System.Net.Http;

namespace RaspberryIRBlaster.RemoteBuilder
{
    static class HttpRequestMaker
    {
        private static string _lastRootUrl = null;

        private static readonly HttpClient _httpClient = new HttpClient();

        private static string GetRootUrl()
        {
            if (string.IsNullOrWhiteSpace(_lastRootUrl))
            {
                _lastRootUrl = Program.Config.GeneralConfig.ListenAtUrl.Replace("*", "localhost").Replace("0.0.0.0", "localhost");
            }
            Console.Write($"Enter root URL [{_lastRootUrl}] : ");
            string newUrl = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newUrl))
            {
                _lastRootUrl = newUrl;
            }

            return _lastRootUrl;
        }

        public static void SignalClearCache()
        {
            string url = GetRootUrl() + "/Maintenance/ClearCache";
            try
            {
                Console.WriteLine("Making HTTP request...");
                Console.WriteLine("  " + url);
                var task = _httpClient.PostAsync(url, null);
                task.Wait();
                if (task.Result.StatusCode != System.Net.HttpStatusCode.OK && task.Result.StatusCode != System.Net.HttpStatusCode.NoContent)
                {
                    Console.WriteLine($"HTTP request returned an error! - {(int)task.Result.StatusCode} {task.Result.ReasonPhrase}");
                }
                else
                {
                    Console.WriteLine("Clear cache request sent.");
                }
            }
            catch (AggregateException aggErr) when (aggErr.InnerException is HttpRequestException err && aggErr.InnerExceptions.Count <= 1)
            {
                Console.WriteLine("HTTP request failed! - " + err.Message);
                Console.WriteLine("Details:");
                Console.WriteLine(err.ToString());
                Console.WriteLine();
            }
        }
    }
}
