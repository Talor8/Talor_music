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

        // זהו שדה הז'אנר הרגיל כפי שביקשת
        [Required]
        [StringLength(50)]
        [Display(Name = "ז'אנר")]
        public string Genre { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal Price { get; set; }
        public int ArtistID { get; set; }
        public Artist? Artist { get; set; }


    }
}
