using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("Accounts")]
    public class Account
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountId { get; set; }
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }
        [StringLength(255)]
        public string? FullName { get; set; }
        [Required]
        public int RoomId { get; set; }
        [Required]
        [StringLength(100)]
        public string? Password { get; set; }
        [Required]
        [StringLength(200)]
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public int Role { get; set; } = 0;
        public string? Image { get; set; }
        public bool Status {  get; set; } = true;

        [ForeignKey("RoomId")]
        public Room? Room { get; set; }
        public ICollection<AccountExam> AccountExams { get; set; } = new HashSet<AccountExam>();
        public ICollection<Exam> Exams { get; set; } = new HashSet<Exam>();
        public ICollection<Register> Registers { get; set; } = new HashSet<Register>();
        public ICollection<CourseStudent> CourseStudents { get; set; } = new HashSet<CourseStudent>();
    }
}
