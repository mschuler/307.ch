using System.Security.Cryptography;
using System.Text;

namespace UrlShorteningService.Services.Impl
{
    internal sealed class AdminCodeGenerator: IAdminCodeGenerator
    {
        private static readonly RNGCryptoServiceProvider _cryptoServiceProvider = new RNGCryptoServiceProvider();

        public string Generate()
        {
            var result = new StringBuilder();

            do
            {
                var data = new byte[4];
                _cryptoServiceProvider.GetBytes(data);

                foreach (var b in data)
                {
                    Base58Converter.ToString(b, result);
                }
            }
            while (result.Length < 16);

            return result.ToString(0, 16);
        }
    }
}