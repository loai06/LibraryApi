using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using LibraryManagementSystem.Dtos;
namespace LibraryManagementSystem.Services;
using LibraryManagementSystem.Settings;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
public class AuthServices : IAuthServices
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly JwtSettings _jwtSettings;
  public AuthServices(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtSettings)
  {
    _userManager = userManager;
    _jwtSettings = jwtSettings.Value;
  }

  public async Task<IdentityResult> RegisterAsync(RegisterDTO registerDTO)
  {
    var user = new ApplicationUser
    {
      UserName = registerDTO.Email.Trim(),
      Email = registerDTO.Email.Trim(),
      FullName = registerDTO.FullName.Trim()
    };

   return await _userManager.CreateAsync(user, registerDTO.Password);
  }

  public async Task<LoginResponseDto?> LoginAsync(LoginDTO loginDTO)
  {
    var user = await _userManager.FindByEmailAsync(loginDTO.Email.Trim());
    if (user == null)
    {
      return null;
    }

    var result = await _userManager.CheckPasswordAsync(user, loginDTO.Password);
    if (!result)
    {
      return null;
    }

    var claims = new List<Claim>
    {
      new Claim(JwtRegisteredClaimNames.Sub, user.Id),
      new Claim(JwtRegisteredClaimNames.Email, user.Email),
      new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    var key = new SymmetricSecurityKey(
      Convert.FromBase64String(_jwtSettings.SecretKey));

    var credentials = new SigningCredentials(
      key, SecurityAlgorithms.HmacSha256);

    var expiresAtUtc = DateTime.UtcNow
      .AddMinutes(_jwtSettings.ExpiryInMinutes);

    var token = new JwtSecurityToken(
      issuer: _jwtSettings.Issuer,
      audience: _jwtSettings.Audience,
      claims: claims,
      expires: expiresAtUtc,
      signingCredentials: credentials);

    return new LoginResponseDto
    {
      Token = new JwtSecurityTokenHandler().WriteToken(token),
      UserId = user.Id,
      Email = user.Email,
      FullName = user.FullName,
      ExpiresAtUtc = expiresAtUtc
    };
  }
}