using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Talor_music.Data;
using Talor_music.Models;

namespace Talor_music.Controllers
{
    [Authorize] // ברירת מחדל: רק משתמשים מחוברים
    public class SongsController : Controller
    {
        private readonly Talor_musicContext _context;
        private readonly IWebHostEnvironment _env;

        public SongsController(Talor_musicContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // --- כולם יכולים לראות את רשימת השירים ---
        [AllowAnonymous]
        public async Task<IActionResult> Index(string searchString)
        {
            var songs = _context.Song.Include(s => s.Artist).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                songs = songs.Where(s => s.Title.Contains(searchString)
                                      || s.Artist.Name.Contains(searchString)
                                      || s.Genre.Contains(searchString));
            }

            return View(await songs.ToListAsync());
        }

        // --- כולם יכולים לראות פרטים ---
        [AllowAnonymous]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var song = await _context.Song
                .Include(s => s.Artist)
                .FirstOrDefaultAsync(m => m.SongID == id);

            if (song == null) return NotFound();

            return View(song);
        }

        // --- רק מנהל יכול להוסיף שירים ---
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["ArtistID"] = new SelectList(_context.Artist, "ArtistID", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("SongID,Title,Genre,Price,ArtistID")] Song song, IFormFile? imageFile, IFormFile? audioFile)
        {
            if (ModelState.IsValid)
            {
                // שמירת תמונה
                if (imageFile != null && imageFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string uploadPath = Path.Combine(_env.WebRootPath, "images", fileName);
                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    song.ImagePath = "/images/" + fileName;
                }

                // שמירת קובץ אודיו
                if (audioFile != null && audioFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(audioFile.FileName);
                    string uploadPath = Path.Combine(_env.WebRootPath, "audio", fileName);
                    using (var stream = new FileStream(uploadPath, FileMode.Create))
                    {
                        await audioFile.CopyToAsync(stream);
                    }
                    song.AudioFilePath = "/audio/" + fileName;
                }

                _context.Add(song);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ArtistID"] = new SelectList(_context.Artist, "ArtistID", "Name", song.ArtistID);
            return View(song);
        }

        // --- רק מנהל יכול לערוך ---
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var song = await _context.Song.FindAsync(id);
            if (song == null) return NotFound();
            ViewData["ArtistID"] = new SelectList(_context.Artist, "ArtistID", "Name", song.ArtistID);
            return View(song);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("SongID,Title,Genre,Price,ArtistID,ImagePath,AudioFilePath")] Song song, IFormFile? imageFile, IFormFile? audioFile)
        {
            if (id != song.SongID) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // עדכון תמונה (רק אם הועלתה חדשה)
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string uploadPath = Path.Combine(_env.WebRootPath, "images", fileName);
                        using (var stream = new FileStream(uploadPath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        song.ImagePath = "/images/" + fileName;
                    }

                    // עדכון אודיו (רק אם הועלה חדש)
                    if (audioFile != null && audioFile.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(audioFile.FileName);
                        string uploadPath = Path.Combine(_env.WebRootPath, "audio", fileName);
                        using (var stream = new FileStream(uploadPath, FileMode.Create))
                        {
                            await audioFile.CopyToAsync(stream);
                        }
                        song.AudioFilePath = "/audio/" + fileName;
                    }

                    _context.Update(song);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SongExists(song.SongID)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ArtistID"] = new SelectList(_context.Artist, "ArtistID", "Name", song.ArtistID);
            return View(song);
        }

        // --- רק מנהל יכול למחוק ---
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var song = await _context.Song.Include(s => s.Artist).FirstOrDefaultAsync(m => m.SongID == id);
            if (song == null) return NotFound();
            return View(song);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var song = await _context.Song.FindAsync(id);
            if (song != null) _context.Song.Remove(song);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SongExists(int id) => _context.Song.Any(e => e.SongID == id);
    }
}