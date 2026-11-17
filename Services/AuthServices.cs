using LibraryManagementSystem.Models;
using Microsoft.AspNetCore.Identity;
using LibraryManagementSystem.Dtos;
namespace LibraryManagementSystem.Services;

using LibraryManagementSystem.Settings;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Globalization;

public class AuthServices : IAuthServices
{
  private readonly UserManager<ApplicationUser> _userManager;
  private readonly JwtSettings _jwtSettings;
  private readonly IEmailService _emailService;
  public AuthServices(UserManager<ApplicationUser> userManager, IOptions<JwtSettings> jwtSettings, IEmailService emailService)
  {
    _userManager = userManager;
    _jwtSettings = jwtSettings.Value;
    _emailService = emailService;
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

  public async Task ForgotPasswordAsync(ForgotPasswordDto dto)
  {
    var user = await _userManager.FindByEmailAsync(dto.Email.Trim());

    if (user == null)
      return;

    var code = RandomNumberGenerator.GetInt32(1_000_000)
        .ToString("D6", CultureInfo.InvariantCulture);

    user.ResetCodeHash =
        _userManager.PasswordHasher.HashPassword(user, code);

    user.ResetCodeExpiresAt = DateTime.UtcNow.AddMinutes(10);
    user.ResetCodeAttempts = 0;

    var result = await _userManager.UpdateAsync(user);

    if (!result.Succeeded)
      throw new InvalidOperationException("Could not save reset code.");

    await _emailService.SendEmailAsync(
        user.Email!,
        "Reset your Library API password",
        $"""
        <p>Your password reset code is:</p>
        <h2>{code}</h2>
        <p>This code expires in 10 minutes.</p>
        <p>If you did not request this, ignore this email.</p>
        """);
  }


  public async Task<IdentityResult> ResetPasswordAsync(
     ResetPasswordDto dto)
  {
    var user = await _userManager.FindByEmailAsync(dto.Email.Trim());

    if (user == null ||
        string.IsNullOrEmpty(user.ResetCodeHash) ||
        user.ResetCodeExpiresAt == null ||
        user.ResetCodeExpiresAt <= DateTime.UtcNow ||
        user.ResetCodeAttempts >= 5)
    {
      return InvalidResetCode();
    }

    var verification = _userManager.PasswordHasher.VerifyHashedPassword(
        user, user.ResetCodeHash, dto.Code);


    user.ResetCodeAttempts++;

    var attemptResult = await _userManager.UpdateAsync(user);

    if (!attemptResult.Succeeded)
      return InvalidResetCode();

    if (verification == PasswordVerificationResult.Failed)
      return InvalidResetCode();

    var token = await _userManager.GeneratePasswordResetTokenAsync(user);

    user.ResetCodeHash = null;
    user.ResetCodeExpiresAt = null;
    user.ResetCodeAttempts = 0;

    return await _userManager.ResetPasswordAsync(
        user, token, dto.NewPassword);
  }

  private static IdentityResult InvalidResetCode()
  {
    return IdentityResult.Failed(new IdentityError
    {
      Code = "InvalidResetCode",
      Description = "Invalid or expired reset code. Request a new code."
    });
  }
}