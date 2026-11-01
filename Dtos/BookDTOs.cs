namespace LibraryManagementSystem.Dtos;
public class BookCreateDto
{
    public string Title { get; set; } = string.Empty;
    public string Author {get; set; } = string.Empty;
     public decimal Price { get; set; }  
    public int Quantity { get; set; }  
    public int CategoryId { get; set; }
}

public class BookUpdateDto
{
    public string Title { get; set; } = string.Empty;
    public string Author {get; set; } = string.Empty;
     public decimal Price { get; set; }  
    public int Quantity { get; set; }  
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