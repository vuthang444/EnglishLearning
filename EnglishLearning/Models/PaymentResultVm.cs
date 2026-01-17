using CommonLib.Entities;

namespace EnglishLearning.Models
{
    public class PaymentResultVm
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public Order? Order { get; set; }
    }
}

