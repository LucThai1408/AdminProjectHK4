using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace API.Models
{
    [Table("Courses")]
    public class Course
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CourseId { get; set; }
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }
        public bool Status { get; set; } = true;
        public ICollection<CourseSubject> CourseSubject {  get; set; } = new HashSet<CourseSubject>();
    }
}
