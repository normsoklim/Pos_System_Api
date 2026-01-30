using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PosSystem.Application.DTOs.Auth;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Application.Interfaces.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PosSystem.Domain.Entities;
using BCrypt.Net;

namespace PosSystem.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IConfiguration _configuration;
    private readonly string _jwtSecret;
    private readonly int _jwtExpiryInMinutes;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _configuration = configuration;
        _jwtSecret = _configuration["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret not found in configuration");
        _jwtExpiryInMinutes = int.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60");
    }

    public async Task<TokenResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
        if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid username or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("User account is deactivated");
        }

        var roles = await _userRepository.GetUserRolesAsync(user.Id);
        var roleNames = roles.Select(r => r.Name).ToList();

        var token = GenerateJwtToken(user, roleNames);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // Refresh token expires in 7 days
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        return new TokenResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtExpiryInMinutes),
            Username = user.Username,
            Email = user.Email,
            Roles = roleNames
        };
    }

    public async Task<TokenResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        // Check if user already exists
        var existingUser = await _userRepository.GetByUsernameAsync(registerDto.Username);
        if (existingUser != null)
        {
            throw new ArgumentException("Username already exists");
        }

        existingUser = await _userRepository.GetByEmailAsync(registerDto.Email);
        if (existingUser != null)
        {
            throw new ArgumentException("Email already exists");
        }

        // Create new user
        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
            FirstName = registerDto.FirstName,
            LastName = registerDto.LastName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createdUser = await _userRepository.CreateAsync(user);

        // Assign default role (e.g., "User")
        var defaultRole = await _roleRepository.GetByNameAsync("User");
        if (defaultRole != null)
        {
            await _userRepository.AssignRoleToUserAsync(createdUser.Id, defaultRole.Id);
        }

        // Generate tokens
        var roles = await _userRepository.GetUserRolesAsync(createdUser.Id);
        var roleNames = roles.Select(r => r.Name).ToList();

        var token = GenerateJwtToken(createdUser, roleNames);
        var refreshToken = GenerateRefreshToken();
        var refreshTokenEntity = new RefreshToken
        {
            Token = refreshToken,
            UserId = createdUser.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // Refresh token expires in 7 days
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

        return new TokenResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtExpiryInMinutes),
            Username = createdUser.Username,
            Email = createdUser.Email,
            Roles = roleNames
        };
    }

    public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            throw new ArgumentException("Refresh token is required");
        }

        // Get the refresh token from the database
        var tokenEntity = await _refreshTokenRepository.GetValidTokenAsync(refreshToken);
        if (tokenEntity == null)
        {
            throw new UnauthorizedAccessException("Invalid refresh token");
        }

        // Revoke the old refresh token
        await _refreshTokenRepository.RevokeTokenAsync(refreshToken, ipAddress);

        // Get the user associated with the refresh token
        var user = await _userRepository.GetByIdAsync(tokenEntity.UserId);
        if (user == null)
        {
            throw new UnauthorizedAccessException("User not found");
        }

        // Get user roles
        var roles = await _userRepository.GetUserRolesAsync(user.Id);
        var roleNames = roles.Select(r => r.Name).ToList();

        // Generate new tokens
        var newToken = GenerateJwtToken(user, roleNames);
        var newRefreshToken = GenerateRefreshToken();
        var newRefreshTokenEntity = new RefreshToken
        {
            Token = newRefreshToken,
            UserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddDays(7), // Refresh token expires in 7 days
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.CreateAsync(newRefreshTokenEntity);

        return new TokenResponseDto
        {
            Token = newToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_jwtExpiryInMinutes),
            Username = user.Username,
            Email = user.Email,
            Roles = roleNames
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        return await _userRepository.UpdateAsync(user);
    }

    public async Task<bool> AssignRoleToUserAsync(int userId, int roleId)
    {
        return await _userRepository.AssignRoleToUserAsync(userId, roleId);
    }

    public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
    {
        return await _userRepository.RemoveRoleFromUserAsync(userId, roleId);
    }

    public async Task<List<string>> GetUserRolesAsync(int userId)
    {
        var roles = await _userRepository.GetUserRolesAsync(userId);
        return roles.Select(r => r.Name).ToList();
    }

    public async Task<bool> UserHasRoleAsync(int userId, string roleName)
    {
        return await _userRepository.UserHasRoleAsync(userId, roleName);
    }

    private string GenerateJwtToken(User user, List<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_jwtSecret);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtExpiryInMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
    }
}