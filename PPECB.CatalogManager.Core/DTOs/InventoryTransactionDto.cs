using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.Core.Enums;
using System;

namespace PPECB.CatalogManager.Core.DTOs
{
    public class InventoryTransactionDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public int? WarehouseId { get; set; }
        public string? WarehouseName { get; set; }
        public TransactionType Type { get; set; }
        public string TransactionTypeName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int? PreviousStock { get; set; }
        public int? NewStock { get; set; }
        public string? ReferenceNumber { get; set; }
        public string? Notes { get; set; }
        public DateTime TransactionDate { get; set; }
        public int? PerformedByUserId { get; set; }
        public string? PerformedByUserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}