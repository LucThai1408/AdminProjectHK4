using Microsoft.EntityFrameworkCore;
using API.Models;

namespace API.Models
{
    public class FlutterContext:DbContext
    {
        public FlutterContext(DbContextOptions options) : base(options) 
        {
            
        }
        public DbSet<AccountExam> AccountExams { get; set; } = default!;
        public DbSet<Account> Accounts { get; set; } = default!;
        public DbSet<Exam> Exams { get; set; } = default!;
        public DbSet<Register> Registers { get; set; } = default!;
        public DbSet<Room> Rooms { get; set; } = default!;
        public DbSet<Subject> Subjects { get; set; } = default!;
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Exam - Account
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Account)
                .WithMany(a => a.Exams)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);

            // Exam - Room
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.Room)
                .WithMany(r => r.Exams)
                .HasForeignKey(e => e.RoomId)
                .OnDelete(DeleteBehavior.Restrict);

            // Exam - Subject
            modelBuilder.Entity<Exam>()
                .HasOne(e => e.CourseSubject)
                .WithMany(s => s.Exams)
                .HasForeignKey(e => e.CourseSubjectId)
                .OnDelete(DeleteBehavior.Restrict);
        }
        public DbSet<API.Models.Course> Course { get; set; } = default!;
        public DbSet<API.Models.CourseSubject> CourseSubject { get; set; } = default!;
    }
}
