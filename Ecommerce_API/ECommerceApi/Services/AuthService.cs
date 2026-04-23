using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using ECommerceApi.Data;
using ECommerceApi.DTOs;
using ECommerceApi.Entities;

namespace ECommerceApi.Services;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<AuthResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request);
    Task<AuthResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request);
    Task<AuthResponse> SocialLoginAsync(string provider, string providerKey, string email, string fullName);
    Task<User?> GetUserByIdAsync(int userId);
}

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly Dictionary<string, (string Token, DateTime Expiry, int UserId)> _resetTokens = new();
    
    public AuthService(AppDbContext context, IConfiguration configuration, IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }
    
    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var userExists = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (userExists != null)
            throw new Exception("Email already exists");
        
        var passwordHash = HashPassword(request.Password);
        
        var user = new User
        {
            Email = request.Email,
            UserName = request.Email,
            FullName = request.FullName,
            PasswordHash = passwordHash,
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        
        await _emailService.SendWelcomeEmailAsync(user);
        
        var token = GenerateJwtToken(user);
        
        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName
        };
    }
    
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
            throw new Exception("Invalid credentials");
        
        if (user.Provider != null)
            throw new Exception("Please login with " + user.Provider);
        
        if (!VerifyPassword(request.Password, user.PasswordHash ?? ""))
            throw new Exception("Invalid credentials");
        
        var token = GenerateJwtToken(user);
        
        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName
        };
    }
    
    public async Task<AuthResponse> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == request.Email);
        if (user == null)
            throw new Exception("User not found");
        
        if (user.Provider != null)
            throw new Exception("Please reset password through " + user.Provider);
        
        var resetToken = Guid.NewGuid().ToString("N");
        _resetTokens[resetToken] = (resetToken, DateTime.UtcNow.AddHours(1), user.Id);
        
        await _emailService.SendPasswordResetAsync(user, resetToken);
        
        return new AuthResponse
        {
            Token = "Reset link sent to email",
            Email = user.Email,
            FullName = user.FullName
        };
    }
    
    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        if (!_resetTokens.TryGetValue(request.Token, out var tokenData))
            throw new Exception("Invalid or expired token");
        
        if (tokenData.Expiry < DateTime.UtcNow)
        {
            _resetTokens.Remove(request.Token);
            throw new Exception("Token expired");
        }
        
        var user = await _context.Users.FindAsync(tokenData.UserId);
        if (user == null)
            throw new Exception("User not found");
        
        user.PasswordHash = HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();
        
        _resetTokens.Remove(request.Token);
        
        return true;
    }
    
    public async Task<AuthResponse> ChangePasswordAsync(int userId, ChangePasswordRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User not found");
        
        if (user.Provider != null)
            throw new Exception("Cannot change password for social login accounts");
        
        if (!VerifyPassword(request.CurrentPassword, user.PasswordHash ?? ""))
            throw new Exception("Current password is incorrect");
        
        user.PasswordHash = HashPassword(request.NewPassword);
        await _context.SaveChangesAsync();
        
        var token = GenerateJwtToken(user);
        
        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName
        };
    }
    
    public async Task<AuthResponse> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            throw new Exception("User not found");
        
        if (request.FullName != null)
            user.FullName = request.FullName;
        
        await _context.SaveChangesAsync();
        
        var token = GenerateJwtToken(user);
        
        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName
        };
    }
    
    public async Task<AuthResponse> SocialLoginAsync(string provider, string providerKey, string email, string fullName)
    {
        var user = _context.Users.FirstOrDefault(u => u.Email == email);
        
        if (user == null)
        {
            user = new User
            {
                Email = email,
                UserName = email,
                FullName = fullName,
                Provider = provider,
                ProviderKey = providerKey,
                CreatedAt = DateTime.UtcNow
            };
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }
        else if (user.Provider != provider)
        {
            throw new Exception("Email already registered. Please login with password.");
        }
        
        var token = GenerateJwtToken(user);
        
        return new AuthResponse
        {
            Token = token,
            Email = user.Email,
            FullName = user.FullName
        };
    }
    
    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _context.Users.FindAsync(userId);
    }
    
    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            
        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);
            
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email ?? ""),
            new Claim(ClaimTypes.Name, user.FullName)
        };
        
        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials
        );
        
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    private static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var salted = password + "ECommerceSalt2024";
        var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(salted));
        return Convert.ToBase64String(hash);
    }
    
    private static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrEmpty(storedHash)) return false;
        var hash = HashPassword(password);
        return hash == storedHash;
    }
}