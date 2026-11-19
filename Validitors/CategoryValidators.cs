using FluentValidation;
using LibraryManagementSystem.Dtos;

namespace LibraryManagementSystem.Validators;

public class CategoryCreateDtoValidator
    : AbstractValidator<CategoryCreateDto>
{
    public CategoryCreateDtoValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Category name is required.")
            .Length(2, 100)
                .WithMessage("Category name must be between 2 and 100 characters.")
            .Matches(@"^[\p{L} ]+$")
                .WithMessage("Only letters and spaces are allowed.");
    }
}

public class CategoryUpdateDtoValidator
    : AbstractValidator<CategoryUpdateDto>
{
    public CategoryUpdateDtoValidator()
    {
        RuleFor(x => x.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Category name is required.")
            .Length(2, 100)
                .WithMessage("Category name must be between 2 and 100 characters.")
            .Matches(@"^[\p{L} ]+$")
                .WithMessage("Only letters and spaces are allowed.");
    }
}