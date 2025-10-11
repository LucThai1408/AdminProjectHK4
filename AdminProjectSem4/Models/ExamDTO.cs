namespace API.Models
{
    public class ExamDto
    {
        public int ExamId { get; set; }
        public string? Name { get; set; }

        public int AccountId { get; set; }
        public string? AccountName { get; set; }

        public int RoomId { get; set; }
        public string? RoomName { get; set; }

        public DateTime ExamDay { get; set; }
        public TimeSpan ExamTime { get; set; }
        public bool Status { get; set; }
        public float Fee { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
