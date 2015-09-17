using System.Text;

namespace UrlShorteningService.Services.Impl
{
    public class Base58Converter: IBase58Converter
    {
        private const string Alphabet = "123456789abcdefghijkmnopqrstuvwxyzABCDEFGHJKLMNPQRSTUVWXYZ";
        private static readonly ulong _base = (ulong)Alphabet.Length;

        public string ToString(ulong id)
        {
            var result = new StringBuilder(12);
            ToString(id, result);
            return result.ToString();
        }

        public ulong ToUInt64(string id)
        {
            ulong result = 0;
            ulong factor = 1;

            for (var i = id.Length - 1; i >= 0; i--)
            {
                result += factor * (ulong)Alphabet.IndexOf(id[i]);
                factor = factor * _base;
            }

            return result;
        }

        internal static void ToString(ulong id, StringBuilder sb)
        {
            do
            {
                sb.Insert(0, Alphabet[(int)(id % _base)]);
                id = id / _base;
            } while (id > 0);
        }
    }
}