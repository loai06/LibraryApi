using System.ComponentModel.DataAnnotations;
namespace LibraryManagementSystem.Dtos;
public  class CategoryCreateDto
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Only letters and spaces are allowed.")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

}

public class CategoryUpdateDto
{
    [Required(ErrorMessage = "Category name is required.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Category name must be between 2 and 100 characters.")]
    [RegularExpression(@"^[\p{L} ]+$", ErrorMessage = "Only letters and spaces are allowed.")]
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class CategoryResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
public class CategoryQueryParams
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Search { get; set; }      
    public string? SortBy { get; set; }      
    public bool Descending { get; set; }
}