using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace UrlShorteningService.Services.Impl
{
    internal sealed class IdGenerator : IIdGenerator
    {
        private static readonly Random _random = new Random();
        private static readonly ConcurrentStack<ulong> _availableIds = new ConcurrentStack<ulong>();
        private static readonly object _lock = new object();
        private static readonly ulong _low = 1024;
        private static ulong _high;

        public ulong NextId()
        {
            ulong id;
            if (_availableIds.TryPop(out id))
            {
                return id;
            }

            lock (_lock)
            {
                while (!_availableIds.TryPop(out id))
                {
                    FillIdBag();
                }

                return id;
            }
        }

        private static void FillIdBag()
        {
            if (_availableIds.Count > 100)
            {
                return;
            }

            var lowerBound = _high;
            var upperBound = _high + _low;
            var ids = new List<ulong>((int)_low);

            for (var id = lowerBound; id < upperBound; id++)
            {
                ids.Add(id);
            }

            _availableIds.PushRange(ids.OrderBy(i => _random.Next()).ToArray());

            _high = upperBound;
        }
    }
}