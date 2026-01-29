using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore; // השורה החדשה שהוספנו
using Talor_music.Models;

namespace Talor_music.Data
{
    // כאן שינינו ל-IdentityDbContext
    public class Talor_musicContext : IdentityDbContext
    {
        public Talor_musicContext(DbContextOptions<Talor_musicContext> options)
            : base(options)
        {
        }

        public DbSet<Talor_music.Models.Artist> Artist { get; set; } = default!;
        public DbSet<Talor_music.Models.Song> Song { get; set; } = default!;
        public DbSet<Talor_music.Models.Customer> Customer { get; set; } = default!;
        public DbSet<Talor_music.Models.PlayListSong> PlayListSong { get; set; } = default!;
    }
}
