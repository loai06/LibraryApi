using System.ComponentModel.DataAnnotations;

namespace LibraryManagementSystem.Dtos

{
    public class RegisterDTO
    {
        public string FullName { get; set; }=string.Empty;
        public string Email { get; set; }=string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginDTO
    {
        public string Email { get; set; }=string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ForgotPasswordDto
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordDto
    {
        public string Email { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}