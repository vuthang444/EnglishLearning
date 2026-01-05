using Microsoft.EntityFrameworkCore;
using CommonLib.Entities;
using CommonLib.Interfaces;
using DataServiceLib.Data;

namespace DataServiceLib.Repositories
{
    public class SubmissionRepository : ISubmissionRepository
    {
        private readonly ApplicationDbContext _context;

        public SubmissionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Submission?> GetByIdAsync(int id)
        {
            return await _context.Submissions
                .Include(s => s.User)
                .Include(s => s.Lesson)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<Submission?> GetByUserAndLessonAsync(int userId, int lessonId)
        {
            return await _context.Submissions
                .Include(s => s.User)
                .Include(s => s.Lesson)
                .Where(s => s.UserId == userId && s.LessonId == lessonId)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Submission>> GetByUserIdAsync(int userId)
        {
            return await _context.Submissions
                .Include(s => s.Lesson)
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Submission>> GetByLessonIdAsync(int lessonId)
        {
            return await _context.Submissions
                .Include(s => s.User)
                .Where(s => s.LessonId == lessonId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<Submission> CreateAsync(Submission submission)
        {
            _context.Submissions.Add(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        public async Task<Submission> UpdateAsync(Submission submission)
        {
            _context.Submissions.Update(submission);
            await _context.SaveChangesAsync();
            return submission;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var submission = await _context.Submissions.FindAsync(id);
            if (submission == null)
                return false;

            _context.Submissions.Remove(submission);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

