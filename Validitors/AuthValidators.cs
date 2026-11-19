using FluentValidation;
using LibraryManagementSystem.Dtos;

namespace LibraryManagementSystem.Validators;

public class RegisterDtoValidator : AbstractValidator<RegisterDTO>
{
    public RegisterDtoValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .WithMessage("Full name is required.");

        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Email is required.")
            .EmailAddress()
                .WithMessage("Invalid email address.");

        RuleFor(x => x.Password)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Password is required.")
            .Length(8, 100)
                .WithMessage(
                    "Password must be between 8 and 100 characters.");
    }
}

public class LoginDtoValidator : AbstractValidator<LoginDTO>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Email is required.")
            .EmailAddress()
                .WithMessage("Invalid email address.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.");
    }
}

public class ForgotPasswordDtoValidator
    : AbstractValidator<ForgotPasswordDto>
{
    public ForgotPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Email is required.")
            .EmailAddress()
                .WithMessage("Invalid email address.");
    }
}

public class ResetPasswordDtoValidator
    : AbstractValidator<ResetPasswordDto>
{
    public ResetPasswordDtoValidator()
    {
        RuleFor(x => x.Email)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Email is required.")
            .EmailAddress()
                .WithMessage("Invalid email address.");

        RuleFor(x => x.Code)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Code is required.")
            .Matches(@"^[0-9]{6}$")
                .WithMessage("Code must contain exactly 6 digits.");

        RuleFor(x => x.NewPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("New password is required.")
            .Length(8, 100)
                .WithMessage(
                    "New password must be between 8 and 100 characters.");

        RuleFor(x => x.ConfirmPassword)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
                .WithMessage("Confirm password is required.")
            .Equal(x => x.NewPassword)
                .WithMessage("Passwords do not match.");
    }
}