using System.ComponentModel.DataAnnotations;
namespace LibraryManagementSystem.Dtos;
public class BookCreateDto
{
    [Required(ErrorMessage = "Book title is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Book title must be between 2 and 200 characters.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Only letters and spaces are allowed.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Book author is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Book author must be between 2 and 100 characters.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Only letters and spaces are allowed.")]
    public string Author {get; set; } = string.Empty;

   [Range(typeof(decimal), "0.01", "9999999999999999.99",
    ErrorMessage = "Price must be between 0.01 and 9999999999999999.99.")]
    public decimal Price { get; set; } 

    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    public int Quantity { get; set; }  

    [Required(ErrorMessage = "Category is required.")]
    public int CategoryId { get; set; }
}

public class BookUpdateDto
{
    [Required(ErrorMessage = "Book title is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Book title must be between 2 and 200 characters.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Only letters and spaces are allowed.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Book author is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Book author must be between 2 and 100 characters.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Only letters and spaces are allowed.")]
    public string Author {get; set; } = string.Empty;
    [Range(typeof(decimal), "0.01", "9999999999999999.99",
    ErrorMessage = "Price must be between 0.01 and 9999999999999999.99.")]
     public decimal Price { get; set; }  
    [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative.")]
    public int Quantity { get; set; }  
    [Required(ErrorMessage = "Category is required.")]
    public int CategoryId { get; set; }
    
}

public class BookResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author {get; set; } = string.Empty;
     public decimal Price { get; set; }  
    public int Quantity { get; set; }  
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

}
public class BookQueryParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }      
    public int? CategoryId { get; set; }     
    public string? SortBy { get; set; }      
    public bool Descending { get; set; }
}