using System;
using System.Collections.Generic;

namespace PPECB.CatalogManager.Core.DTOs
{
    public class PurchaseOrderDto
    {
        public int Id { get; set; }
        public string PONumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public List<PurchaseOrderItemDto> Items { get; set; } = new List<PurchaseOrderItemDto>();
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }

    public class CreatePurchaseOrderDto
    {
        public string PONumber { get; set; } = string.Empty;
        public int SupplierId { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public string? Notes { get; set; }
        public List<CreatePurchaseOrderItemDto> Items { get; set; } = new List<CreatePurchaseOrderItemDto>();
    }

    public class UpdatePurchaseOrderDto
    {
        public int Id { get; set; }
        public DateTime? ExpectedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
    }
}