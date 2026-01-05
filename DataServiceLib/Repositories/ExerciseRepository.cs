using Microsoft.EntityFrameworkCore;
using CommonLib.Entities;
using CommonLib.Interfaces;
using DataServiceLib.Data;

namespace DataServiceLib.Repositories
{
    public class ExerciseRepository : IExerciseRepository
    {
        private readonly ApplicationDbContext _context;

        public ExerciseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Exercise>> GetByLessonIdAsync(int lessonId)
        {
            return await _context.Exercises
                .Include(e => e.Answers)
                .Where(e => e.LessonId == lessonId)
                .OrderBy(e => e.Order)
                .ToListAsync();
        }

        public async Task<Exercise?> GetByIdAsync(int id)
        {
            return await _context.Exercises
                .Include(e => e.Answers)
                .Include(e => e.Lesson)
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<Exercise> CreateAsync(Exercise exercise)
        {
            exercise.CreatedAt = DateTime.UtcNow;
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(exercise.Id) ?? exercise;
        }

        public async Task<Exercise> UpdateAsync(Exercise exercise)
        {
            exercise.UpdatedAt = DateTime.UtcNow;
            
            // Lấy exercise hiện tại từ database
            var existingExercise = await _context.Exercises
                .Include(e => e.Answers)
                .FirstOrDefaultAsync(e => e.Id == exercise.Id);
            
            if (existingExercise != null)
            {
                // Xóa các answers cũ
                _context.Answers.RemoveRange(existingExercise.Answers);
                
                // Cập nhật thông tin exercise
                existingExercise.Question = exercise.Question;
                existingExercise.Content = exercise.Content;
                existingExercise.Type = exercise.Type;
                existingExercise.Order = exercise.Order;
                existingExercise.IsActive = exercise.IsActive;
                existingExercise.UpdatedAt = exercise.UpdatedAt;
                
                // Thêm các answers mới
                if (exercise.Answers != null && exercise.Answers.Any())
                {
                    foreach (var answer in exercise.Answers)
                    {
                        existingExercise.Answers.Add(new Answer
                        {
                            Text = answer.Text,
                            IsCorrect = answer.IsCorrect,
                            Order = answer.Order,
                            ExerciseId = existingExercise.Id
                        });
                    }
                }
                
                await _context.SaveChangesAsync();
                return await GetByIdAsync(exercise.Id) ?? existingExercise;
            }
            
            _context.Exercises.Update(exercise);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(exercise.Id) ?? exercise;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var exercise = await _context.Exercises.FindAsync(id);
            if (exercise == null) return false;

            _context.Exercises.Remove(exercise);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

