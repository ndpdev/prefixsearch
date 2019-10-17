namespace PrefixSearch.Services.Clients
{
    public class CloudProviderClientSettings
    {
        public string AwsIpRangeDownloadUrl { get; set; }
        public string AzureIpRangeDownloadConfirmationUrl { get; set; }
        public string AzureServiceTagRegexPattern { get; set; }
    }
}