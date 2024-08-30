﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BookManagement.Models
{
    public class Book
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        public string Author { get; set; }

        [Required(ErrorMessage = "Genre is required")]
        public int? GenreId { get; set; }
        public Genre? Genre { get; set; }

        public int Pages { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Price must be a positive value")]
        public int Price { get; set; }

        [Required(ErrorMessage = "Description is required")]
        public string Description { get; set; }

        public ICollection<BorrowRecord>? BorrowRecords { get; set; }
    }
}
