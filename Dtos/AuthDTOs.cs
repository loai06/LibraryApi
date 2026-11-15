using System.ComponentModel.DataAnnotations;

 namespace LibraryManagementSystem.Dtos

{
    public class RegisterDTO
    {
        [Required]
        public string FullName { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 8)]
        public string Password { get; set; }= string.Empty;
    }

    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}