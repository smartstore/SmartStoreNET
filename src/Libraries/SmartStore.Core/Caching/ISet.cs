using System.Collections.Generic;

namespace SmartStore.Core.Caching
{
    public interface ISet : IEnumerable<string>
    {
        bool Add(string item);
        void AddRange(IEnumerable<string> items);
        void Clear();
        bool Contains(string item);
        bool Remove(string item);
        bool Move(string destinationKey, string item);

        long UnionWith(params string[] keys);
        long IntersectWith(params string[] keys);
        long ExceptWith(params string[] keys);

        int Count { get; }
    }
}
