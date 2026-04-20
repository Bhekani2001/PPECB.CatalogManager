namespace PPECB.CatalogManager.Core.DTOs
{
    public class CreateSupplierDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? TaxNumber { get; set; }
    }
}