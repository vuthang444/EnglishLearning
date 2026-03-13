namespace EnglishLearning.Models
{
    public class AdminDashboardVm
    {
        public int CourseCount { get; set; }
        public int NewsCount { get; set; }
        public int UserCount { get; set; }
        public int PremiumUserCount { get; set; }
        public int SubmissionCount { get; set; }
        public double AverageScore { get; set; }
        public List<SkillStatItem> SkillStats { get; set; } = new();
    }

    public class SkillStatItem
    {
        public int SkillId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int LessonCount { get; set; }
        public string AdminUrl { get; set; } = string.Empty;
        public string IconClass { get; set; } = string.Empty;
    }
}
