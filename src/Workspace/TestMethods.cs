using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PrefixSearch.Services.Clients;
using PrefixSearch.Types;

namespace PrefixSearch.Workspace
{
    public class TestMethods
    {
        private readonly IServiceProvider services;
        private readonly IOptions<WorkspaceSettings> settings;

        public TestMethods(IServiceProvider services)
        {
            this.services = services;
            settings = services.GetRequiredService<IOptions<WorkspaceSettings>>();
        }

        public void ParsePrefix()
        {
            var testPrefix = settings.Value.TestPrefix;
            var testCount = settings.Value.TestCount;

            IPv4Prefix x = IPv4Prefix.Parse(testPrefix);
            Console.WriteLine($"Test: {IPv4.ConvertToBitString(x.IP)} {x.ToString()} [ParsePrefix prefix:{testPrefix}]");

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < testCount; i++) { _ = IPv4.ParsePrefix(testPrefix); }
            Console.WriteLine($" - {sw.ElapsedMilliseconds} ms");
        }

        public void ParseIPAddress()
        {
            var testAddress = settings.Value.TestAddress;
            var testCount = settings.Value.TestCount;

            var ip = IPv4.ParseIP(testAddress);
            Console.WriteLine($"Test: {IPv4.ConvertToBitString(ip)} {IPv4.ConvertToString(ip)} [ParseIPAddress address:{testAddress}]");

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < testCount; i++) { _ = IPv4.ParseIP(testAddress); }
            Console.WriteLine($" - {sw.ElapsedMilliseconds} ms");
        }

        public void SearchIPv4PrefixMap(IPv4PrefixMap map)
        {
            int threadCount = Math.Max(Environment.ProcessorCount - 1, 2);
            int searchesPerThread = settings.Value.SearchCount / threadCount;

            //TODO: Convert to PLINQ expression?
            var sw = Stopwatch.StartNew();
            var batches = Enumerable.Range(1, threadCount)
                .Select( id => Task.Run(() => map.FindNetworks(id, GenerateRandomIPAddresses(searchesPerThread)).ToList()) )
                .ToArray();
            Task.WaitAll(batches);
            sw.Stop();

            long searchTime = sw.ElapsedMilliseconds;
            var totalResultCount = batches.Select(x => x.Result.Count).Sum();
            var results = batches.SelectMany( x => x.Result.Take(5) );
            
            Console.WriteLine("Sample Search Results");
            foreach (var x in results)
            {
                string block = $"{x.Prefix} ({(x.Prefix.HasFlag ? "Azure" : "Amazon")})";
                string message = $" * [{x.Thread}] ip: {IPv4.ConvertToString(x.Address),-17} net: {block,-27} miss: {x.Counter,-5}";
                Console.WriteLine(message);
            }

            Console.WriteLine();
            Console.WriteLine($"Found {totalResultCount} matching addresses in {map.Count} distinct prefixes ({searchTime} ms)");
        }

        public async Task<IPv4PrefixMap> RetrieveIPv4PrefixMap()
        {
            IPv4PrefixMap value;
            var networkMapPath = settings.Value.NetworkMapPath;
            
            var sw = Stopwatch.StartNew();
            if (File.Exists(networkMapPath) && (DateTime.Now - File.GetLastWriteTime(networkMapPath)).TotalDays < 7)
            {
                value = await IPv4PrefixMap.ReadFile(networkMapPath);
                Console.WriteLine($" - Prefix map deserialization completed ({sw.ElapsedMilliseconds} ms)");
            }
            else
            {
                //TODO: Convert to PLINQ expression?
                var awsRangeTask = Task.Run(() => services.GetRequiredService<IAwsRangeClient>().GetAwsIPRange());
                var azureRangeTask = Task.Run(() => services.GetRequiredService<IAzureRangeClient>().GetAzureIPRange());
                Task.WaitAll(new Task[] { awsRangeTask, azureRangeTask });
                Console.WriteLine($" - Downloading IP Ranges completed ({sw.ElapsedMilliseconds} ms)");
                sw.Restart();

                var awsPrefixes = awsRangeTask.Result.IPv4Prefixes.Select(x => x.Prefix);
                var azurePrefixes = azureRangeTask.Result.Services.SelectMany(x => x.Value.AddressPrefixes.Select(IPv4Prefix.SetMaskFlag));
                var prefixes = IPv4Prefix.Consolidate(awsPrefixes.Union(azurePrefixes)).ToList();
                value = new IPv4PrefixMap(prefixes);
                Console.WriteLine($" - Constructing prefix map completed ({sw.ElapsedMilliseconds} ms)");
                sw.Restart();

                await value.WriteFile(networkMapPath);
                Console.WriteLine($" - Prefix map serialization completed ({sw.ElapsedMilliseconds} ms)");
            }

            return value;
        }

        private static IEnumerable<uint> GenerateRandomIPAddresses(int count)
        {
            using var rng = RandomNumberGenerator.Create();
            var buffer = new byte[4096];
            ReadOnlySpan<byte> span;

            int pos = buffer.Length;
            int maxPos = buffer.Length - 4;
            for (int i = 0; i < count; i++)
            {
                do
                {
                    if (pos >= maxPos) { rng.GetBytes(buffer); pos = 0; }
                    span = buffer.AsSpan(pos, 4); pos += 4;
                }
                while (IsInvalidAddress(span));
                yield return BitConverter.ToUInt32(span);
            }
        }

        private static bool IsInvalidAddress(ReadOnlySpan<byte> x)
        {
            // ref: https://en.wikipedia.org/wiki/Reserved_IP_addresses
            return (x[0] == 0) || 
                   (x[0] == 10) || 
                   (x[0] == 100 && x[1] >= 64 && x[1] <= 127) ||
                   (x[0] == 127) ||
                   (x[0] == 169 && x[1] == 254) ||
                   (x[0] == 172 && x[1] >= 16 && x[1] <= 31) || 
                   (x[0] == 192 && (
                       (x[1] == 0 && (x[2] == 0 || x[2] == 2)) ||
                       (x[1] == 88 && x[2] == 99) ||
                       (x[1] == 168) )) ||
                   (x[0] == 198 && x[1] == 51 && x[2] == 100) ||
                   (x[0] == 203 && x[1] == 0 && x[2] == 113) ||
                   (x[0] >= 224);
        }
    }
}