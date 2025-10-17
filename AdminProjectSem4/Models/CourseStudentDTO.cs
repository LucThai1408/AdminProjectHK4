namespace AdminProjectSem4.Models
{
    public class CourseStudentDTO
    {
        public int CourseStudentId { get; set; }
        public int StudentId { get; set; }
        public int CourseId { get; set; }
        public bool Status { get; set; } = true;
        public string StudentName { get; set; }
        public string CourseName { get; set; }
    }
}
