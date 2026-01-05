using Microsoft.EntityFrameworkCore;
using CommonLib.Entities;
using CommonLib.Interfaces;
using DataServiceLib.Data;

namespace DataServiceLib.Repositories
{
    public class LessonRepository : ILessonRepository
    {
        private readonly ApplicationDbContext _context;

        public LessonRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Lesson>> GetAllAsync()
        {
            return await _context.Lessons
                .Include(l => l.Skill)
                .OrderBy(l => l.SkillId)
                .ThenBy(l => l.Order)
                .ToListAsync();
        }

        public async Task<List<Lesson>> GetBySkillIdAsync(int skillId)
        {
            return await _context.Lessons
                .Include(l => l.Skill)
                .Where(l => l.SkillId == skillId)
                .OrderBy(l => l.Order)
                .ToListAsync();
        }

        public async Task<Lesson?> GetByIdAsync(int id)
        {
            return await _context.Lessons
                .Include(l => l.Skill)
                .Include(l => l.Exercises)
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Lesson> CreateAsync(Lesson lesson)
        {
            lesson.CreatedAt = DateTime.UtcNow;
            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(lesson.Id) ?? lesson;
        }

        public async Task<Lesson> UpdateAsync(Lesson lesson)
        {
            lesson.UpdatedAt = DateTime.UtcNow;
            _context.Lessons.Update(lesson);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(lesson.Id) ?? lesson;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null) return false;

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

