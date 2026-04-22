using System.ComponentModel.DataAnnotations;

namespace Talor_music.Models;
    // זהו מודל עזר שנועד רק כדי להעביר את הנתונים בין הקונטרולר לתצוגה של דף התשלום (כדי שהטופס ידע איזה פלייליסט קונים ומה המחיר הסופי).
    public class CheckoutViewModel
    {
        public int PlaylistID { get; set; }
        public string PlaylistName { get; set; }
        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "חובה להזין מספר כרטיס")]
        public string CardNumber { get; set; }

        [Required(ErrorMessage = "חובה להזין תוקף")]
        public string ExpiryDate { get; set; }

        [Required(ErrorMessage = "חובה להזין CVV")]
        public string CVV { get; set; }
    }
