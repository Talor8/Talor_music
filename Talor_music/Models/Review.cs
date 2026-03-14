using System;
using System.ComponentModel.DataAnnotations;

namespace Talor_music.Models
{
    public class Review
    {
        public int ReviewID { get; set; }

        [Range(1, 5)]
        public int Rating { get; set; } // דירוג 1-5

        
        public string? Comment { get; set; } // תוכן התגובה

        public DateTime DatePosted { get; set; } = DateTime.Now;

        // קשר לשיר
        public int SongID { get; set; }
        public virtual Song? Song { get; set; }

        // קשר ללקוח (מי כתב את הביקורת)
        public int? CustomerID { get; set; }
        public virtual Customer? Customer { get; set; }



        
    }
}