using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;
using Raven.Client;
using Raven.Client.Embedded;

namespace UrlShorteningService.Services.Impl
{
    public class Repository : IRepository
    {
        //private static readonly ConcurrentDictionary<string, Entry> _dictionary = new ConcurrentDictionary<string, Entry>(StringComparer.Ordinal);
        private static readonly IDocumentStore _store;

        static Repository()
        {
            //_dictionary.TryAdd("20", new LinkEntry { Id = "20", Link = "http://www.20min.ch" });
            //_dictionary.TryAdd("google", new LinkEntry { Id = "google", Link = "http://www.google.com" });

            _store = new EmbeddableDocumentStore
            {
                DataDirectory = "Data",
                DefaultDatabase = "Entries",
            };
            
            _store.Initialize();
        }

        public void Add(Entry entry)
        {
            //_dictionary.AddOrUpdate(entry.Id, entry, (k, v) => entry);

            using (var session = _store.OpenSession())
            {
                session.Store(entry);
                session.SaveChanges();
            }
        }

        public Entry Get(string id)
        {
            //Entry entry;
            //if (_dictionary.TryGetValue(id, out entry))
            //{
            //    return entry;
            //}
            //return null;

            using (var session = _store.OpenSession())
            {
                return session.Load<Entry>(id);
            }
        }
    }
    

        /// <summary>
        /// Generates new unique ids based on Twitter's Snowflake algorithm.
        /// This class is not thread-safe.
        /// </summary>
        public class Generator
        {
            // should be between 40 (34 years) and 42 (139 years)
            private readonly int _numberOfTimeBits;

            // should be 10 at least (4096 unique ids per millisecond per generator)
            private readonly int _numberOfSequenceBits;

            private readonly byte[] _buffer = new byte[8];
            private readonly byte[] _timeBytes = new byte[8];
            private readonly byte[] _idBytes = new byte[2];
            private readonly byte[] _sequenceBytes = new byte[2];
            private readonly int _maxSequence;
            private readonly DateTime _start;

            private short _sequence;
            private long _previousTime;

            /// <summary>
            /// Instantiate the generator. Each Generator should have its own ID, so you can
            /// use multiple Generator instances in a cluster. All generated IDs are unique
            /// provided the start date newer changes. I recommend to choose January 1, 2013.
            /// </summary>
            public Generator(short generatorId, DateTime start, int numberOfTimeBits = 60, int numberOfGeneratorIdBits = 0)
            {
                _numberOfTimeBits = numberOfTimeBits;
                _numberOfSequenceBits = 64 - _numberOfTimeBits - numberOfGeneratorIdBits;
                _maxSequence = (int)Math.Pow(2, _numberOfSequenceBits) - 1;

                if (generatorId < 0 || generatorId >= Math.Pow(2, numberOfGeneratorIdBits))
                {
                    var msg = string.Format(
                        CultureInfo.InvariantCulture,
                        "generator id must be between 0 (inclusive) and {0} (exclusive).",
                        Math.Pow(2, numberOfGeneratorIdBits));
                    throw new ArgumentException(msg, "generatorId");
                }
                if (start > DateTime.Today)
                {
                    throw new ArgumentException("start date must not be in the future.", "start");
                }

                CalculateIdBytes(generatorId);
                _start = start;
            }

            /// <summary>
            /// Can generate up to 4096 different IDs per millisecond.
            /// </summary>
            public string Next(bool removeTrailingEqualSign = false)
            {
                SpinToNextSequence();
                WriteValuesToByteArray(_buffer, _previousTime, _sequence);

                var id = Convert.ToBase64String(_buffer, Base64FormattingOptions.None);
                if (removeTrailingEqualSign)
                {
                    return id.Substring(0, id.Length - 1);
                }
                return id;
            }

            public ulong NextLong()
            {
                SpinToNextSequence();
                WriteValuesToByteArray(_buffer, _previousTime, _sequence);

                Array.Reverse(_buffer);
                return BitConverter.ToUInt64(_buffer, 0);
            }

            internal unsafe void WriteValuesToByteArray(byte[] target, long time, short sequence)
            {
                fixed (byte* arrayPointer = target)
                {
                    *(long*)arrayPointer = 0;
                }

                fixed (byte* arrayPointer = _timeBytes)
                {
                    *(long*)arrayPointer = time << (64 - _numberOfTimeBits);
                }

                fixed (byte* arrayPointer = _sequenceBytes)
                {
                    *(short*)arrayPointer = sequence;
                }

                WriteValuesToByteArray(target, _timeBytes, _idBytes, _sequenceBytes);
            }

            private unsafe void CalculateIdBytes(short id)
            {
                fixed (byte* arrayPointer = _idBytes)
                {
                    *(short*)arrayPointer = (short)(id << (8 - ((64 - _numberOfSequenceBits) % 8)));
                }
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private void WriteValuesToByteArray(byte[] target, byte[] time, byte[] id, byte[] sequence)
            {
                ////                                                 1234567890123456789012
                //// time: 1111111111111111111111111111111111111111110000
                //// id:                                           0011111111110000
                //// seq:                                                  0000111111111111
                ////
                ////       000000000000000100010111101010100001000010 0000001011 000000000000
                //// pos:  0         1         2         3         4         5         6
                //// byte: 0       1       2       3       4       5       6       7

                target[0] = (byte)(target[0] | time[7]);
                target[1] = (byte)(target[1] | time[6]);
                target[2] = (byte)(target[2] | time[5]);
                target[3] = (byte)(target[3] | time[4]);
                target[4] = (byte)(target[4] | time[3]);
                target[5] = (byte)(target[5] | time[2]);
                target[6] = (byte)(target[6] | time[1]);
                target[7] = (byte)(target[7] | time[0]);

                target[5] = (byte)(target[5] | id[1]);
                target[6] = (byte)(target[6] | id[0]);

                target[6] = (byte)(target[6] | sequence[1]);
                target[7] = (byte)(target[7] | sequence[0]);
            }

            private void SpinToNextSequence()
            {
                var time = GetTime();

                while (time == _previousTime && _sequence >= _maxSequence)
                {
                    Thread.Sleep(0);
                    time = GetTime();
                }

                _sequence = time == _previousTime ? (short)(_sequence + 1) : (short)0;
                _previousTime = time;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private long GetTime()
            {
                return (long)(DateTime.UtcNow - _start).TotalMilliseconds;
            }
        }
}
