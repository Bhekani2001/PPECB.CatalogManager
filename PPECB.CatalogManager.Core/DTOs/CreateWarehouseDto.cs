namespace PPECB.CatalogManager.Core.DTOs
{
    public class CreateWarehouseDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Address { get; set; }
        public string? ManagerName { get; set; }
        public string? Phone { get; set; }
    }
}