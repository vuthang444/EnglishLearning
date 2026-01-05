using System.Security.Claims;
using BCrypt.Net;
using CommonLib.DTOs;
using CommonLib.Entities;
using CommonLib.Interfaces;

namespace CommonLib.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IJwtService _jwtService;

        public AuthService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IJwtService jwtService)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _jwtService = jwtService;
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto)
        {
            // Kiểm tra user đã tồn tại chưa
            if (await _userRepository.UserExistsAsync(registerDto.Username, registerDto.Email))
            {
                return null;
            }

            // Lấy role User mặc định
            var userRole = await _roleRepository.GetByNameAsync("User");
            if (userRole == null)
            {
                throw new InvalidOperationException("Role 'User' không tồn tại trong hệ thống");
            }

            // Tạo user mới
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                RoleId = userRole.Id,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var createdUser = await _userRepository.CreateAsync(user);

            // Tạo token
            var token = _jwtService.GenerateToken(createdUser);
            var refreshToken = _jwtService.GenerateRefreshToken();

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = new UserDto
                {
                    Id = createdUser.Id,
                    Username = createdUser.Username,
                    Email = createdUser.Email,
                    Role = createdUser.Role?.Name ?? "User"
                }
            };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(loginDto.UsernameOrEmail);
            
            if (user == null || !user.IsActive)
            {
                return null;
            }

            // Kiểm tra mật khẩu
            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                return null;
            }

            // Tạo token
            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            return new AuthResponseDto
            {
                Token = token,
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddHours(1),
                User = new UserDto
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    Role = user.Role?.Name ?? "User"
                }
            };
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            var principal = _jwtService.ValidateToken(token);
            if (principal == null)
            {
                return false;
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return false;
            }

            var user = await _userRepository.GetByIdAsync(userId);
            return user != null && user.IsActive;
        }
    }
}

