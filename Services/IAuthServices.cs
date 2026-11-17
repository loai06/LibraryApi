using LibraryManagementSystem.Dtos;
using Microsoft.AspNetCore.Identity;
namespace LibraryManagementSystem.Services
{
    public interface IAuthServices
    {
        Task<IdentityResult> RegisterAsync(RegisterDTO registerDTO);
        Task<LoginResponseDto?> LoginAsync(LoginDTO loginDTO);
        Task ForgotPasswordAsync(ForgotPasswordDto dto);
        Task<IdentityResult> ResetPasswordAsync(ResetPasswordDto dto);
    }
}