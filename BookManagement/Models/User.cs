using System.Text.Json.Serialization;

namespace BookManagement.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set;}

        [JsonIgnore]
        public ICollection<BorrowRecord>? BorrowRecords { get; set; }
    }
}
