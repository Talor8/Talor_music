using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using Talor_music.Data;
using Talor_music.Models;

namespace Talor_music.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly Talor_musicContext _context;

        public CheckoutController(Talor_musicContext context)
        {
            _context = context;
        }

        // פעולה שמציגה את דף התשלום ומחשבת את המחיר
        [HttpGet]
        public IActionResult Index(int playlistId)
        {
            // תיקון: PlayListSong עם L גדולה בדיוק כמו ב-Context שלך
            var playlist = _context.PlayListSong
                .Include(p => p.Songs)
                .FirstOrDefault(p => p.PlaylistSongID == playlistId);

            if (playlist == null) return NotFound();

            // מציאת המזהה של הלקוח המחובר כרגע
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // שליפת כל מספרי השירים שהלקוח הזה כבר קנה בעבר
            var purchasedSongIds = _context.OrderItems
                .Where(oi => oi.Order.CustomerID == userId)
                .Select(oi => oi.SongID)
                .ToList();

            // חישוב המחיר הסופי: סוכמים רק את השירים שהלקוח עדיין לא קנה
            decimal finalPrice = 0;
            if (playlist.Songs != null)
            {
                foreach (var song in playlist.Songs)
                {
                    if (!purchasedSongIds.Contains(song.SongID))
                    {
                        finalPrice += song.Price;
                    }
                }
            }

            var viewModel = new CheckoutViewModel
            {
                PlaylistID = playlist.PlaylistSongID,
                PlaylistName = playlist.Title,
                TotalPrice = finalPrice
            };

            return View(viewModel);
        }

        // פעולה שמקבלת את הטופס שהלקוח שלח ומבצעת את הרכישה
        [HttpPost]
        public IActionResult ProcessPayment(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model); // אם חסרים פרטים, נחזיר אותו לטופס
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // תיקון: PlayListSong עם L גדולה
            var playlist = _context.PlayListSong
                .Include(p => p.Songs)
                .FirstOrDefault(p => p.PlaylistSongID == model.PlaylistID);

            // יצירת הזמנה חדשה
            var order = new Order
            {
                CustomerID = userId,
                OrderDate = DateTime.Now,
                TotalAmount = model.TotalPrice,
                // לוקחים רק את 4 הספרות האחרונות מהכרטיס שהוקלד
                CardLastFourDigits = model.CardNumber.Length >= 4 ? model.CardNumber.Substring(model.CardNumber.Length - 4) : "****"
            };

            _context.Orders.Add(order);
            _context.SaveChanges(); // שומרים כדי לקבל OrderID

            // בדיקה שוב אילו שירים הוא כבר קנה (כדי לא לשמור אותם כ-OrderItem כפול)
            var purchasedSongIds = _context.OrderItems
                .Where(oi => oi.Order.CustomerID == userId)
                .Select(oi => oi.SongID)
                .ToList();

            // הוספת השירים החדשים להזמנה
            if (playlist?.Songs != null)
            {
                foreach (var song in playlist.Songs)
                {
                    if (!purchasedSongIds.Contains(song.SongID))
                    {
                        var orderItem = new OrderItem
                        {
                            OrderID = order.OrderID,
                            SongID = song.SongID,
                            PriceAtPurchase = song.Price
                        };
                        _context.OrderItems.Add(orderItem);
                    }
                }
                _context.SaveChanges(); // שמירה סופית של השירים שנקנו
            }

            return RedirectToAction("Success");
        }

        // דף אישור אחרי הקנייה
        public IActionResult Success()
        {
            return View();
        }
    }
}
