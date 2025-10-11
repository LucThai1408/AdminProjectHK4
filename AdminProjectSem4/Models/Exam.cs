using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("Exams")]
    public class Exam
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ExamId { get; set; }
        public string Name { get; set; }
        [Required]
        public int RoomId { get; set; }
        [Required]
        public int AccountId { get; set; }
        [Required]
        public DateTime ExamDay { get; set; }
        [Required] 
        public TimeSpan ExamTime { get; set; }
        public bool Status { get; set; } = true;
        public float Fee { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public ICollection<AccountExam> AccountExams { get; set; } = new HashSet<AccountExam>();
        [ForeignKey("AccountId")]
        public Account? Account { get; set; }
        [ForeignKey("CourseSubjectId")]
        public CourseSubject? CourseSubject { get; set; }
        [ForeignKey("RoomId")]
        public Room? Room { get; set; }
        public ICollection<Register> Registers { get; set; } = new HashSet<Register>();
    }
}
