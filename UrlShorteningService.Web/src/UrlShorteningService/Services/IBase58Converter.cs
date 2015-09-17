namespace UrlShorteningService.Services
{
    public interface IBase58Converter
    {
        string ToString(ulong id);
        ulong ToUInt64(string id);
    }
}
