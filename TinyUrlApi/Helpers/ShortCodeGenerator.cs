namespace TinyUrlApi.Helpers
{
    public static class ShortCodeGenerator
    {
        private const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        public static string Generate()
        {
            var random = new Random();
            return new string(
                Enumerable.Range(0, 6)
                .Select(x => chars[random.Next(chars.Length)])
                .ToArray());
        }
    }
}
