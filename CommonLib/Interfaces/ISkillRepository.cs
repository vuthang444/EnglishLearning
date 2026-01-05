using CommonLib.Entities;

namespace CommonLib.Interfaces
{
    public interface ISkillRepository
    {
        Task<List<Skill>> GetAllAsync();
        Task<Skill?> GetByIdAsync(int id);
        Task<Skill?> GetByNameAsync(string name);
        Task<Skill> CreateAsync(Skill skill);
        Task<Skill> UpdateAsync(Skill skill);
        Task<bool> DeleteAsync(int id);
    }
}

