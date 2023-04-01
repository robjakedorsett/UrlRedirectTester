using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Globalization;

namespace UrlRedirectTester
{
    public class UrlTesterService
    {

        static readonly HttpClient _client = new HttpClient();

        public static async Task TestUrlsAsync(string baseUrl, string csvName)
        {
            var statusObjects = new List<Status>();
            try
            {
                var valid = await ValidateBaseUrlAsync(baseUrl);
                if (!valid) return;

                var validCsvName = await ValidateCsvAsync(csvName);
                if (string.IsNullOrEmpty(validCsvName)) return;

                using (var reader = new StreamReader(validCsvName))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    var records = csv.GetRecordsAsync<Rewrite>();

                    await foreach (var record in records)
                    {
                        statusObjects.Add(await TestUrlAsync(record, baseUrl));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Error Reading CSV - {0}", ex.Message);
                Console.ResetColor();

            }

            if (statusObjects.Any())
            {

                Console.WriteLine($"{statusObjects.Count(x => !x.Success)} Failed Redirect(s) | {statusObjects.Count(x => x.Success)} Successful Redirect(s)");

                foreach (var status in statusObjects.Where(x => !x.Success))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed");
                    Console.WriteLine("Actual {0} | Expected {1} | Old {2} | Status Code {3}", status.Actual, status.NewUrl, status.OldUrl, status.StatusCode);
                    Console.ResetColor();

                }

            }

            Console.ReadLine();
        }

        public static async Task<Status> TestUrlAsync(Rewrite rewrite, string baseUrl)
        {
            try
            {
                using HttpResponseMessage response = await _client.GetAsync($"{baseUrl.TrimEnd('/')}/{rewrite.OldUrl.TrimStart('/')}", HttpCompletionOption.ResponseHeadersRead);

                var location = response.RequestMessage?.RequestUri;
                if (location == null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Bad Url:{0} | Old {1} | New {2}", $"{baseUrl.TrimEnd('/')}/{rewrite.OldUrl.TrimStart('/')}", rewrite.OldUrl, rewrite.NewUrl);
                    Console.ResetColor();
                    return new Status(rewrite.OldUrl, rewrite.NewUrl, false, 0, "");
                }
                if (location.AbsoluteUri.Equals(rewrite.NewUrl, StringComparison.InvariantCultureIgnoreCase)
                    ||
                    location.AbsoluteUri.Equals($"{baseUrl}/{rewrite.NewUrl}", StringComparison.InvariantCultureIgnoreCase))
                {
                    var status = new Status(rewrite.OldUrl, rewrite.NewUrl, true, (int)response.StatusCode, location.AbsoluteUri);

                    if (!response.IsSuccessStatusCode)
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    else
                        Console.ForegroundColor = ConsoleColor.Green;

                    Console.WriteLine("Success | Old {0} | New {1} | Status Code {2}", status.OldUrl, status.NewUrl, status.StatusCode);
                    Console.ResetColor();

                    return status;
                }
                else
                {

                    var status = new Status(rewrite.OldUrl, rewrite.NewUrl, false, (int)response.StatusCode, location.AbsoluteUri);
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Failed | New Actual {0} | New Expected {1} | Old {2} | Status Code {3}", status.Actual, status.NewUrl, status.OldUrl, status.StatusCode);
                    Console.ResetColor();

                    return status;

                }
            }
            catch (HttpRequestException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
                Console.ResetColor();

                return new Status(rewrite.OldUrl, rewrite.NewUrl, false, 0, "");

            }
        }
        private static async Task<string> ValidateCsvAsync(string name)
        {
            name.Replace(".csv", "");
            if (name == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("CSV Name, required");
                Console.ResetColor();
                return string.Empty;

            }
            var csvFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CsvFiles", $"{name}.csv");

            if (!File.Exists(csvFullPath))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Csv not found, Csv should be put into ~/CsvFiles/");
                Console.ResetColor();
                return string.Empty;
            }

            return csvFullPath;
        }
        private static async Task<bool> ValidateBaseUrlAsync(string url)
        {
            if (!url.Contains("http://") && !url.Contains("https://"))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unable to validate base url :Doesn't contain protocol, i.e. http, https");
                Console.ForegroundColor = ConsoleColor.White;
                return false;
            }

            try
            {
                using HttpResponseMessage response = await _client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                return true;

            }
            catch (HttpRequestException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Unable to validate base url :{0} ", e.Message);
                Console.ForegroundColor = ConsoleColor.White;
                return false;

            }
        }


    }

    public class Rewrite
    {
        //[Name(new string[] { "old", "oldUrl", "OldUrl" })]
        [Index(0)]
        public string OldUrl;
        //[Name(new string[] { "new", "newUrl", "NewUrl" })]
        [Index(1)]
        public string NewUrl;
        public Rewrite(string oldUrl, string newUrl)
        {
            OldUrl = oldUrl;
            NewUrl = newUrl;
        }
    }

    public record Status
    {
        public string OldUrl;
        public string NewUrl;
        public string Actual;
        public bool Success;
        public int StatusCode;
        public Status(string oldUrl, string newUrl, bool success, int statusCode, string actual)
        {
            OldUrl = oldUrl;
            NewUrl = newUrl;
            Success = success;
            StatusCode = statusCode;
            Actual = actual;
        }
    }
}
