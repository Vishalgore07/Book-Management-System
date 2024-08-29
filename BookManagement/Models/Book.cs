using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookManagement.Models
{
    public class Book
    {
        [Key]
        public int BookId { get; set; }
        [Required]
        public string Title { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
        public int Pages { get; set; }
        public int Price { get; set; }
        [Required]
        public string Description { get; set; }
        public ICollection<BorrowRecord>? BorrowRecords { get; set; }

    }
}
