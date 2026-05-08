using Microsoft.EntityFrameworkCore;
using TinyUrlApi.Models;

namespace TinyUrlApi.Data
{
    public class AppDBContext: DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options): base(options)
        {

        }
        public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();
    }
}
