namespace CommonLib.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public int CourseId { get; set; }
        public Course? Course { get; set; }
        /// <summary>Số tiền VND (để gửi MoMo)</summary>
        public decimal Amount { get; set; }
        /// <summary>Pending, Paid, Failed, Cancelled</summary>
        public string Status { get; set; } = "Pending";
        public string? MomoOrderId { get; set; }
        public string? MomoRequestId { get; set; }
        public string? MomoTransId { get; set; }
        public int? MomoResultCode { get; set; }
        public string? MomoMessage { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

