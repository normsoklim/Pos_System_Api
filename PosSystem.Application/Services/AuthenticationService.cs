using Microsoft.Extensions.Configuration;
using PosSystem.Application.DTOs.Auth;
using PosSystem.Application.Interfaces.Repositories;
using PosSystem.Application.Interfaces.Services;
using PosSystem.Domain.Entities;
using PosSystem.Application.Security;
using BCrypt.Net;

namespace PosSystem.Application.Services
{
    public class AuthenticationService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly ISecurityAuditService _securityAuditService;
        private readonly IConfiguration _configuration;
        private readonly int _refreshTokenExpiryInDays;

        public AuthenticationService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IRefreshTokenRepository refreshTokenRepository,
            IJwtTokenService jwtTokenService,
            ISecurityAuditService securityAuditService,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _jwtTokenService = jwtTokenService;
            _securityAuditService = securityAuditService;
            _configuration = configuration;
            _refreshTokenExpiryInDays = int.Parse(_configuration["Jwt:RefreshTokenExpireDays"] ?? "7");
        }

        public async Task<TokenResponseDto> LoginAsync(LoginDto loginDto)
        {
            if (loginDto == null)
                throw new ArgumentNullException(nameof(loginDto));

            if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                throw new ArgumentException("Email and password are required");

            var user = await _userRepository.GetByEmailAsync(loginDto.Email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }

            if (!user.IsActive)
            {
                throw new UnauthorizedAccessException("Account is deactivated");
            }

            var token = await _jwtTokenService.GenerateToken(user);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryInDays),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            return new TokenResponseDto
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60"))
            };
        }

        public async Task<TokenResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            if (registerDto == null)
                throw new ArgumentNullException(nameof(registerDto));

            if (string.IsNullOrWhiteSpace(registerDto.Email) || string.IsNullOrWhiteSpace(registerDto.Password))
                throw new ArgumentException("Email and password are required");

            if (await _userRepository.ExistsAsync(registerDto.Email))
            {
                throw new ArgumentException("Email already exists");
            }

            if (registerDto.Password.Length < 6)
            {
                throw new ArgumentException("Password must be at least 6 characters long");
            }

            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                ApiKey = GenerateApiKey(),
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                LastLoginAt = null
            };

            var createdUser = await _userRepository.CreateAsync(user);

            // Assign default role (e.g., "User")
            var defaultRole = await _roleRepository.GetByNameAsync("User");
            if (defaultRole == null)
            {
                defaultRole = await _roleRepository.CreateAsync(new Role
                {
                    Name = "User",
                    Description = "Default user role"
                });
            }

            // Create UserRole relationship and update the user
            var userRole = new UserRole
            {
                UserId = createdUser.Id,
                RoleId = defaultRole.Id,
                AssignedAt = DateTime.UtcNow
            };

            // Add the UserRole to the user's collection and update
            createdUser.UserRoles.Add(userRole);
            await _userRepository.UpdateAsync(createdUser);

            var token = await _jwtTokenService.GenerateToken(createdUser);
            var refreshToken = GenerateRefreshToken();

            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = createdUser.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryInDays),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _refreshTokenRepository.CreateAsync(refreshTokenEntity);

            return new TokenResponseDto
            {
                AccessToken = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60"))
            };
        }

        public async Task<TokenResponseDto> RefreshTokenAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                throw new ArgumentException("Refresh token is required");

            var tokenEntity = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (tokenEntity == null || tokenEntity.ExpiresAt < DateTime.UtcNow || tokenEntity.IsRevoked)
            {
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            var user = await _userRepository.GetByIdAsync(tokenEntity.UserId);
            if (user == null || !user.IsActive)
            {
                throw new UnauthorizedAccessException("User account is invalid or deactivated");
            }

            // Revoke old refresh token
            await _refreshTokenRepository.RevokeAsync(refreshToken);

            // Generate new tokens
            var newToken = await _jwtTokenService.GenerateToken(user);
            var newRefreshToken = GenerateRefreshToken();

            var newRefreshTokenEntity = new RefreshToken
            {
                Token = newRefreshToken,
                UserId = user.Id,
                ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpiryInDays),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            await _refreshTokenRepository.CreateAsync(newRefreshTokenEntity);

            return new TokenResponseDto
            {
                AccessToken = newToken,
                RefreshToken = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60"))
            };
        }

        public async Task<bool> LogoutAsync(string refreshToken)
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            return await _refreshTokenRepository.RevokeAsync(refreshToken);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            if (userId <= 0)
                throw new ArgumentException("Invalid user ID");

            if (string.IsNullOrWhiteSpace(currentPassword) || string.IsNullOrWhiteSpace(newPassword))
                throw new ArgumentException("Current password and new password are required");

            if (newPassword.Length < 6)
                throw new ArgumentException("New password must be at least 6 characters long");

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
            await _userRepository.UpdateAsync(user);
            return true;
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }

        private string GenerateApiKey()
        {
            return "API-" + Guid.NewGuid().ToString("N")[..16].ToUpper();
        }
    }
}