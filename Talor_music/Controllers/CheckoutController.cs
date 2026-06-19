using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using System.Linq;
using System.Security.Claims;
using Talor_music.Data;
using Talor_music.Models;
using System.Collections.Generic;

namespace Talor_music.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly Talor_musicContext _context;
        private readonly IConfiguration _configuration;

        public CheckoutController(Talor_musicContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // פעולה שמציגה את עמוד התשלום
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

        // כאן הפונקציה ששולחת את המשתמש ל-Stripe
        [HttpPost]
        public IActionResult CreateCheckoutSession(int playlistId)
        {
            StripeConfiguration.ApiKey = _configuration["Stripe:SecretKey"];
            var domain = $"{Request.Scheme}://{Request.Host}";

            var playlist = _context.PlayListSong
                .Include(p => p.Songs)
                .FirstOrDefault(p => p.PlaylistSongID == playlistId);

            if (playlist == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.Identity?.Name;
            var customer = _context.Customer.FirstOrDefault(c => c.Email == userEmail);
            var customerIdStr = customer?.Id.ToString();

            // קיזוז שירים שנקנו בכל הזמנה של המשתמש
            var purchasedSongIds = _context.OrderItems
                .Include(oi => oi.Order) // <--- זה התיקון הקריטי ביותר
                .Where(oi => oi.Order != null && ( // הגנה מפני null
                             oi.Order.CustomerID == userId ||
                             oi.Order.CustomerID == userEmail ||
                             oi.Order.CustomerID == customerIdStr))
                .Select(oi => oi.SongID)
                .Distinct()
                .ToList();

            var songsToPayFor = playlist.Songs
                .Where(s => !purchasedSongIds.Contains(s.SongID))
                .ToList();

            decimal totalToPay = songsToPayFor.Sum(s => s.Price);

            if (totalToPay <= 0) return RedirectToAction("Success", new { playlistId = playlistId });

            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(totalToPay * 100),
                            Currency = "ils",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = playlist.Title + " (קיזוז שירים שנקנו)",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = domain + "/Checkout/Success?playlistId=" + playlistId,
                CancelUrl = domain + "/PlayListSongs/Details/" + playlistId,
            };

            var service = new SessionService();
            Session session = service.Create(options);

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        public IActionResult Success(int playlistId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // 1. יצירת הזמנה חדשה בטבלת Orders
            // בתוך הפונקציה Success
            var order = new Order
            {
                CustomerID = userId,
                OrderDate = DateTime.Now,
                // הנה התיקון: את חייבת להוסיף את השדה הזה
                CardLastFourDigits = "0000" // תכניסי כאן ערך זמני או את ה-4 ספרות שחזרו מהתשלום
            };
            _context.Orders.Add(order);
            _context.SaveChanges();

            // 2. הוספת השירים מהפלייליסט לטבלת OrderItems
            var playlist = _context.PlayListSong.Include(p => p.Songs)
                                  .FirstOrDefault(p => p.PlaylistSongID == playlistId);

            foreach (var song in playlist.Songs)
            {
                _context.OrderItems.Add(new OrderItem
                {
                    OrderID = order.OrderID,
                    SongID = song.SongID,
                    PriceAtPurchase = song.Price
                });
            }
            _context.SaveChanges();

            return View();
        }
    }
}




