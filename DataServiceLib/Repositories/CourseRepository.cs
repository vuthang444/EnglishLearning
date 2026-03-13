using Microsoft.EntityFrameworkCore;
using CommonLib.Entities;
using CommonLib.Interfaces;
using DataServiceLib.Data;

namespace DataServiceLib.Repositories
{
    public class CourseRepository : ICourseRepository
    {
        private readonly ApplicationDbContext _context;

        public CourseRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Course>> GetAllAsync()
        {
            return await _context.Courses.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<List<Course>> GetActiveAsync()
        {
            return await _context.Courses.Where(c => c.IsActive).OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        public async Task<Course?> GetByIdAsync(int id)
        {
            return await _context.Courses.FindAsync(id);
        }

        public async Task<Course> CreateAsync(Course course)
        {
            course.CreatedAt = DateTime.UtcNow;
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task<Course> UpdateAsync(Course course)
        {
            course.UpdatedAt = DateTime.UtcNow;
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();
            return course;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var c = await _context.Courses.FindAsync(id);
            if (c == null) return false;

            // Kiểm tra xem có Order nào đang sử dụng Course này không
            var hasOrders = await _context.Orders.AnyAsync(o => o.CourseId == id);
            if (hasOrders)
            {
                throw new InvalidOperationException("Không thể xóa khóa học vì đã có đơn hàng liên quan. Vui lòng vô hiệu hóa khóa học thay vì xóa.");
            }

            _context.Courses.Remove(c);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

