namespace LibraryManagementSystem.Models
{
    public class Book
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public decimal Price { get; set; } = 0m;
        public int Quantity { get; set; } = 0;
        public int CategoryId { get; set; }
        public Category? Category { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }




}