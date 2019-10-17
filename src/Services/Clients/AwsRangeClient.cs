using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using PrefixSearch.Data.Amazon;
using PrefixSearch.Data.Converters;

namespace PrefixSearch.Services.Clients
{
    public class AwsRangeClient : IAwsRangeClient
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly IOptions<CloudProviderClientSettings> settings;

        public AwsRangeClient(IHttpClientFactory clientFactory, IOptions<CloudProviderClientSettings> settings)
        {
            this.clientFactory = clientFactory;
            this.settings = settings;
        }

        public async Task<AwsIPRange> GetAwsIPRange()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, settings.Value.AwsIpRangeDownloadUrl) { Version = new Version(2, 0) };
            var client = clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode) { return null; }

            using var contentStream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<AwsIPRange>(contentStream, new JsonSerializerOptions {
                Converters = { new AwsTimeConverter(), new IPv4Converter() }
            }, cancellationToken: default);
        }
    }
}