using PPECB.CatalogManager.Core.Enums;
using System;

namespace PPECB.CatalogManager.Core.Entities
{
    public class InventoryTransaction : BaseEntity
    {
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public int? WarehouseId { get; set; }
        public virtual Warehouse? Warehouse { get; set; }

        public TransactionType Type { get; set; } // Receipt, Issue, Transfer, Adjustment
        public int Quantity { get; set; }
        public int? PreviousStock { get; set; }
        public int? NewStock { get; set; }

        public string? ReferenceNumber { get; set; } // PO number, SO number
        public string? Notes { get; set; }

        public DateTime TransactionDate { get; set; }

        // User who performed transaction
        public int? PerformedByUserId { get; set; }
        public virtual User? PerformedByUser { get; set; }
    }
}