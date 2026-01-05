using CommonLib.Entities;

namespace CommonLib.Interfaces
{
    public interface IExerciseRepository
    {
        Task<List<Exercise>> GetByLessonIdAsync(int lessonId);
        Task<Exercise?> GetByIdAsync(int id);
        Task<Exercise> CreateAsync(Exercise exercise);
        Task<Exercise> UpdateAsync(Exercise exercise);
        Task<bool> DeleteAsync(int id);
    }
}

