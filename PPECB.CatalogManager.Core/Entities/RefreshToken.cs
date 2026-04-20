using System;

namespace PPECB.CatalogManager.Core.Entities
{
    public class RefreshToken : BaseEntity
    {
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;

        public string Token { get; set; } = string.Empty;
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
        public string? RevokedByIp { get; set; }
        public string? CreatedByIp { get; set; }
        public string? DeviceInfo { get; set; }
    }
}