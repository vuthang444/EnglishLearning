using Microsoft.EntityFrameworkCore;
using CommonLib.Entities;

namespace DataServiceLib.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Lesson> Lessons { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<Answer> Answers { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Order> Orders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình User
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.HasOne(e => e.Role)
                      .WithMany(r => r.Users)
                      .HasForeignKey(e => e.RoleId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình Role
            modelBuilder.Entity<Role>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            });

            // Cấu hình Skill
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Name).IsUnique();
                entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            });

            // Cấu hình Lesson
            modelBuilder.Entity<Lesson>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.ReadingContent).HasMaxLength(10000);
                entity.Property(e => e.ReadingLevel).HasMaxLength(10);
                entity.Property(e => e.AudioUrl).HasMaxLength(1000);
                entity.Property(e => e.Transcript).HasMaxLength(20000);
                entity.Property(e => e.Topic).HasMaxLength(500);
                entity.Property(e => e.ReferenceText).HasMaxLength(10000);
                entity.Property(e => e.SpeakingLevel).HasMaxLength(10);
                // Writing fields
                entity.Property(e => e.WritingTopic).HasMaxLength(500);
                entity.Property(e => e.WritingPrompt).HasMaxLength(5000);
                entity.Property(e => e.WritingHints).HasMaxLength(5000);
                entity.Property(e => e.WritingLevel).HasMaxLength(10);
                entity.HasOne(e => e.Skill)
                      .WithMany(s => s.Lessons)
                      .HasForeignKey(e => e.SkillId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình Exercise
            modelBuilder.Entity<Exercise>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Question).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Timestamp).HasMaxLength(20);
                entity.HasOne(e => e.Lesson)
                      .WithMany(l => l.Exercises)
                      .HasForeignKey(e => e.LessonId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình Answer
            modelBuilder.Entity<Answer>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Text).IsRequired().HasMaxLength(1000);
                entity.HasOne(e => e.Exercise)
                      .WithMany(ex => ex.Answers)
                      .HasForeignKey(e => e.ExerciseId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình Course
            modelBuilder.Entity<Course>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(300);
                entity.Property(e => e.Topic).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Level).HasMaxLength(20);
                entity.Property(e => e.Syllabus).HasMaxLength(10000);
                entity.Property(e => e.TargetAudience).HasMaxLength(2000);
                entity.Property(e => e.MarketingCopy).HasMaxLength(5000);
                entity.Property(e => e.PriceUSD).HasPrecision(10, 2);
            });

            // Cấu hình Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).HasMaxLength(20);
                entity.Property(e => e.MomoOrderId).HasMaxLength(50);
                entity.Property(e => e.MomoRequestId).HasMaxLength(50);
                entity.Property(e => e.MomoTransId).HasMaxLength(50);
                entity.Property(e => e.MomoMessage).HasMaxLength(500);
                entity.Property(e => e.Amount).HasPrecision(18, 0);
                entity.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Course).WithMany().HasForeignKey(e => e.CourseId).OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình Submission
            modelBuilder.Entity<Submission>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AnswersJson).IsRequired().HasMaxLength(5000);
                entity.HasOne(e => e.User)
                      .WithMany()
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(e => e.Lesson)
                      .WithMany()
                      .HasForeignKey(e => e.LessonId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data cho Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin", Description = "Quản trị viên" },
                new Role { Id = 2, Name = "User", Description = "Người dùng" }
            );

            // Seed data cho Skills
            modelBuilder.Entity<Skill>().HasData(
                new Skill { Id = 1, Name = "Listening", Description = "Kỹ năng nghe", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Skill { Id = 2, Name = "Speaking", Description = "Kỹ năng nói", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Skill { Id = 3, Name = "Reading", Description = "Kỹ năng đọc", CreatedAt = DateTime.UtcNow, IsActive = true },
                new Skill { Id = 4, Name = "Writing", Description = "Kỹ năng viết", CreatedAt = DateTime.UtcNow, IsActive = true }
            );
        }
    }
}

