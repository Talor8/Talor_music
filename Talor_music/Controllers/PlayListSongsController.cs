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
    public class PlayListSongsController : Controller
    {
        private readonly Talor_musicContext _context;

        public PlayListSongsController(Talor_musicContext context)
        {
            _context = context;
        }

        // 1. תצוגת רשימת הפלייליסטים (מסונן לפי משתמש)
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity?.Name;
            var query = _context.PlayListSong
                .Include(p => p.Songs)
                .Include(p => p.Customer)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(p => p.Customer != null && p.Customer.Email == userEmail);
            }

            return View(await query.ToListAsync());
        }

        // 2. הצגת דף בחירת פלייליסט להוספת שיר (הדף המעוצב)
        public async Task<IActionResult> AddSongToPlayList(int? songId)
        {
            if (songId == null) return NotFound();

            var userEmail = User.Identity?.Name;
            var query = _context.PlayListSong.Include(p => p.Customer).AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(p => p.Customer != null && p.Customer.Email == userEmail);
            }

            ViewBag.SongId = songId;
            return View(await query.ToListAsync());
        }

        // 3. הפעולה שמבצעת את ההוספה הפיזית למסד הנתונים
        public async Task<IActionResult> AddSongToListAction(int playListId, int songId)
        {
            // שליפת הפלייליסט כולל רשימת השירים הקיימת בו
            var playlist = await _context.PlayListSong
                .Include(p => p.Songs)
                .FirstOrDefaultAsync(p => p.PlaylistSongID == playListId);

            // שליפת השיר שרוצים להוסיף
            var song = await _context.Song.FindAsync(songId);

            if (playlist != null && song != null)
            {
                // בדיקה שהשיר לא קיים כבר בפלייליסט (כדי למנוע כפילויות)
                if (!playlist.Songs.Any(s => s.SongID == songId))
                {
                    playlist.Songs.Add(song);
                    await _context.SaveChangesAsync();
                }

                // אחרי ההוספה - עוברים לדף הפרטים של הפלייליסט לראות שהשיר שם
                return RedirectToAction("Details", new { id = playListId });
            }

            return RedirectToAction("Index", "Songs");
        }

        // --- שאר פעולות ה-CRUD הסטנדרטיות (Create, Edit, Delete) ---

        public IActionResult Create()
        {
            if (User.IsInRole("Admin"))
                ViewData["CustomerID"] = new SelectList(_context.Customer, "Id", "Email");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlaylistSongID,Title,DateAdded,CustomerID")] PlayListSong playListSong)
        {
            if (!User.IsInRole("Admin"))
            {
                var userEmail = User.Identity?.Name;
                var customer = _context.Customer.FirstOrDefault(c => c.Email == userEmail);
                if (customer != null) playListSong.CustomerID = customer.Id;
            }

            ModelState.Remove("Customer");
            if (ModelState.IsValid)
            {
                _context.Add(playListSong);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(playListSong);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var playListSong = await _context.PlayListSong
                .Include(p => p.Songs)
                    .ThenInclude(s => s.Artist)
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(m => m.PlaylistSongID == id);

            if (playListSong == null) return NotFound();
            return View(playListSong);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var playListSong = await _context.PlayListSong.Include(p => p.Customer).FirstOrDefaultAsync(m => m.PlaylistSongID == id);
            if (playListSong == null) return NotFound();
            return View(playListSong);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var playListSong = await _context.PlayListSong.FindAsync(id);
            if (playListSong != null) _context.PlayListSong.Remove(playListSong);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PlayListSongExists(int id)
        {
            return _context.PlayListSong.Any(e => e.PlaylistSongID == id);
        }
    }
}