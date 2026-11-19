using FluentValidation;
using LibraryManagementSystem.Dtos;

namespace LibraryManagementSystem.Validators;

public class BookCreateDtoValidator : AbstractValidator<BookCreateDto>
{
    public BookCreateDtoValidator()
    {
        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Book title is required.")
            .Length(2, 200)
                .WithMessage("Book title must be between 2 and 200 characters.")
            .Matches(@"^[\p{L} ]+$")
                .WithMessage("Only letters and spaces are allowed.");

        RuleFor(x => x.Author)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Book author is required.")
            .Length(2, 100)
                .WithMessage("Book author must be between 2 and 100 characters.")
            .Matches(@"^[\p{L} ]+$")
                .WithMessage("Only letters and spaces are allowed.");

        RuleFor(x => x.Price)
            .InclusiveBetween(0.01m, 9999999999999999.99m)
            .WithMessage(
                "Price must be between 0.01 and 9999999999999999.99.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity cannot be negative.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Category must be a positive ID.");
    }
}

public class BookUpdateDtoValidator : AbstractValidator<BookUpdateDto>
{
    public BookUpdateDtoValidator()
    {
        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Book title is required.")
            .Length(2, 200)
                .WithMessage("Book title must be between 2 and 200 characters.")
            .Matches(@"^[\p{L} ]+$")
                .WithMessage("Only letters and spaces are allowed.");

        RuleFor(x => x.Author)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Book author is required.")
            .Length(2, 100)
                .WithMessage("Book author must be between 2 and 100 characters.")
            .Matches(@"^[\p{L} ]+$")
                .WithMessage("Only letters and spaces are allowed.");

        RuleFor(x => x.Price)
            .InclusiveBetween(0.01m, 9999999999999999.99m)
            .WithMessage(
                "Price must be between 0.01 and 9999999999999999.99.");

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Quantity cannot be negative.");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0)
            .WithMessage("Category must be a positive ID.");
    }
}