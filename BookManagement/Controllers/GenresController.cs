using BookManagement.Data;
using BookManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Controllers
{
    public class GenresController : Controller
    {
        private readonly BookDbContext _db;

        public GenresController(BookDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var genre = await _db.Genres
                .Include(x=>x.Books)
                .ToListAsync();
            return View(genre);
        }

        public async Task<IActionResult> Details(int id)
        {
            var genre = await _db.Genres
                .Include(u => u.Books)
                .FirstOrDefaultAsync(u => u.GenreId == id);

            if (genre == null)
            {
                return NotFound();
            }
            return View(genre);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Genre genre)
        {
            if (ModelState.IsValid)
            {
                _db.Genres.Add(genre);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(genre);
        }


        public async Task<IActionResult> Edit(int id)
        {
            var genre = await _db.Genres.FindAsync(id);
            if (genre == null)
            {
                return NotFound();
            }
            return View(genre);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Genre genre)
        {
            if (id != genre.GenreId || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var genreToEdit = await _db.Genres.FindAsync(id);
            if (genreToEdit == null)
            {
                return NotFound();
            }

            genreToEdit.Name = genre.Name;

            _db.Entry(genreToEdit).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var genre = await _db.Genres
                .Include(u => u.Books)
                .FirstOrDefaultAsync(u => u.GenreId == id);

            if (genre == null)
            {
                return NotFound();
            }

            return View(genre);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var genre = await _db.Genres
                .Include(u => u.Books)
                .FirstOrDefaultAsync(u => u.GenreId == id);

            if (genre == null)
            {
                return NotFound();
            }

            _db.Books.RemoveRange(genre.Books);
            _db.Genres.Remove(genre);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
