using FluentValidation;
using LibraryManagementSystem.Dtos;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagementSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly IValidator<CategoryCreateDto> _createValidator;
    private readonly IValidator<CategoryUpdateDto> _updateValidator;

    public CategoriesController(
        ICategoryService categoryService,
        IValidator<CategoryCreateDto> createValidator,
        IValidator<CategoryUpdateDto> updateValidator)
    {
        _categoryService = categoryService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Retrieves categories with pagination, search by name,
    /// and sorting by name or creation date.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<PagedResult<CategoryResponseDto>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] CategoryQueryParams query)
    {
        var result = await _categoryService.GetAllAsync(query);

        return Ok(ApiResponse<PagedResult<CategoryResponseDto>>
            .SuccessResponse(result, "Categories retrieved successfully."));
    }

    /// <summary>
    /// Retrieves a category by its ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<CategoryResponseDto>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _categoryService.GetByIdAsync(id);

        return Ok(ApiResponse<CategoryResponseDto>
            .SuccessResponse(category, "Category retrieved successfully."));
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(
        typeof(ApiResponse<CategoryResponseDto>),
        StatusCodes.Status201Created)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CategoryCreateDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);

        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(
                    error.PropertyName,
                    error.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        var created = await _categoryService.CreateAsync(dto);

        var response = ApiResponse<CategoryResponseDto>
            .SuccessResponse(created, "Category created successfully.", 201);

        return CreatedAtAction(
            nameof(GetById),
            new { id = created.Id },
            response);
    }

    /// <summary>
    /// Updates an existing category by its ID.
    /// </summary>
    [Authorize]
    [HttpPut("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<CategoryResponseDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update(
        int id, [FromBody] CategoryUpdateDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);

        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                ModelState.AddModelError(
                    error.PropertyName,
                    error.ErrorMessage);
            }

            return ValidationProblem(ModelState);
        }

        var updated = await _categoryService.UpdateAsync(id, dto);

        return Ok(ApiResponse<CategoryResponseDto>
            .SuccessResponse(updated, "Category updated successfully."));
    }

    /// <summary>
    /// Deletes a category by its ID.
    /// </summary>
    [Authorize]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(
        typeof(ApiResponse<object>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Delete(int id)
    {
        await _categoryService.DeleteAsync(id);

        return Ok(ApiResponse<object>
            .SuccessResponse(null, "Category deleted successfully."));
    }
}