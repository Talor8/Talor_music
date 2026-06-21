using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Talor_music.Models
{
    public class Song
    {
        public int SongID { get; set; }

        [Required]
        [StringLength(200)]
        [Display(Name = "שם השיר")]
        public string Title { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "ז'אנר")]
        public string Genre { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }
        public int ArtistID { get; set; }
        public Artist? Artist { get; set; } = null;
        public ICollection<PlayListSong>? PlaylistSong { get; set; } = new List<PlayListSong>();
        
        public string? ImagePath { get; set; }
        public string? AudioFilePath { get; set; } 
        public virtual ICollection<Review>? Reviews { get; set; } = new List<Review>();

    }



}

