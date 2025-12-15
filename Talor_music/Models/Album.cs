namespace Talor_music.Models
{
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Album
    {
        public int AlbumID { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Display(Name = "שנת יציאה")]
        public int ReleaseYear { get; set; }

        // מפתח זר לאמן
        public int ArtistID { get; set; }
        public Artist Artist { get; set; } // Navigation Property

        // קשר 1:N - אלבום אחד מכיל הרבה שירים
        public ICollection<Song> Songs { get; set; }
    }
}
