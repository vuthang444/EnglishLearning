using CommonLib.Entities;

namespace CommonLib.Interfaces
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task<Order?> GetByMomoOrderIdAsync(string momoOrderId);
        Task<List<Order>> GetByUserIdAsync(int userId);
        Task<List<Order>> GetPaidByUserAndCourseAsync(int userId, int courseId);
        Task<Order> CreateAsync(Order order);
        Task<Order> UpdateAsync(Order order);
    }
}

