namespace AdminProjectSem4.Models
{
    public class CourseSubjectDTO
    {
        public int CourseSubjectId { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
        public int CourseId { get; set; }
        public string CourseName { get; set; }
        public int Sem { get; set; }
        public bool Status { get; set; } = true;
    }
}
