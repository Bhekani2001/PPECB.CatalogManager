using System;

namespace PPECB.CatalogManager.Core.Entities
{
    public class PriceHistory : BaseEntity
    {
        public int ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime ChangeDate { get; set; }
        public string? Reason { get; set; }

        public int? ChangedByUserId { get; set; }
        public virtual User? ChangedByUser { get; set; }
    }
}