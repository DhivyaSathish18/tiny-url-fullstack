using Microsoft.EntityFrameworkCore;
using System.Xml.Serialization;
using TinyUrlApi.Models;

namespace TinyUrlApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        //public DbSet<ShortUrl> ShortUrls => Set<ShortUrl>();
        public DbSet<ShortUrl> ShortUrls { get; set; }
    }
}
