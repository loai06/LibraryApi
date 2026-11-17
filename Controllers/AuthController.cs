using Microsoft.AspNetCore.Mvc;
using LibraryManagementSystem.Services;
using LibraryManagementSystem.Dtos;
namespace LibraryManagementSystem.Controllers;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthServices _authServices;

    public AuthController(IAuthServices authServices)
    {
        _authServices = authServices;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDTO registerDTO)
    {
        var result = await _authServices.RegisterAsync(registerDTO);
        if (!result.Succeeded)
        {
            return BadRequest(result.Errors);
        }
        return Ok(new { Message = "User registered successfully." });
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
    {
        var response = await _authServices.LoginAsync(loginDTO);
        if (response == null)
        {
            return Unauthorized(new { Message = "Invalid email or password." });
        }
        return Ok(response);
    }
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
    [FromBody] ForgotPasswordDto dto)
    {
        await _authServices.ForgotPasswordAsync(dto);

        return Ok(new
        {
            Message = "If an account exists, a password reset email has been sent."
        });
        
    }
    [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _authServices.ResetPasswordAsync(dto);

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            return Ok(new
            {
                Message = "Password reset successfully."
            });
        }
}