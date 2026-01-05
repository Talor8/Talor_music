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

        // GET: PlayListSongs
        public async Task<IActionResult> Index()
        {
            return View(await _context.PlayListSong.ToListAsync());
        }

        // GET: PlayListSongs/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playListSong = await _context.PlayListSong
                .FirstOrDefaultAsync(m => m.PlaylistSongID == id);
            if (playListSong == null)
            {
                return NotFound();
            }

            ViewBag.Songs = await _context.Song.ToListAsync();

            return View(playListSong);
        }

        // GET: PlayListSongs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: PlayListSongs/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PlaylistSongID,Title,CustomerID,DateAdded")] PlayListSong playListSong)
        {
            if (ModelState.IsValid)
            {
                _context.Add(playListSong);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(playListSong);
        }

        // GET: PlayListSongs/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playListSong = await _context.PlayListSong.FindAsync(id);
            if (playListSong == null)
            {
                return NotFound();
            }
            return View(playListSong);
        }

        // POST: PlayListSongs/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("PlaylistSongID,Title,CustomerID,DateAdded")] PlayListSong playListSong)
        {
            if (id != playListSong.PlaylistSongID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(playListSong);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PlayListSongExists(playListSong.PlaylistSongID))
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
            return View(playListSong);
        }

        // GET: PlayListSongs/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var playListSong = await _context.PlayListSong
                .FirstOrDefaultAsync(m => m.PlaylistSongID == id);
            if (playListSong == null)
            {
                return NotFound();
            }

            return View(playListSong);
        }

        // POST: PlayListSongs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var playListSong = await _context.PlayListSong.FindAsync(id);
            if (playListSong != null)
            {
                _context.PlayListSong.Remove(playListSong);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        //Harel Function to add song to playlist
        public async Task<IActionResult> AddSongToListAction(int playListId, int songId)
        {
            var playList = await _context.PlayListSong.FindAsync(playListId);
            if (playList != null)
            {
                var song = await _context.Song.FindAsync(songId);
                if (song != null)
                {
                    playList.Songs.Add(song);
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction("Details", new { Id = playListId });
        }
        public async Task<IActionResult> AddSongToPlayList(int songId)
        {
            return View(songId);
        }

        private bool PlayListSongExists(int id)
        {
            return _context.PlayListSong.Any(e => e.PlaylistSongID == id);
        }
    }
}
