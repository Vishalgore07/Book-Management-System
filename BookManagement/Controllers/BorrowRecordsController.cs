using BookManagement.Data;
using BookManagement.DTOs;
using BookManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Controllers
{
    public class BorrowRecordsController : Controller
    {
        private readonly BookDbContext _db;

        public BorrowRecordsController(BookDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var borrowRecords = await _db.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .ToListAsync();
            return View(borrowRecords);
        }

        public async Task<IActionResult> Details(int id)
        {
            var borrowRecord = await _db.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (borrowRecord == null)
            {
                return NotFound();
            }

            var borrowRecordDto = new BorrowRecordDto
            {
                BookId = borrowRecord.BookId,
                UserId = borrowRecord.UserId,
                BorrowDate = borrowRecord.BorrowDate,
                ReturnDate = borrowRecord.ReturnDate
            };

            return View(borrowRecordDto);
        }

        public IActionResult Create()
        {
            ViewBag.Users = new SelectList(_db.Users, "UserId", "UserName");
            ViewBag.Books = new SelectList(_db.Books, "BookId", "Title");
            var borrowRecordDto = new BorrowRecordDto
            {
                BorrowDate = DateTime.Now
            };
            return View(borrowRecordDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BorrowRecordDto borrowRecordDto)
        {
            if (ModelState.IsValid)
            {
                var userExists = await _db.Users.AnyAsync(u => u.UserId == borrowRecordDto.UserId);
                var bookExists = await _db.Books.AnyAsync(b => b.BookId == borrowRecordDto.BookId);

                if (!userExists || !bookExists)
                {
                    return BadRequest("User or Book does not exist.");
                }

                var borrowDate = borrowRecordDto.BorrowDate == default(DateTime) ? DateTime.Now : borrowRecordDto.BorrowDate;
                var borrowRecord = new BorrowRecord
                {
                    BookId = borrowRecordDto.BookId,
                    UserId = borrowRecordDto.UserId,
                    BorrowDate = borrowDate,
                    ReturnDate = borrowRecordDto.ReturnDate
                };

                _db.BorrowRecords.Add(borrowRecord);
                await _db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Users = new SelectList(_db.Users, "UserId", "UserName");
            ViewBag.Books = new SelectList(_db.Books, "BookId", "Title");

            return View(borrowRecordDto);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var borrowRecord = await _db.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (borrowRecord == null)
            {
                return NotFound();
            }

            ViewBag.Users = new SelectList(await _db.Users.ToListAsync(), "UserId", "UserName");
            ViewBag.Books = new SelectList(await _db.Books.ToListAsync(), "BookId", "Title");

            var borrowRecordDto = new BorrowRecordDto
            {
                BookId = borrowRecord.BookId,
                UserId = borrowRecord.UserId,
                BorrowDate = borrowRecord.BorrowDate,
                ReturnDate = borrowRecord.ReturnDate
            };

            return View(borrowRecordDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BorrowRecordDto borrowRecordDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Users = new SelectList(await _db.Users.ToListAsync(), "UserId", "UserName");
                ViewBag.Books = new SelectList(await _db.Books.ToListAsync(), "BookId", "Title");
                return BadRequest(ModelState);
            }

            var recordToEdit = await _db.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (recordToEdit == null)
            {
                return NotFound();
            }

            recordToEdit.BookId = borrowRecordDto.BookId;
            recordToEdit.UserId = borrowRecordDto.UserId;
            recordToEdit.BorrowDate = borrowRecordDto.BorrowDate;
            recordToEdit.ReturnDate = borrowRecordDto.ReturnDate;

            _db.Entry(recordToEdit).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var borrowRecord = await _db.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (borrowRecord == null)
            {
                return NotFound();
            }

            return View(borrowRecord);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var borrowRecord = await _db.BorrowRecords
                .FirstOrDefaultAsync(x => x.Id == id);

            if (borrowRecord == null)
            {
                return NotFound();
            }

            _db.BorrowRecords.Remove(borrowRecord);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> UserBorrowRecords(int userId)
        {
            var borrowRecords = await _db.BorrowRecords
                .Include(br => br.Book)
                .Include(br => br.User)
                .Where(br => br.UserId == userId)
                .ToListAsync();

            var user = await _db.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            ViewBag.UserName = user.UserName;

            return View(borrowRecords);
        }

        public IActionResult BorrowBookByUserId(int userId)
        {
            var user = _db.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            var borrowRecordDto = new BorrowRecordDto
            {
                UserId = userId,
                BorrowDate = DateTime.Now
            };

            ViewBag.Books = new SelectList(_db.Books, "BookId", "Title");
            ViewBag.UserName = user.UserName;

            return View(borrowRecordDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowBookByUserId(BorrowRecordDto borrowRecordDto)
        {
            if (ModelState.IsValid)
            {
                var userExists = await _db.Users.AnyAsync(u => u.UserId == borrowRecordDto.UserId);
                var bookExists = await _db.Books.AnyAsync(b => b.BookId == borrowRecordDto.BookId);

                if (!userExists || !bookExists)
                {
                    return BadRequest("User or Book does not exist.");
                }
                else
                {
                    var borrowRecord = new BorrowRecord
                    {
                        BookId = borrowRecordDto.BookId,
                        UserId = borrowRecordDto.UserId,
                        BorrowDate = borrowRecordDto.BorrowDate,
                        ReturnDate = borrowRecordDto.ReturnDate
                    };

                    _db.BorrowRecords.Add(borrowRecord);
                    await _db.SaveChangesAsync();

                    return RedirectToAction("UserBorrowRecords","BorrowRecords",new {userId = borrowRecordDto.UserId});
                }
            }

            ViewBag.Books = new SelectList(_db.Books, "BookId", "Title");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == borrowRecordDto.UserId);
            ViewBag.UserName = user?.UserName ?? "Unknown user";

            return View(borrowRecordDto);
        }
    }
}
