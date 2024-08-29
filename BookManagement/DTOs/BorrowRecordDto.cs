using BookManagement.Models;

namespace BookManagement.DTOs
{
    public class BorrowRecordDto
    {
        public int UserId { get; set; }
        public int BookId { get; set; }
        public DateTime BorrowDate { get; set; }
        public DateTime? ReturnDate { get; set; }
    }
}
