using System;
using System.Collections.Concurrent;

namespace UrlShorteningService.Services.Impl
{
    internal sealed class Repository : IRepository
    {
        private static readonly ConcurrentDictionary<string, Entry> _dictionary = new ConcurrentDictionary<string, Entry>(StringComparer.Ordinal);

        public void Add(Entry entry)
        {
            _dictionary.AddOrUpdate(entry.Id, entry, (k, v) => entry);
        }

        public Entry Get(string id)
        {
            Entry entry;
            if (_dictionary.TryGetValue(id, out entry))
            {
                return entry;
            }
            return null;
        }
    }
}