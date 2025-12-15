namespace Talor_music.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Artist
    {
        public int ArtistID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "שם האמן")]
        public string Name { get; set; }

        // קשר 1:N - אמן אחד יצר הרבה אלבומים
        public ICollection<Album> Albums { get; set; }
    }
}
