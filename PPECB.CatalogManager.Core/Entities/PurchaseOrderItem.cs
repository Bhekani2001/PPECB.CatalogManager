namespace PPECB.CatalogManager.Core.Entities
{
    public class PurchaseOrderItem : BaseEntity
    {
        public int PurchaseOrderId { get; set; }
        public virtual PurchaseOrder PurchaseOrder { get; set; } = null!;

        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public int? ReceivedQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}