using Microsoft.EntityFrameworkCore;
using Talor_music.Models;

namespace Talor_music.Data
{
    public class Talor_musicContext : DbContext
    {
        public Talor_musicContext(DbContextOptions<Talor_musicContext> options)
            : base(options)
        {
        }

        public DbSet<Artist> Artist { get; set; }
        public DbSet<Album> Album { get; set; }
    }
}
