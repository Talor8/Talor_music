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
    [Authorize] // כברירת מחדל: רק משתמשים מחוברים יכולים לגשת
    public class SongsController : Controller
    {
        private readonly Talor_musicContext _context;
        private readonly IWebHostEnvironment _env;

        public SongsController(Talor_musicContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [AllowAnonymous] // כולם יכולים לראות את הרשימה
        public async Task<IActionResult> Index(string searchString)
        {
            var songs = _context.Song.Include(s => s.Artist).AsQueryable();
            if (!string.IsNullOrEmpty(searchString))
            {
                songs = songs.Where(s => s.Title.Contains(searchString) || s.Artist.Name.Contains(searchString) || s.Genre.Contains(searchString));
            }
            return View(await songs.ToListAsync());
        }

        [AllowAnonymous] // כולם יכולים לראות פרטים
   
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            // הוספנו Include ל-Reviews וגם ל-Customer שכתב כל ביקורת
            var song = await _context.Song
                .Include(s => s.Artist)
                .Include(s => s.Reviews)
                    .ThenInclude(r => r.Customer)
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
                if (imageFile != null && imageFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    string uploadPath = Path.Combine(_env.WebRootPath, "images", fileName);
                    using (var stream = new FileStream(uploadPath, FileMode.Create)) { await imageFile.CopyToAsync(stream); }
                    song.ImagePath = "/images/" + fileName;
                }

                if (audioFile != null && audioFile.Length > 0)
                {
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(audioFile.FileName);
                    string uploadPath = Path.Combine(_env.WebRootPath, "audio", fileName);
                    using (var stream = new FileStream(uploadPath, FileMode.Create)) { await audioFile.CopyToAsync(stream); }
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
                    // טיפול בתמונה
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        string uploadPath = Path.Combine(_env.WebRootPath, "images", fileName);
                        using (var stream = new FileStream(uploadPath, FileMode.Create)) { await imageFile.CopyToAsync(stream); }
                        song.ImagePath = "/images/" + fileName;
                    }
                    else { _context.Entry(song).Property(x => x.ImagePath).IsModified = false; }

                    // טיפול באודיו
                    if (audioFile != null && audioFile.Length > 0)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(audioFile.FileName);
                        string uploadPath = Path.Combine(_env.WebRootPath, "audio", fileName);
                        using (var stream = new FileStream(uploadPath, FileMode.Create)) { await audioFile.CopyToAsync(stream); }
                        song.AudioFilePath = "/audio/" + fileName;
                    }
                    else { _context.Entry(song).Property(x => x.AudioFilePath).IsModified = false; }

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int songId, int rating, string comment)
        {
            // מוצאים את המשתמש המחובר לפי האימייל שלו
            var customer = _context.Customer.FirstOrDefault(c => c.Email == User.Identity.Name);

            if (customer == null)
            {
                return Unauthorized(); // אם המשתמש לא מחובר
            }

            var review = new Review
            {
                SongID = songId,
                Rating = rating,
                Comment = comment,
                CustomerID = customer.Id,
                DatePosted = DateTime.Now
            };

            _context.Review.Add(review);
            await _context.SaveChangesAsync();

            // חוזרים לדף ה-Details של השיר כדי לראות את התגובה החדשה
            return RedirectToAction("Details", new { id = songId });
        }
        private bool SongExists(int id) => _context.Song.Any(e => e.SongID == id);
        

        
    }
}