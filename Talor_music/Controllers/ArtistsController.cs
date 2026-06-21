using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; //  עבור ההרשאות
using Talor_music.Data;
using Talor_music.Models;

namespace Talor_music.Controllers
{
    public class ArtistsController : Controller
    {
        private readonly Talor_musicContext _context;

        public ArtistsController(Talor_musicContext context)
        {
            _context = context;
        }

        // כולם יכולים לראות את רשימת האמנים 
        public async Task<IActionResult> Index()
        {
            return View(await _context.Artist.ToListAsync());
        }

        //  כולם יכולים לראות פרטי אמן 
        
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // זה החלק שמושך את השירים מהטבלה השנייה
            var artist = await _context.Artist
                .Include(a => a.Songs)
                .FirstOrDefaultAsync(m => m.ArtistID == id);

            if (artist == null)
            {
                return NotFound();
            }

            return View(artist);
        }

        //  רק מנהל יכול ליצור אמן חדש 
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ArtistID,Name")] Artist artist)
        {
            if (ModelState.IsValid)
            {
                _context.Add(artist);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(artist);
        }

        //  רק מנהל יכול לערוך אמן 
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var artist = await _context.Artist.FindAsync(id);
            if (artist == null) return NotFound();
            return View(artist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ArtistID,Name")] Artist artist)
        {
            if (id != artist.ArtistID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(artist);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArtistExists(artist.ArtistID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(artist);
        }

        // רק מנהל יכול למחוק אמן 
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var artist = await _context.Artist
                .FirstOrDefaultAsync(m => m.ArtistID == id);
            if (artist == null) return NotFound();

            return View(artist);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var artist = await _context.Artist.FindAsync(id);
            if (artist != null)
            {
                _context.Artist.Remove(artist);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ArtistExists(int id)
        {
            return _context.Artist.Any(e => e.ArtistID == id);
        }
    }
}
