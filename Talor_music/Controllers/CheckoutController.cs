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
            var playlist = _context.PlayListSong
                .Include(p => p.Songs)
                .FirstOrDefault(p => p.PlaylistSongID == playlistId);

            if (playlist == null) return NotFound();

            // התיקון שלנו: חיפוש חכם של המשתמש וההזמנות שלו
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userEmail = User.Identity?.Name;
            var customer = _context.Customer.FirstOrDefault(c => c.Email == userEmail);
            var customerIdStr = customer?.Id.ToString();

            var purchasedSongIds = _context.OrderItems
                .Where(oi => oi.Order.CustomerID == userId ||
                             oi.Order.CustomerID == userEmail ||
                             oi.Order.CustomerID == customerIdStr)
                .Select(oi => oi.SongID)
                .Distinct()
                .ToList();

            // חישוב מחיר רק לשירים שעדיין לא נקנו
            decimal finalPrice = playlist.Songs
                .Where(s => !purchasedSongIds.Contains(s.SongID))
                .Sum(s => s.Price);

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
                .Where(oi => oi.Order.CustomerID == userId ||
                             oi.Order.CustomerID == userEmail ||
                             oi.Order.CustomerID == customerIdStr)
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
            return View();
        }
    }
}


