using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// Retrieves categories with pagination, search by name, and sorting by name or creation date.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<CategoryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] CategoryQueryParams query)
    {
        var result = await _categoryService.GetAllAsync(query);
        return Ok(ApiResponse<PagedResult<CategoryResponseDto>>
            .SuccessResponse(result, "Categories retrieved successfully."));
    }

    /// <summary>
    /// Retrieves a category by its ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);
        return Ok(ApiResponse<CategoryResponseDto>
            .SuccessResponse(category, "Category retrieved successfully."));
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
    {
        var created = await _categoryService.CreateAsync(dto);

        var response = ApiResponse<CategoryResponseDto>
            .SuccessResponse(created, "Category created successfully.", 201);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    /// <summary>
    /// Updates an existing category by its ID.
    /// </summary>

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<CategoryResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
    {
        var updated = await _categoryService.UpdateAsync(id, dto);
        return Ok(ApiResponse<CategoryResponseDto>
            .SuccessResponse(updated, "Category updated successfully."));
    }

    /// <summary>
    /// Deletes a category by its ID.
    /// /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);
        return Ok(ApiResponse<object>
            .SuccessResponse(null, "Category deleted successfully."));
    }
}