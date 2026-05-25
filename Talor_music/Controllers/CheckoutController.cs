using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json; // חשוב לשליחת JSON
using System.Security.Claims;
using Talor_music.Data;
using Talor_music.Models;

namespace Talor_music.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly Talor_musicContext _context;
        private readonly IHttpClientFactory _httpClientFactory;

        // הזרקת ה-HttpClientFactory במקום ה-PaymentService
        public CheckoutController(Talor_musicContext context, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _httpClientFactory = httpClientFactory;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessPayment(CheckoutViewModel model)
        {
            if (!ModelState.IsValid) return View("Index", model);

            // יצירת לקוח HTTP
            var client = _httpClientFactory.CreateClient();

            // הכנת הנתונים למשלוח ל-API
            var paymentData = new
            {
                CardNumber = model.CardNumber,
                ExpiryDate = model.ExpirationDate,
                CVV = model.CVV
            };

            // שליחת בקשה ל-API שיצרת
            // שימי לב: הכתובת צריכה להיות בדיוק איפה שה-API שלך רץ (למשל localhost:7101)
            var response = await client.PostAsJsonAsync("https://localhost:7101/api/PaymentApi/validate", paymentData);

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("CardNumber", "התשלום נדחה: פרטי האשראי שגויים או שהתוקף פג.");
                return View("Index", model);
            }

            // --- מכאן הקוד נשאר בדיוק כפי שהיה (שמירת הזמנה במסד הנתונים) ---
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var playlist = _context.PlayListSong
                .Include(p => p.Songs)
                .FirstOrDefault(p => p.PlaylistSongID == model.PlaylistID);

            var order = new Order
            {
                CustomerID = userId,
                OrderDate = DateTime.Now,
                TotalAmount = model.TotalPrice,
                CardLastFourDigits = model.CardNumber.Length >= 4 ? model.CardNumber.Substring(model.CardNumber.Length - 4) : "****"
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            // ... (שאר הקוד של הוספת ה-OrderItems נשאר אותו דבר) ...

            return RedirectToAction("Success");
        }

        // ... שאר הפעולות ...
    }
}
