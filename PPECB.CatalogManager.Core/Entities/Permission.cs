using System.Collections.Generic;

namespace PPECB.CatalogManager.Core.Entities
{
    public class Permission : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Module { get; set; }
        public string? Description { get; set; }

        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}