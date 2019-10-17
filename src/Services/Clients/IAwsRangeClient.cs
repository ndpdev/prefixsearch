using System.Threading.Tasks;
using PrefixSearch.Data.Amazon;

namespace PrefixSearch.Services.Clients
{
    public interface IAwsRangeClient
    {
        Task<AwsIPRange> GetAwsIPRange();
    }
}