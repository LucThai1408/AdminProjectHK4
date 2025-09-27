namespace API.Models
{
    public class AccountImage
    {
        public int AccountId { get; set; }
        public string? Name { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public int Role { get; set; } = 0;
        public IFormFile? Image { get; set; }
        public string? OldImage { get; set; }
    }
}
