using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("Subjects")]
    public class Subject
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int SubjectId { get; set; }
        [Required]
        [StringLength(255)]
        public string? Name { get; set; }
        public int MaxScore { get; set; }
        public float PassScore { get; set; }
        public bool Status { get; set; } = true;
        public ICollection<CourseSubject> CourseSubjects { get; set; } = new HashSet<CourseSubject>();
    }
}
