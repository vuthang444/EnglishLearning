using CommonLib.Entities;

namespace CommonLib.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task<User> CreateAsync(User user);
        Task<User?> GetByIdAsync(int id);
        Task<bool> UserExistsAsync(string username, string email);
    }
}

