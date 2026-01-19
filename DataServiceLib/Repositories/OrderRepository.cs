using Microsoft.EntityFrameworkCore;
using CommonLib.Entities;
using CommonLib.Interfaces;
using DataServiceLib.Data;

namespace DataServiceLib.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ApplicationDbContext _context;

        public OrderRepository(ApplicationDbContext context) => _context = context;

        public async Task<Order?> GetByIdAsync(int id) =>
            await _context.Orders.Include(o => o.Course).Include(o => o.User).FirstOrDefaultAsync(o => o.Id == id);

        public async Task<Order?> GetByMomoOrderIdAsync(string momoOrderId) =>
            await _context.Orders.Include(o => o.Course).FirstOrDefaultAsync(o => o.MomoOrderId == momoOrderId);

        public async Task<List<Order>> GetByUserIdAsync(int userId) =>
            await _context.Orders.Include(o => o.Course).Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).ToListAsync();

        public async Task<List<Order>> GetPaidByUserAndCourseAsync(int userId, int courseId) =>
            await _context.Orders.Where(o => o.UserId == userId && o.CourseId == courseId && o.Status == "Paid").ToListAsync();

        public async Task<bool> HasActivePremiumAsync(int userId)
        {
            // Kiểm tra user có đơn hàng đã thanh toán trong vòng 30 ngày gần đây
            var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
            return await _context.Orders
                .AnyAsync(o => o.UserId == userId 
                    && o.Status == "Paid" 
                    && o.CreatedAt >= thirtyDaysAgo);
        }

        public async Task<Order> CreateAsync(Order order)
        {
            order.CreatedAt = DateTime.UtcNow;
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order> UpdateAsync(Order order)
        {
            order.UpdatedAt = DateTime.UtcNow;
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();
            return order;
        }
    }
}

