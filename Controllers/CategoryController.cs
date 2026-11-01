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

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] CategoryQueryParams query)
    {
        var result = await _categoryService.GetAllAsync(query);
        return Ok(ApiResponse<PagedResult<CategoryResponseDto>>
            .SuccessResponse(result, "Categories retrieved successfully."));
    }

    
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        if (category == null)
            return NotFound(ApiResponse<CategoryResponseDto>
                .FailResponse($"Category with id {id} not found.", 404));

        return Ok(ApiResponse<CategoryResponseDto>
            .SuccessResponse(category, "Category retrieved successfully."));
    }

    
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
    {
        var created = await _categoryService.CreateAsync(dto);

        var response = ApiResponse<CategoryResponseDto>
            .SuccessResponse(created, "Category created successfully.", 201);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, response);
    }

    
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
    {
        var updated = await _categoryService.UpdateAsync(id, dto);

        if (updated == null)
            return NotFound(ApiResponse<CategoryResponseDto>
                .FailResponse($"Category with id {id} not found.", 404)); 

        return Ok(ApiResponse<CategoryResponseDto>
            .SuccessResponse(updated, "Category updated successfully."));
    }

    
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _categoryService.DeleteAsync(id);

        if (!deleted)
            return BadRequest(ApiResponse<object>
                .FailResponse("Could not delete the category.", 400)); // TODO Step 8: precise errors

        return Ok(ApiResponse<object>
            .SuccessResponse(null, "Category deleted successfully."));
    }
}