using Microsoft.AspNetCore.Identity;
namespace LibraryManagementSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string? ResetCodeHash { get; set; }
        public DateTime? ResetCodeExpiresAt { get; set; }
        public int ResetCodeAttempts { get; set; }

    }

}