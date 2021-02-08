using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Identity.Client;

namespace SecureClient
{
    class Program
    {
        static void Main(string[] args)
        {
            RunAsync().GetAwaiter().GetResult();

        }

        private static async Task RunAsync()
        {
            AuthConfig config = AuthConfig.ReadJsonFromFile("appsettings.json");

            IConfidentialClientApplication app;

            app = ConfidentialClientApplicationBuilder.Create(config.ClientId)
                .WithClientSecret(config.ClientSecret)
                .WithAuthority(new Uri(config.Authority))
                .Build();

            string[] resourceIds = new string[] {config.ResourceId};

            AuthenticationResult result = null;

            try
            {
                result = await app.AcquireTokenForClient(resourceIds).ExecuteAsync();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Token aquired \n");
                Console.WriteLine(result.AccessToken);
                Console.ResetColor();
            }

            catch (MsalClientException ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.Message);
                Console.ResetColor();
            }

            if (!string.IsNullOrEmpty(result.AccessToken))
            {
                var httpClient = new HttpClient();
                var defaultRequestHeaders = httpClient.DefaultRequestHeaders;

                if (defaultRequestHeaders.Accept == null || !defaultRequestHeaders.Accept.Any (m => m.MediaType == "application/json"))
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                }

                defaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("bearer", result.AccessToken);

                HttpResponseMessage response = await httpClient.GetAsync(config.BaseAddress);

                if (response.IsSuccessStatusCode)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    string json = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(json);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to call API: {response.StatusCode}");
                    string content = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Content: {content}");
                }
                Console.ResetColor();
            }

        }
    }
}
