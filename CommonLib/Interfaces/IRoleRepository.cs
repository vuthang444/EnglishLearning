using CommonLib.Entities;

namespace CommonLib.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(string name);
        Task<Role?> GetByIdAsync(int id);
        Task<List<Role>> GetAllAsync();
    }
}

