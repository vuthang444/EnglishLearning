using CommonLib.Entities;

namespace CommonLib.Interfaces
{
    public interface ILessonRepository
    {
        Task<List<Lesson>> GetAllAsync();
        Task<List<Lesson>> GetBySkillIdAsync(int skillId);
        Task<Lesson?> GetByIdAsync(int id);
        Task<Lesson> CreateAsync(Lesson lesson);
        Task<Lesson> UpdateAsync(Lesson lesson);
        Task<bool> DeleteAsync(int id);
    }
}

