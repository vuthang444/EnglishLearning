using CommonLib.Entities;
using CommonLib.DTOs;

namespace CommonLib.Interfaces
{
    public interface ISubmissionRepository
    {
        Task<Submission?> GetByIdAsync(int id);
        Task<Submission?> GetByUserAndLessonAsync(int userId, int lessonId);
        Task<List<Submission>> GetByUserIdAsync(int userId);
        Task<List<Submission>> GetByLessonIdAsync(int lessonId);
        Task<Submission> CreateAsync(Submission submission);
        Task<Submission> UpdateAsync(Submission submission);
        Task<bool> DeleteAsync(int id);
    }
}

