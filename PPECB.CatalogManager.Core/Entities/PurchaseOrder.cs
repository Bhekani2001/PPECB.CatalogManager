using System;
using System.Collections.Generic;

namespace PPECB.CatalogManager.Core.Entities
{
    public class PurchaseOrder : BaseEntity
    {
        public string PONumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public virtual Supplier Supplier { get; set; } = null!;

        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }

        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Shipped, Received, Cancelled

        public string? Notes { get; set; }

        // Navigation
        public virtual ICollection<PurchaseOrderItem> Items { get; set; } = new List<PurchaseOrderItem>();
    }
}