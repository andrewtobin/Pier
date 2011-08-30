using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Pier
{
    public class PierContext : DbContext
    {
        public DbSet<Shortened> ShortUrls { get; set; }
    }
}