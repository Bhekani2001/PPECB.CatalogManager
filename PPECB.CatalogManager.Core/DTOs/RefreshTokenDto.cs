using System;

namespace PPECB.CatalogManager.Core.DTOs
{
    public class RefreshTokenDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? CreatedByIp { get; set; }
        public string? DeviceInfo { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}