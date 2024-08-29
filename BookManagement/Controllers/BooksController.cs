using BookManagement.Data;
using BookManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Controllers
{
    public class BooksController : Controller
    {
        private readonly BookDbContext _db;

        public BooksController(BookDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var books = await _db.Books
                .Include(b => b.BorrowRecords)
                .ThenInclude(br => br.User)
                .ToListAsync();
            return View(books);
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _db.Books
                .Include(b => b.BorrowRecords)
                .ThenInclude(br => br.User)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Book book)
        {
            System.Diagnostics.Debug.WriteLine(ModelState.IsValid);
            if (ModelState.IsValid)
            {
                _db.Books.Add(book);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(book);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var book = await _db.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Book book)
        {
            if (id != book.BookId || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var bookToEdit = await _db.Books.FindAsync(id);
            if (bookToEdit == null)
            {
                return NotFound();
            }

            bookToEdit.Title = book.Title;
            bookToEdit.Author = book.Author;
            bookToEdit.Genre = book.Genre;
            bookToEdit.Pages = book.Pages;
            bookToEdit.Price = book.Price;
            bookToEdit.Description = book.Description;

            _db.Entry(bookToEdit).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if(id==null || id == 0)
            {
                return NotFound();
            }
            var book = await _db.Books
                .Include(b => b.BorrowRecords)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            return View(book);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            var book = await _db.Books
                .Include(b => b.BorrowRecords)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return NotFound();
            }

            _db.BorrowRecords.RemoveRange(book.BorrowRecords);
            _db.Books.Remove(book);
            await _db.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
