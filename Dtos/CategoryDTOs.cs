namespace LibraryManagementSystem.Dtos;
public  class CategoryCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

}

public class CategoryUpdateDto
{
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