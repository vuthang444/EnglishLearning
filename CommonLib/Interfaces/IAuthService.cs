using CommonLib.DTOs;

namespace CommonLib.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
        Task<bool> ValidateTokenAsync(string token);
    }
}

