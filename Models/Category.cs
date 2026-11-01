namespace LibraryManagementSystem.Models
{
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = null!;
        public bool IsDeleted { get; set; } = false;
        public ICollection<Book> Books { get; set; } = new List<Book>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}