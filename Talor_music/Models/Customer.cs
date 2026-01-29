namespace Talor_music.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        // כל לקוח יכול להיות לו פלייליסט
        public List<PlayListSong>? Playlists { get; set; }

    }
}
