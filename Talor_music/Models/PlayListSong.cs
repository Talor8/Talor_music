namespace Talor_music.Models
{
    public class PlayListSong
    {
        public int PlaylistSongID { get; set; }

        // מפתח זר ללקוח (נניח שהוא מנוהל על ידי מחרוזת ה-ID של Identity)
        public string CustomerID { get; set; }
        // נניח שיש לנו כאן Navigation Property ל-ApplicationUser

        public int SongID { get; set; }
        public Song Song { get; set; } // Navigation Property

        public DateTime DateAdded { get; set; } = DateTime.Now;

    }
}
