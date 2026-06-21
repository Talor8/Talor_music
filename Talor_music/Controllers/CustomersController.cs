using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Talor_music.Data;
using Talor_music.Models;

namespace Talor_music.Controllers
{
    public class CustomersController : Controller
    {
        private readonly Talor_musicContext _context;

        public CustomersController(Talor_musicContext context)
        {
            _context = context;
        }

        // GET: Customers
        public IActionResult Index()
        {
            // 1. שליפת כל הלקוחות עבור הטבלה
            var customers = _context.Customer.ToList();

            // 2. שליפת כל הפריטים שנמכרו מהמסד לזיכרון, כולל פרטי ההזמנה
            var allOrderItems = _context.OrderItems
                                        .Include(oi => oi.Order)
                                        .ToList();

            // 3. חישוב הכנסות ללא כפילויות:
            // קיבוץ לפי לקוח ושיר, לקיחת הרשומה הראשונה מכל קבוצה, וסכימת המחירים
            ViewBag.TotalRevenue = allOrderItems
                .GroupBy(oi => new { oi.Order.CustomerID, oi.SongID })
                .Select(g => g.First())
                .Sum(oi => oi.PriceAtPurchase);

            // 4. החזרת הלקוחות לתצוגה
            return View(customers);
        }


        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customer
                .Include(c => c.Playlists) // השורה הזו טוענת את הפלייליסטים של הלקוח
                .FirstOrDefaultAsync(m => m.Id == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        
        public IActionResult Create()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                _context.Add(customer);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(customer);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
       

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customer
                .FirstOrDefaultAsync(m => m.Id == id);
            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customer.FindAsync(id);
            if (customer != null)
            {
                _context.Customer.Remove(customer);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customer.Any(e => e.Id == id);
        }
    }
}
