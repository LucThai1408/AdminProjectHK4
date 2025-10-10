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
        public int AccountId { get; set; }
        [Required]
        public int ExamId { get; set; }
        [Required]
        public string? Email { get; set; }
        public bool Status { get; set; } = true;
        public string payment {  get; set; }
        public DateTime CreatedAt { get; set; }
        [ForeignKey("AccountId")]
        public Account? Account { get; set; }
        [ForeignKey("ExamId")]
        public Exam? Exam { get; set; }

    }
}
