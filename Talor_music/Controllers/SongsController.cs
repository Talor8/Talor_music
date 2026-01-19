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
using Talor_music.Data;
using Talor_music.Models;

namespace Talor_music.Controllers
{
    public class SongsController : Controller
    {
        private readonly Talor_musicContext _context;
        private readonly IWebHostEnvironment _env;

        public SongsController(Talor_musicContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: Songs
        public async Task<IActionResult> Index(string searchString)
        {
            // טעינת השירים יחד עם האמן שלהם
            var songs = _context.Song.Include(s => s.Artist).AsQueryable();

            // אם המשתמש הזין טקסט בתיבת החיפוש
            if (!string.IsNullOrEmpty(searchString))
            {
                songs = songs.Where(s => s.Title.Contains(searchString)
                                      || s.Artist.Name.Contains(searchString)
                                      || s.Genre.Contains(searchString));
            }

            return View(await songs.ToListAsync());
        }

        // GET: Songs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var song = await _context.Song
                .Include(s => s.Artist)
                .FirstOrDefaultAsync(m => m.SongID == id);
            if (song == null)
            {
                return NotFound();
            }

            return View(song);
        }

        // GET: Songs/Create
        public IActionResult Create()
        {
            ViewData["ArtistID"] = new SelectList(_context.Artist, "ArtistID", "Name");
            return View();
        }

        // POST: Songs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SongID,Title,Genre,Price,ArtistID")] Song song, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var uploads = Path.Combine(_env.WebRootPath, "images/songs");
                    Directory.CreateDirectory(uploads);
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                    var filePath = Path.Combine(uploads, fileName);
                    using var stream = System.IO.File.Create(filePath);
                    await imageFile.CopyToAsync(stream);
                    song.ImagePath = Path.Combine("images/songs", fileName).Replace("\\", "/");
                }

                _context.Add(song);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ArtistID"] = new SelectList(_context.Artist, "ArtistID", "Name", song.ArtistID);
            return View(song);
        }

        // GET: Songs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var song = await _context.Song.FindAsync(id);
            if (song == null)
            {
                return NotFound();
            }
            ViewData["ArtistID"] = new SelectList(_context.Artist, "ArtistID", "Name", song.ArtistID);
            return View(song);
        }

        // POST: Songs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("SongID,Title,Genre,Price,ArtistID,ImagePath")] Song song, IFormFile? imageFile)
        {
            if (id != song.SongID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var uploads = Path.Combine(_env.WebRootPath, "images/songs");
                        Directory.CreateDirectory(uploads);
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(uploads, fileName);
                        using var stream = System.IO.File.Create(filePath);
                        await imageFile.CopyToAsync(stream);
                        // optionally delete old file
                        if (!string.IsNullOrEmpty(song.ImagePath))
                        {
                            var old = Path.Combine(_env.WebRootPath, song.ImagePath.Replace('/', Path.DirectorySeparatorChar));
                            if (System.IO.File.Exists(old))
                            {
                                System.IO.File.Delete(old);
                            }
                        }
                        song.ImagePath = Path.Combine("images/songs", fileName).Replace("\\", "/");
                    }

                    _context.Update(song);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SongExists(song.SongID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ArtistID"] = new SelectList(_context.Artist, "ArtistID", "Name", song.ArtistID);
            return View(song);
        }

        // GET: Songs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var song = await _context.Song
                .Include(s => s.Artist)
                .FirstOrDefaultAsync(m => m.SongID == id);
            if (song == null)
            {
                return NotFound();
            }

            return View(song);
        }

        // POST: Songs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var song = await _context.Song.FindAsync(id);
            if (song != null)
            {
                _context.Song.Remove(song);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool SongExists(int id)
        {
            return _context.Song.Any(e => e.SongID == id);
        }
    }
}
