using BookManagement.Data;
using BookManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookManagement.Controllers
{
    public class UsersController : Controller
    {
        private readonly BookDbContext _db;

        public UsersController(BookDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _db.Users
                .Include(u => u.BorrowRecords)
                .ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _db.Users
                .Include(u => u.BorrowRecords)
                .ThenInclude(br => br.Book)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (ModelState.IsValid)
            {
                _db.Users.Add(user);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.UserId || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userToEdit = await _db.Users.FindAsync(id);
            if (userToEdit == null)
            {
                return NotFound();
            }

            userToEdit.UserName = user.UserName;
            userToEdit.Email = user.Email;
            userToEdit.Phone = user.Phone;

            _db.Entry(userToEdit).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var user = await _db.Users
                .Include(u => u.BorrowRecords)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _db.Users
                .Include(u => u.BorrowRecords)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
            {
                return NotFound();
            }

            _db.BorrowRecords.RemoveRange(user.BorrowRecords);
            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

    }
}
