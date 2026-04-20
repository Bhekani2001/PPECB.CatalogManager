using System.ComponentModel.DataAnnotations;

namespace PPECB.CatalogManager.Core.DTOs
{
    public class UpdateProfileDto
    {
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; }
        public string? MobileNumber { get; set; }
        public string? Bio { get; set; }
        public string? Department { get; set; }
        public string? JobTitle { get; set; }
    }
}