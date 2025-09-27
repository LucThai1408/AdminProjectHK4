using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("AccountExams")]
    public class AccountExam
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int AccountExamId { get; set; }
        public int SubjectId { get; set; }
        [Required]
        public int ExamId { get; set; }
        [Required]
        public int StudentId { get; set; }
        public float Score { get; set; }
        public int Record {  get; set; }

        public bool IsPass { get; set; }
        public bool Status { get; set; } = true;
        [ForeignKey("ExamId")]
        public Exam? Exam { get; set; }
        [ForeignKey("StudentId")]
        public Account? Student { get; set; }
    }
}
