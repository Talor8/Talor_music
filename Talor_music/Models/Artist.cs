using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Talor_music.Models
{
    public class Artist
    {
        public int ArtistID { get; set; }

        [Required(ErrorMessage = "נא להזין שם אמן")]
        [StringLength(100)]
        [Display(Name = "שם האמן")]
        public string Name { get; set; }

        // Added '?' to make it optional during creation
       
    }
}