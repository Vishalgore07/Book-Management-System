using System.Text.Json.Serialization;

namespace BookManagement.Models
{
    public class BorrowRecord
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int BookId { get; set; }
        public Book Book { get; set; }
        public DateTime BorrowDate { get; set; } = DateTime.Now;
        public DateTime? ReturnDate { get; set; }

        [JsonIgnore]
        public User User { get; set; }


    }
}
