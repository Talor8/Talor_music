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
            // במקום לשלוח _context.Users (שהם משתמשי Identity), 
            // אנחנו שולחים את הטבלה שנקראת Customer שמוגדרת ב-DbContext שלך
            var customers = _context.Customer.ToList();

            // חישוב הכנסות בטוח
            ViewBag.TotalRevenue = _context.Orders.Any() ? _context.Orders.Sum(o => o.TotalAmount) : 0;

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
