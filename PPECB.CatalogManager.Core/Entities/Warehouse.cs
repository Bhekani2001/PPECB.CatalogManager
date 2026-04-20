using System.Collections.Generic;

namespace PPECB.CatalogManager.Core.Entities
{
    public class Warehouse : BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Address { get; set; }
        public string? ManagerName { get; set; }
        public string? Phone { get; set; }
        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
    }
}