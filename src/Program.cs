using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PrefixSearch.Services.Clients;
using PrefixSearch.Workspace;

namespace PrefixSearch
{
    class Program
    {
        static async Task<int> Main()
        {
            var builder = new HostBuilder().ConfigureAppConfiguration((host, config) => {
                config.SetBasePath(Environment.CurrentDirectory);
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
                config.AddEnvironmentVariables();
            }).ConfigureServices((host, services) => {
                services.AddHttpClient();
                services.Configure<CloudProviderClientSettings>(host.Configuration.GetSection(nameof(CloudProviderClientSettings)));
                services.Configure<WorkspaceSettings>(host.Configuration.GetSection(nameof(WorkspaceSettings)));
                services.AddTransient<IAwsRangeClient, AwsRangeClient>();
                services.AddTransient<IAzureRangeClient, AzureRangeClient>();
            }).UseConsoleLifetime();
            var host = builder.Build();

            using var serviceScope = host.Services.CreateScope();
            var services = serviceScope.ServiceProvider;

            try
            {
                var test = new TestMethods(services);
                var map = await test.RetrieveIPv4PrefixMap();
                test.SearchIPv4PrefixMap(map);
            }
            catch (Exception e)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(e, "An unknown error occured.");
            }

            return 0;
        }
    }
}
