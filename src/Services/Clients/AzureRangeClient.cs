using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PrefixSearch.Data.Azure;
using PrefixSearch.Data.Converters;

namespace PrefixSearch.Services.Clients
{
    public class AzureRangeClient : IAzureRangeClient
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly IOptions<CloudProviderClientSettings> settings;

        public AzureRangeClient(IHttpClientFactory clientFactory, IOptions<CloudProviderClientSettings> settings)
        {
            this.clientFactory = clientFactory;
            this.settings = settings;
        }

        public async Task<AzureIPRange> GetAzureIPRange()
        {
            var downloadUriRequest = new HttpRequestMessage(HttpMethod.Get, settings.Value.AzureIpRangeDownloadConfirmationUrl) { Version = new Version(2, 0) };
            var client = clientFactory.CreateClient();
            var downloadUriResponse = await client.SendAsync(downloadUriRequest);

            if (!downloadUriResponse.IsSuccessStatusCode) { return null; }

            var downloadUriContent = await downloadUriResponse.Content.ReadAsStringAsync();
            var matches = Regex.Match(downloadUriContent, settings.Value.AzureServiceTagRegexPattern);
            if (matches.Success && Uri.TryCreate(matches.Value, UriKind.Absolute, out Uri downloadUri))
            {
                var request = new HttpRequestMessage(HttpMethod.Get, downloadUri) { Version = new Version(2, 0) };
                var response = await client.SendAsync(request);

                if (!response.IsSuccessStatusCode) { return null; }

                using var contentStream = await response.Content.ReadAsStreamAsync();
                return await JsonSerializer.DeserializeAsync<AzureIPRange>(contentStream, new JsonSerializerOptions {
                    Converters = { new AzureDictionaryServiceTagConverter(), new ListIPv4Converter() }
                }, cancellationToken: default);
            }
            else { return null; }
        }
    }
}