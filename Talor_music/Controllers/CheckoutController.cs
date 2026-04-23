using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Talor_music.Data;
using Talor_music.Models;
using Talor_music.Services; // וודאי שה-Namespace של ה-Service נכון

namespace Talor_music.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly Talor_musicContext _context;
        private readonly IPaymentService _paymentService; // השירות החדש שלנו

        // הוספנו את ה-paymentService לבנאי
        public CheckoutController(Talor_musicContext context, IPaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }

        // פעולה שמציגה את דף התשלום ומחשבת את המחיר
        [HttpGet]
        public IActionResult Index(int playlistId)
        {
            var playlist = _context.PlayListSong
                .Include(p => p.Songs)
                .FirstOrDefault(p => p.PlaylistSongID == playlistId);

            if (playlist == null) return NotFound();

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var purchasedSongIds = _context.OrderItems
                .Where(oi => oi.Order.CustomerID == userId)
                .Select(oi => oi.SongID)
                .ToList();

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

        // פעולה שמקבלת את הטופס ומבצעת את הרכישה
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ProcessPayment(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // --- הבדיקה מול ה-API ---
            // כאן אנחנו קוראים לשירות שבודק אם הכרטיס נמצא ברשימה המאושרת
            bool isApproved = _paymentService.ValidatePayment(model.CardNumber, model.CVV, model.ExpirationDate);

            if (!isApproved)
            {
                // אם הכרטיס לא אושר, נוסיף שגיאה שתוצג למשתמש
                ModelState.AddModelError("CardNumber", "התשלום נדחה על ידי חברת האשראי. אנא השתמש בכרטיס מאושר לבדיקה.");
                return View("Index", model);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var playlist = _context.PlayListSong
                .Include(p => p.Songs)
                .FirstOrDefault(p => p.PlaylistSongID == model.PlaylistID);

            // יצירת הזמנה חדשה
            var order = new Order
            {
                CustomerID = userId,
                OrderDate = DateTime.Now,
                TotalAmount = model.TotalPrice,
                CardLastFourDigits = model.CardNumber.Length >= 4 ? model.CardNumber.Substring(model.CardNumber.Length - 4) : "****"
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

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
                _context.SaveChanges();
            }

            return RedirectToAction("Success");
        }

        public IActionResult Success()
        {
            return View();
        }
    }
}
