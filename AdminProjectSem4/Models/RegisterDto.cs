namespace API.Models
{
    public class RegisterDto
    {
        public int RegisterId { get; set; }
        public int StudentId { get; set; }
        public string? StudentName { get; set; }
        public int ExamId { get; set; }
        public int SubjectId { get; set; }
        public string? Email { get; set; }
        public bool Status { get; set; }
        public string? Payment { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? ExamName { get; set; }
        public float ExamFee { get; set; }
        public string? RoomName { get; set; } 

    }

}