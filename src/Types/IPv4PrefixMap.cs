using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections;

namespace PrefixSearch.Types
{
    public class IPv4PrefixMap : IEnumerable<IPv4Prefix>
    {
        private readonly byte[] map;
        public readonly int Count;

        public IPv4PrefixMap(List<IPv4Prefix> prefixes)
        {
            Count = prefixes.Count;
            map = new byte[Count * 5];

            int index = 0;
            prefixes.ForEach(x => WriteTo(x.IP, x.Mask, ref index));
        }

        private IPv4PrefixMap(byte[] mapData)
        {
            if (mapData.Length % 5 != 0) { throw new ArgumentException("IPv4PrefixMap length should be a multiple of 5", "mapData"); }

            Count = mapData.Length / 5;
            map = mapData;
        }

        public IPv4Prefix this[int index] => ReadFrom(index * 5);

        public bool TryGetValue(uint ip, out IPv4Prefix prefix)
        {
            int lower = 0, upper = Count - 1, mid;
            uint maskedIP;

            while (lower <= upper)
            {
                mid = (lower + upper) / 2;
                IPv4Prefix x = this[mid];
                maskedIP = ip & x.NetMask;

                if (maskedIP == x.IP) { prefix = x; return true; }
                else if (maskedIP < x.IP) { upper = mid - 1; }
                else { lower = mid + 1; }
            }

            prefix = IPv4Prefix.None;
            return false;
        }

        public async Task WriteFile(string path) => await File.WriteAllBytesAsync(path, map);
        public static async Task<IPv4PrefixMap> ReadFile(string path) => new IPv4PrefixMap(await File.ReadAllBytesAsync(path));

        internal IEnumerable<IPv4PrefixMapSearchResult> FindNetworks(int thread, IEnumerable<uint> searchAddresses)
        {
            int missCounter = 0;
            foreach (uint address in searchAddresses)
            {
                if (TryGetValue(address, out IPv4Prefix x))
                {
                    yield return new IPv4PrefixMapSearchResult(thread, address, x, missCounter);
                    missCounter = 0;
                }
                else { ++missCounter; }
            }
        }

        public IEnumerator<IPv4Prefix> GetEnumerator()
        {
            for (int pos = 0; pos < map.Length; pos += 5) { yield return ReadFrom(pos); }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private void WriteTo(uint ip, byte maskBits, ref int pos)
        {
            map[pos++] = (byte)((ip >> 24) & 0xff);
            map[pos++] = (byte)((ip >> 16) & 0xff);
            map[pos++] = (byte)((ip >> 8) & 0xff);
            map[pos++] = (byte)(ip & 0xff);
            map[pos++] = maskBits;
        }

        private IPv4Prefix ReadFrom(int pos) => new IPv4Prefix((uint)((map[pos++] << 24) | (map[pos++] << 16) | (map[pos++] << 8) | map[pos++]), map[pos]);
    }
}