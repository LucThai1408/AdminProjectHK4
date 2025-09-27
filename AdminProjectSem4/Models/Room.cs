using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Models
{
    [Table("Rooms")]
    public class Room
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoomId { get; set; }
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }
        public bool Status { get; set; } = true;
        public ICollection<Account> Accounts { get; set; } = new HashSet<Account>();
        public ICollection<Exam> Exams { get; set; } = new HashSet<Exam>();
    }
}
