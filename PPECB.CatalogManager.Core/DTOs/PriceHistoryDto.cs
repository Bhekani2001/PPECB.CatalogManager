using System;

namespace PPECB.CatalogManager.Core.DTOs
{
    public class PriceHistoryDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string? ProductCode { get; set; }
        public string? ProductName { get; set; }
        public decimal OldPrice { get; set; }
        public decimal NewPrice { get; set; }
        public DateTime ChangeDate { get; set; }
        public string? Reason { get; set; }
        public int? ChangedByUserId { get; set; }
        public string? ChangedByUserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}