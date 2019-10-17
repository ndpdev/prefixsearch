namespace PrefixSearch.Types
{
    internal class IPv4PrefixMapSearchResult
    {
        public int Thread { get; }
        public uint Address { get; }
        public IPv4Prefix Prefix { get; }
        public int Counter { get; }

        public IPv4PrefixMapSearchResult(int thread, uint address, IPv4Prefix prefix, int counter)
        {
            Thread = thread;
            Address = address;
            Prefix = prefix;
            Counter = counter;
        }
    }    
}