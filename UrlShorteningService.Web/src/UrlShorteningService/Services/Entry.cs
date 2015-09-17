namespace UrlShorteningService.Services
{
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
        public string AdminCode { get; set; }
    }
}
