using System;
using System.Collections.Generic;
using System.Linq;

namespace PrefixSearch.Types
{
    public readonly struct IPv4Prefix
    {
        public static readonly IPv4Prefix None = new IPv4Prefix(0, 0);

        public uint IP { get; }
        public byte Mask { get; }

        public IPv4Prefix(uint ip, byte mask) { IP = ip & IPv4.NetMask(mask & 0x7f); Mask = mask; }
        public void Deconstruct(out uint ip, out byte mask) => (ip, mask) = (IP, Mask);

        public int MaskBits => Mask & 0x7f;
        public bool HasFlag => (Mask & 0x80) == 0x80;
        public uint NetMask => IPv4.NetMask(MaskBits);
        public uint Wildcard => IPv4.Wildcard(MaskBits);
        public uint Broadcast => IPv4.Broadcast(IP, MaskBits);

        public uint MaximumSubnets => (uint)1 << (32 - MaskBits);
        public uint MaximumAddresses => MaskBits switch { 32 => 1, 31 => 2, _ => MaximumSubnets - 2 };

        public static IPv4Prefix Parse(string value) => IPv4.ParsePrefix(value);
        public override string ToString() => $"{IPv4.ConvertToString(IP)}/{MaskBits}";

        public bool IsSubsetOf(in IPv4Prefix prefix) => (prefix.MaskBits < MaskBits) && (prefix.IP == (IP & prefix.NetMask));

        public static IEnumerable<IPv4Prefix> Consolidate(IEnumerable<IPv4Prefix> prefixes)
        {
            IPv4Prefix previousPrefix = None;
            return prefixes.OrderBy(x => (x.IP, x.MaskBits)).Where(x => {
                bool result = !x.IsSubsetOf(previousPrefix);
                if (result) { previousPrefix = x; };
                return result;
            });
        }

        public static Func<IPv4Prefix, IPv4Prefix> SetMaskFlag = x => new IPv4Prefix(x.IP, (byte)(x.Mask ^ 0x80));
    }
}