namespace PPECB.CatalogManager.Core.DTOs
{
    public class CreateUserDto
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? MobileNumber { get; set; }
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
    }
}