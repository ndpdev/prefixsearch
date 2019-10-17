using System.Threading.Tasks;
using PrefixSearch.Data.Azure;

namespace PrefixSearch.Services.Clients
{
    public interface IAzureRangeClient
    {
        Task<AzureIPRange> GetAzureIPRange();
    }
}