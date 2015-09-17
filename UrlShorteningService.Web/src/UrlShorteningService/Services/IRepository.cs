namespace UrlShorteningService.Services
{
    public interface IRepository
    {
        void Add(Entry entry);

        Entry Get(string id);
    }
}