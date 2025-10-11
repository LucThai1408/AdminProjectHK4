using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("CourseStudents")]
    public class CourseStudent
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseStudentId { get; set; }
        [Required]
        public int StudentId { get; set; }
        [Required]
        public int CourseId { get; set; }
        public bool Status { get; set; } = true;
        [ForeignKey("StudentId")]
        public Account? Student { get; set; }
        [ForeignKey("CourseId")]
        public Course? Course { get; set; }
    }
}
