using CommonLib.Entities;

namespace EnglishLearning.Models
{
    public class UserProfileViewModel
    {
        public User User { get; set; } = null!;
        public IReadOnlyList<Order> PaidOrders { get; set; } = Array.Empty<Order>();
    }
}

