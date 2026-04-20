using PPECB.CatalogManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PPECB.CatalogManager.Core.DTOs
{
    public class ProductFilterDto
    {
        public int? CategoryId { get; set; }
        public int? BrandId { get; set; }
        public int? SupplierId { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public ProductStatus? Status { get; set; }
        public string? SearchTerm { get; set; }
        public bool? IsPublished { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsOnSale { get; set; }
    }
}
