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

        // דף הבית של הפלייליסטים
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity?.Name;

            // הוספנו Include(p => p.Songs) כדי שהמחיר יתעדכן בטבלה
            var query = _context.PlayListSong
                .Include(p => p.Customer)
                .Include(p => p.Songs)
                .AsQueryable();

            if (!User.IsInRole("Admin"))
            {
                query = query.Where(p => p.Customer != null && p.Customer.Email == userEmail);
            }

            return View(await query.ToListAsync());
        }

        // זה הכפתור מהחנות (הירוק) - מחזיר רשימה של פלייליסטים לבחירה
        // דף בחירת פלייליסט כשלוחצים על Add בחנות (הכפתור הירוק)
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
            // כאן התיקון: אנחנו שולחים רשימה (List)
            return View(await query.ToListAsync());
        }

        // הפעולה שמבצעת את ההוספה בפועל (משותפת לחנות ולדף ה-Details)
        [HttpPost]
        public async Task<IActionResult> AddSongToListAction(int playListId, int songId)
        {
            var playlist = await _context.PlayListSong.Include(p => p.Songs)
                .FirstOrDefaultAsync(p => p.PlaylistSongID == playListId);
            var song = await _context.Song.FindAsync(songId);

            if (playlist != null && song != null)
            {
                if (!playlist.Songs.Any(s => s.SongID == songId))
                {
                    playlist.Songs.Add(song);
                    await _context.SaveChangesAsync();
                }
            }
            // אחרי ההוספה חוזרים לדף ה-Details של הפלייליסט אליו הוספנו
            return RedirectToAction("Details", new { id = playListId });
        }

        // דף הפרטים (הדף של רונית עם ה-No Results Found)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var playListSong = await _context.PlayListSong
                .Include(p => p.Songs).ThenInclude(s => s.Artist)
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(m => m.PlaylistSongID == id);

            if (playListSong == null) return NotFound();

            // התיקון הקדוש לחיפוש:
            ViewBag.AllSongs = await _context.Song.ToListAsync();

            return View(playListSong); // שולח אובייקט בודד
        }

        // --- פונקציות עזר סטנדרטיות ---
        public IActionResult Create()
        {
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

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var playListSong = await _context.PlayListSong.Include(p => p.Customer).FirstOrDefaultAsync(m => m.PlaylistSongID == id);
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
        // 1. דף שמציג את הטופס לעריכת השם
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var playlist = await _context.PlayListSong.FindAsync(id);
            if (playlist == null) return NotFound();

            return View(playlist);
        }

        // 2. הפעולה ששומרת את השם החדש בדאטה-בייס
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlaylistSongID,Title,DateAdded,CustomerID")] PlayListSong playListSong)
        {
            if (id != playListSong.PlaylistSongID) return NotFound();

            // התיקון הקריטי: אנחנו מסירים את הבדיקה של הלקוח כי אנחנו לא משנים אותו בעריכה
            ModelState.Remove("Customer");
            ModelState.Remove("Songs");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(playListSong);
                    await _context.SaveChangesAsync();

                    // אחרי השמירה - חוזרים לדף הפרטים לראות את השם החדש
                    return RedirectToAction("Details", new { id = playListSong.PlaylistSongID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PlayListSong.Any(e => e.PlaylistSongID == playListSong.PlaylistSongID)) return NotFound();
                    else throw;
                }
            }

            // אם הגענו לכאן סימן שיש שגיאה - נחזיר את המשתמש לדף עם השגיאות
            return View(playListSong);
        }
    }
}