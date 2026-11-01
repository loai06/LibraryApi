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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] BookQueryParams query)
    {
        var result = await _bookService.GetAllAsync(query);
        return Ok(ApiResponse<PagedResult<BookResponseDto>>
            .SuccessResponse(result, "Books retrieved successfully."));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var book = await _bookService.GetByIdAsync(id);
        if (book == null)
            return NotFound(ApiResponse<BookResponseDto>
                .FailResponse($"Book with id {id} not found.", 404));
        return Ok(ApiResponse<BookResponseDto>
        .SuccessResponse(book, "Book retrieved successfully."));
    }

    [HttpPost]
    public async Task<IActionResult> Create ([FromBody]BookCreateDto dto)
    {
        var created = await _bookService.CreateAsync(dto);
        var response = ApiResponse<BookResponseDto>
        .SuccessResponse(created, "Book created successfully.", 201);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody]BookUpdateDto dto)
    {
        var updated = await _bookService.UpdateAsync(id, dto);
        return Ok(ApiResponse<BookResponseDto>
        .SuccessResponse(updated, "Book updated successfully."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _bookService.DeleteAsync(id);
        if (!deleted)
        return NotFound(ApiResponse<BookResponseDto>
            .FailResponse($"Book with id {id} not found.", 404));

        return Ok(ApiResponse<BookResponseDto>
            .SuccessResponse(null, "Book deleted successfully."));    
    }
}