using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("CourseSubjects")]
    public class CourseSubject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseSubjectId { get; set; }
        [Required]
        public int SubjectId { get; set; }
        [Required]
        public int CourseId { get; set; }
        public int Sem {  get; set; }
        public bool Status { get; set; } = true;
        [ForeignKey("SubjectId")]
        public Subject? Subject { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }
        public ICollection<Exam> Exams { get; set; } = new HashSet<Exam>();
        public ICollection<Register> Registers { get; set; } = new HashSet<Register>();
    }
}
