using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController : ControllerBase
{
    private readonly IBookService _bookService;

    public BookController(IBookService bookService)
    {
        _bookService = bookService;
    }
    
    /// <summary>
    /// Retrieves books with pagination, search by title or author,
    /// category filtering, and sorting by title, price, or creation date.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookResponseDto>>), StatusCodes.Status200OK)] 
    public async Task<IActionResult> GetAll([FromQuery] BookQueryParams query)
    {
        var result = await _bookService.GetAllAsync(query);
        return Ok(ApiResponse<PagedResult<BookResponseDto>>
            .SuccessResponse(result, "Books retrieved successfully."));
    }

    /// <summary>
    /// Retrieves a book by its ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var book = await _bookService.GetByIdAsync(id);
        return Ok(ApiResponse<BookResponseDto>
        .SuccessResponse(book, "Book retrieved successfully."));
    }
    /// <summary>
    /// Creates a new book. 
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookResponseDto>>),StatusCodes.Status200OK)]
    public async Task<IActionResult> Create ([FromBody]BookCreateDto dto)
    {
        var created = await _bookService.CreateAsync(dto);
        var response = ApiResponse<BookResponseDto>
        .SuccessResponse(created, "Book created successfully.", 201);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    /// <summary>
    /// Updates an existing book by its ID.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody]BookUpdateDto dto)
    {
        var updated = await _bookService.UpdateAsync(id, dto);
        return Ok(ApiResponse<BookResponseDto>
        .SuccessResponse(updated, "Book updated successfully."));
    }

    /// <summary>
    /// Deletes a book by its ID.
    /// </summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _bookService.DeleteAsync(id);
        return Ok(ApiResponse<BookResponseDto>
            .SuccessResponse(null, "Book deleted successfully."));    
    }
}