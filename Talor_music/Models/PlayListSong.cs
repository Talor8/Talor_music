namespace Talor_music.Models
{
    public class PlayListSong
    {
        public int PlaylistSongID { get; set; }

        // מפתח זר ללקוח (נניח שהוא מנוהל על ידי מחרוזת ה-ID של Identity)
        public string Title { get; set; }
        public int CustomerID { get; set; }
        // נניח שיש לנו כאן Navigation Property ל-ApplicationUser
        public virtual Customer Customer { get; set; } // ודאי שיש את השורה הזו
        public ICollection<Song>? Songs { get; set; } = new List<Song>();

        public DateTime DateAdded { get; set; } = DateTime.Now;

    }
}
