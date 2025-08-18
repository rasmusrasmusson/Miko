using Microsoft.EntityFrameworkCore;
using MikoMe.Models; // fixed namespace

namespace MikoMe.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }

        // Your entity sets
        public DbSet<Card> Cards { get; set; }
        public DbSet<Word> Words { get; set; }
    }
}
