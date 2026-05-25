using Microsoft.AspNetCore.Mvc;

namespace Talor_music.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentApiController : ControllerBase
    {
        // מודל קטן שמייצג את המידע שנקבל מהמשתמש
        public class PaymentRequest
        {
            public string CardNumber { get; set; }
            public string ExpiryDate { get; set; }
            public string CVV { get; set; }
        }

        [HttpPost("validate")]
        public IActionResult ValidateCard([FromBody] PaymentRequest request)
        {
            // רשימת כרטיסים מדומים "תקינים" (אפשר להוסיף כמה שרוצים)
            var validCards = new[] { "1234567812345678", "4242424242424242", "2222222222222222", "8946826839641111" };
            var validCvv = "123";

            // בדיקה פשוטה: אם המספר מופיע ברשימה שלנו וה-CVV נכון
            if (validCards.Contains(request.CardNumber) && request.CVV == validCvv)
            {
                // מחזירים תשובה חיובית 200 OK
                return Ok(new { success = true, message = "התשלום אושר בהצלחה" });
            }

            // אם הפרטים לא תואמים - מחזירים שגיאה 400 Bad Request
            return BadRequest(new { success = false, message = "פרטי האשראי שהוזנו שגויים או שפג תוקפם." });
        }
    }
}