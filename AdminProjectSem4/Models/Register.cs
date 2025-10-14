using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("Registers")]
    public class Register
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RegisterId { get; set; }
        [Required]
        public int StudentId { get; set; }
        [Required]
        public int ExamId { get; set; }
        [Required]
        public int CourseSubjectId { get; set; }
        [Required]
        public string? Email { get; set; }
        public bool Status { get; set; } = true;
        public string payment {  get; set; }
        public DateTime CreatedAt { get; set; }
        [ForeignKey("StudentId")]
        public Account? Student { get; set; }
        [ForeignKey("ExamId")]
        public Exam? Exam { get; set; }
        [ForeignKey("CourseSubjectId")]
        public CourseSubject? CourseSubject { get; set; }

    }
}
