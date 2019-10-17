using System;

namespace PrefixSearch.Types
{
    public static class IPv4
    {
        private static void AddChar(ref uint val, in char x) { val = (val * 10) + (uint)(x - '0'); }
        private static void AddChar(ref int val, in char x) { val = (val * 10) + (x - '0'); }

        public static IPv4Prefix ParsePrefix(string prefix)
        {
            var span = prefix.AsSpan();
            int len = span.Length, offset = 24, bits = 0, i = 0;
            uint octet = 0, ip = 0;
            char x;

            for (; i < len; i++)
            {
                x = span[i];
                if (x >= '0' && x <= '9') { AddChar(ref octet, x); }
                else if (x == '.' && octet < 256 && offset > 0) { ip |= octet << offset; octet = 0; offset -= 8; }
                else if (x == '/' && octet < 256) { ip |= octet; ++i; break; }
                else { i = len; }
            }
            for (; i < len; i++)
            {
                x = span[i];
                if (x >= '0' && x <= '9') { AddChar(ref bits, x); }
                else { bits = 0; break; }
            }

            if (bits == 0 || bits > 32)
            {
                return IPv4Prefix.None;
            }
            else
            {
                var byteBits = (byte)bits;
                return new IPv4Prefix(ip, byteBits);
            }
        }

        public static uint ParseIP(string ip)
        {
            var span = ip.AsSpan();
            int spanLen = span.Length, offset = 24, i = 0;
            uint octet = 0, rval = 0;
            char x;

            for (; i < spanLen; i++)
            {
                x = span[i];
                if (x >= '0' && x <= '9') { AddChar(ref octet, x); }
                else if (x == '.' && octet < 256) { rval |= octet << offset; octet = 0; offset -= 8; }
                else { return 0; }
            }
            
            if (octet > 255) { return 0; } else { return rval | octet; }
        }

        public static uint NetMask(int maskBits) => uint.MaxValue << (32 - maskBits);
        public static uint NetMask(byte maskBits) => uint.MaxValue << (32 - maskBits);
        public static uint Wildcard(int maskBits) => NetMask(maskBits) ^ uint.MaxValue;
        public static uint Wildcard(byte maskBits) => NetMask(maskBits) ^ uint.MaxValue;
        public static uint Broadcast(uint ip, int maskBits) => ip | Wildcard(maskBits);
        public static uint Broadcast(uint ip, byte maskBits) => ip | Wildcard(maskBits);

        public static string ConvertToString(uint ip) => $"{(ip >> 24) & 0xff}.{(ip >> 16) & 0xff}.{(ip >> 8) & 0xff}.{ip & 0xff}";
        public static string ConvertToBitString(uint ip) => Convert.ToString(ip, 2).PadLeft(32, '0');
    }
}