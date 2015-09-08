namespace UrlShorteningService.Services
{
    public interface IRepository
    {
        void Add(Entry entry);

        Entry Get(string id);
    }

    public class LinkEntry : Entry
    {
        public string Link { get; set; }
    }

    public class FileEntry : Entry
    {
    }

    public abstract class Entry
    {
        public string Id { get; set; }
    }
}
