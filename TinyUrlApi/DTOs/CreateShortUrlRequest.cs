namespace TinyUrlApi.DTOs
{
    public class CreateShortUrlRequest
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public bool IsPrivate { get; set; }
    }
}
