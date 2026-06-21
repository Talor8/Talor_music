using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; // חובה להוספה
using Talor_music.Data;
using Talor_music.Models;
using System.Security.Claims;

namespace Talor_music.Controllers
{
   
    public class PlayListSongsController : Controller
    {
        private readonly Talor_musicContext _context;

        public PlayListSongsController(Talor_musicContext context)
        {
            _context = context;
        }

        [Authorize] // דורש התחברות לצפייה בפלייליסטים
        public async Task<IActionResult> Index()
        {
            var userEmail = User.Identity?.Name;

            //  כדי שהמחיר יתעדכן בטבלה
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

        [Authorize] // דורש התחברות כדי להוסיף שיר
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

        [HttpPost]
        [Authorize] // דורש התחברות לביצוע ההוספה בפועל
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
            return RedirectToAction("Details", new { id = playListId });
        }

        [Authorize] // דורש התחברות לצפייה בפרטי פלייליסט
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var playListSong = await _context.PlayListSong
                .Include(p => p.Songs)
                .Include(p => p.Customer)
                .FirstOrDefaultAsync(m => m.PlaylistSongID == id);

            if (playListSong == null) return NotFound();

            ViewBag.AllSongs = await _context.Song.ToListAsync();


            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var userEmail = User.Identity?.Name;
            var customer = await _context.Customer.FirstOrDefaultAsync(c => c.Email == userEmail);
            var customerIdStr = customer?.Id.ToString();

            var purchasedSongIds = await _context.OrderItems
                .Where(oi => oi.Order.CustomerID == userId ||
                             oi.Order.CustomerID == userEmail ||
                             oi.Order.CustomerID == customerIdStr)
                .Select(oi => oi.SongID)
                .Distinct()
                .ToListAsync();

            ViewBag.PurchasedSongIds = purchasedSongIds; // מעביר את רשימת השירים שנקנו ל-View

            return View(playListSong);
        }

        [Authorize] // דורש התחברות ליצירת פלייליסט
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [Authorize] // דורש התחברות ליצירת פלייליסט
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

        
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var playListSong = await _context.PlayListSong.Include(p => p.Customer).FirstOrDefaultAsync(m => m.PlaylistSongID == id);
            return View(playListSong);
        }

        [HttpPost, ActionName("Delete")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var playListSong = await _context.PlayListSong.FindAsync(id);
            if (playListSong != null) _context.PlayListSong.Remove(playListSong);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var playlist = await _context.PlayListSong.FindAsync(id);
            if (playlist == null) return NotFound();
            return View(playlist);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlaylistSongID,Title,DateAdded,CustomerID")] PlayListSong playListSong)
        {
            if (id != playListSong.PlaylistSongID) return NotFound();
            ModelState.Remove("Customer");
            ModelState.Remove("Songs");
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(playListSong);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Details", new { id = playListSong.PlaylistSongID });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PlayListSong.Any(e => e.PlaylistSongID == playListSong.PlaylistSongID)) return NotFound();
                    else throw;
                }
            }
            return View(playListSong);
        }
        [HttpPost]
        public IActionResult RemoveSong(int playlistId, int songId)
        {
            // מוצאים את הפלייליסט עם השירים שלו
            var playlist = _context.PlayListSong
                .Include(p => p.Songs)
                .FirstOrDefault(p => p.PlaylistSongID == playlistId);

            if (playlist == null) return NotFound();

            // מוצאים את השיר בתוך הפלייליסט
            var songToRemove = playlist.Songs.FirstOrDefault(s => s.SongID == songId);

            if (songToRemove != null)
            {
                playlist.Songs.Remove(songToRemove); // מסירים את השיר מהרשימה
                _context.SaveChanges(); // שומרים את השינויים בבסיס הנתונים
            }

            return RedirectToAction("Details", new { id = playlistId });
        }
    }
}