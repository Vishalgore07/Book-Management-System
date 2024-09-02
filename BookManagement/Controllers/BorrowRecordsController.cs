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

        public async Task<IActionResult> Create()
        {
            ViewBag.Users = new SelectList(_db.Users, "UserId", "UserName");
            
            //only select books that are in stock
            ViewBag.Books = new SelectList(await _db.Books.Where(b=>b.Stock>0).ToListAsync(), "BookId", "Title");
            
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
                var book = await _db.Books.FirstOrDefaultAsync(b => b.BookId == borrowRecordDto.BookId);

                if (!userExists || book==null)
                {
                    return BadRequest("User or Book does not exist.");
                }

                // Check for active borrow record for this book by the same user
                var activeBorrowRecord = await _db.BorrowRecords
                    .AnyAsync(br => br.UserId == borrowRecordDto.UserId
                                    && br.BookId == borrowRecordDto.BookId
                                    && (br.ReturnDate == null || br.ReturnDate > DateTime.Now));

                if (activeBorrowRecord)
                {
                    ModelState.AddModelError("", "User already have an active borrow record for this book. You cannot borrow the same book until you return it.");
                    ViewBag.Users = new SelectList(_db.Users, "UserId", "UserName");
                    ViewBag.Books = new SelectList(await _db.Books.Where(b => b.Stock > 0).ToListAsync(), "BookId", "Title");
                    return View(borrowRecordDto);
                }

                if (book.Stock <= 0)
                {
                    ModelState.AddModelError("", "The selected book is not available for borrowing");
                    ViewBag.Users = new SelectList(_db.Users, "UserId", "UserName");
                    ViewBag.Books = new SelectList(_db.Books, "BookId", "Title");
                    return View(borrowRecordDto);
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

                //update book stock
                book.Stock--;

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

            //if the current book is still selected, include it in the list even if out of stock
            var books = await _db.Books
                            .Where(b => b.Stock > 0 || b.BookId == borrowRecord.BookId)
                            .ToListAsync();
            
            ViewBag.Books = new SelectList(books, "BookId", "Title");

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

                var books = await _db.Books
                                .Where(b => b.Stock > 0 || b.BookId == borrowRecordDto.BookId)
                                .ToListAsync();
                
                ViewBag.Books = new SelectList(books, "BookId", "Title");
                
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

            //Check if book is changed
            if (recordToEdit.BookId != borrowRecordDto.BookId)
            {
                var newBook = await _db.Books.FindAsync(borrowRecordDto.BookId);
                if (newBook == null || newBook.Stock <= 0)
                {
                    ModelState.AddModelError("", "Selected book is not available");
                    ViewBag.Users = new SelectList(await _db.Users.ToListAsync(), "UserId", "UserName");

                    var books = await _db.Books
                                    .Where(b => b.Stock > 0 || b.BookId == recordToEdit.BookId)
                                    .ToListAsync();

                    ViewBag.Books = new SelectList(books, "BookId", "Title");
                    return View(borrowRecordDto);
                }

                var activeBorrowRecord = await _db.BorrowRecords
                                                .AnyAsync(br => br.UserId == borrowRecordDto.UserId
                                                && br.BookId == borrowRecordDto.BookId
                                                && (br.ReturnDate == null || br.ReturnDate > DateTime.Now));

                if (activeBorrowRecord)
                {
                    ModelState.AddModelError("", "User already have an active borrow record for this book. You cannot borrow the same book until you return it.");
                    ViewBag.Users = new SelectList(await _db.Users.ToListAsync(), "UserId", "UserName");

                    var books = await _db.Books
                                        .Where(b => b.Stock > 0 || b.BookId == recordToEdit.BookId)
                                        .ToListAsync();

                    ViewBag.Books = new SelectList(books, "BookId", "Title");
                    return View(borrowRecordDto);
                }

                //update stock of previous book
                var oldBook = await _db.Books.FindAsync(recordToEdit.BookId);
                oldBook.Stock++;

                //update stock of new book
                newBook.Stock--;

                _db.Entry(oldBook).State = EntityState.Modified;
                _db.Entry(newBook).State = EntityState.Modified;
            }

            var book = await _db.Books.FindAsync(borrowRecordDto.BookId);
            if (recordToEdit.ReturnDate != borrowRecordDto.ReturnDate)
            {
                //increment stock when the book is returned
                if(recordToEdit.ReturnDate.HasValue && recordToEdit.ReturnDate.Value < DateTime.Now)
                {
                    book.Stock++;
                }

                recordToEdit.ReturnDate = borrowRecordDto.ReturnDate;

                //if the new return date is set to a past date , then stock is updated
                if(borrowRecordDto.ReturnDate.HasValue && borrowRecordDto.ReturnDate.Value < DateTime.Now)
                {
                    book.Stock++;
                }
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

            //update the book stock when a borrow record is deleted
            var book = await _db.Books.FindAsync(borrowRecord.BookId);
            if (book != null)
            {
                book.Stock++;
                _db.Entry(book).State = EntityState.Modified;
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

            ViewBag.Books = new SelectList(_db.Books.Where(b => b.Stock > 0), "BookId", "Title");
            ViewBag.UserName = user.UserName;

            return View(borrowRecordDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BorrowBookByUserId(BorrowRecordDto borrowRecordDto)
        {
            
            if (ModelState.IsValid)
            {
                //check if the user already has an active borrow record for selected book
                var activeBorrowRecord = await _db.BorrowRecords
                                             .AnyAsync(br => br.UserId == borrowRecordDto.UserId 
                                                             && br.BookId==borrowRecordDto.BookId 
                                                             && (br.ReturnDate == null || br.ReturnDate > DateTime.Now));
                if (activeBorrowRecord)
                {
                    ModelState.AddModelError("", "User already have an active borrow record for this book. You cannot borrow the same book until you return it.");
                    ViewBag.Books = new SelectList(await _db.Books.Where(b => b.Stock > 0).ToListAsync(), "BookId", "Title");
                    ViewBag.UserName = await _db.Users.Where(u => u.UserId == borrowRecordDto.UserId)
                                                      .Select(u => u.UserName)
                                                      .FirstOrDefaultAsync();
                    return View(borrowRecordDto);
                }


                var userExists = await _db.Users.AnyAsync(u => u.UserId == borrowRecordDto.UserId);
                var book = await _db.Books.FirstOrDefaultAsync(b => b.BookId == borrowRecordDto.BookId);

                if (!userExists || book == null)
                {
                    return BadRequest("User or Book does not exist.");
                }

                if (book.Stock <= 0)
                {
                    ModelState.AddModelError("", "The selected book is not available for borrowing");
                    ViewBag.Books = new SelectList(_db.Books, "BookId", "Title");
                    ViewBag.UserName = await _db.Users.Where(u => u.UserId == borrowRecordDto.UserId)
                                                    .Select(u => u.UserName)
                                                    .FirstOrDefaultAsync();
                    return View(borrowRecordDto);
                }

                var borrowRecord = new BorrowRecord
                {
                    BookId = borrowRecordDto.BookId,
                    UserId = borrowRecordDto.UserId,
                    BorrowDate = borrowRecordDto.BorrowDate,
                    ReturnDate = borrowRecordDto.ReturnDate
                };

                _db.BorrowRecords.Add(borrowRecord);

                //update the stock
                book.Stock--;

                await _db.SaveChangesAsync();

                return RedirectToAction("UserBorrowRecords", "BorrowRecords", new { userId = borrowRecordDto.UserId });
            }

            ViewBag.Books = new SelectList(await _db.Books.Where(b => b.Stock > 0).ToListAsync(), "BookId", "Title");

            var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == borrowRecordDto.UserId);
            ViewBag.UserName = user?.UserName ?? "Unknown user";

            return View(borrowRecordDto);
        }
    }
}
