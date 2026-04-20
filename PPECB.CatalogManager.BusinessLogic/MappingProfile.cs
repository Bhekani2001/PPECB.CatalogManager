using AutoMapper;
using PPECB.CatalogManager.Core.DTOs;
using PPECB.CatalogManager.Core.Entities;

namespace PPECB.CatalogManager.BusinessLogic
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // Category mappings
            CreateMap<Category, CategoryDto>()
                .ForMember(dest => dest.SubCategories, opt => opt.Ignore());
            CreateMap<CreateCategoryDto, Category>();
            CreateMap<UpdateCategoryDto, Category>();

            // Product mappings
            CreateMap<Product, ProductDto>()
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : string.Empty));
            CreateMap<CreateProductDto, Product>();
            CreateMap<UpdateProductDto, Product>();

            // User mappings
            CreateMap<User, UserDto>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName))
                .ForMember(dest => dest.Roles, opt => opt.Ignore());
            CreateMap<CreateUserDto, User>();
            CreateMap<UpdateUserDto, User>();
            CreateMap<RegisterDto, User>();
            CreateMap<UpdateProfileDto, User>();

            // AuditLog mappings
            CreateMap<AuditLog, AuditLogDto>()
                .ForMember(dest => dest.UserEmail, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty));

            // ProductImage mappings
            CreateMap<ProductImage, ProductImageDto>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.Base64Image, opt => opt.Ignore());

            // Supplier mappings
            CreateMap<Supplier, SupplierDto>()
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));
            CreateMap<CreateSupplierDto, Supplier>();
            CreateMap<UpdateSupplierDto, Supplier>();

            // Brand mappings
            CreateMap<Brand, BrandDto>()
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));
            CreateMap<CreateBrandDto, Brand>();
            CreateMap<UpdateBrandDto, Brand>();

            // Warehouse mappings
            CreateMap<Warehouse, WarehouseDto>()
                .ForMember(dest => dest.ProductCount, opt => opt.MapFrom(src => src.Products != null ? src.Products.Count : 0));
            CreateMap<CreateWarehouseDto, Warehouse>();
            CreateMap<UpdateWarehouseDto, Warehouse>();

            // InventoryTransaction mappings
            CreateMap<InventoryTransaction, InventoryTransactionDto>()
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.WarehouseName, opt => opt.MapFrom(src => src.Warehouse != null ? src.Warehouse.Name : string.Empty))
                .ForMember(dest => dest.TransactionTypeName, opt => opt.MapFrom(src => src.Type.ToString()))
                .ForMember(dest => dest.PerformedByUserName, opt => opt.MapFrom(src => src.PerformedByUser != null ? src.PerformedByUser.Username : string.Empty));

            // PriceHistory mappings
            CreateMap<PriceHistory, PriceHistoryDto>()
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty))
                .ForMember(dest => dest.ChangedByUserName, opt => opt.MapFrom(src => src.ChangedByUser != null ? src.ChangedByUser.Username : string.Empty));

            // PurchaseOrder mappings
            CreateMap<PurchaseOrder, PurchaseOrderDto>()
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.Name : string.Empty));
            CreateMap<CreatePurchaseOrderDto, PurchaseOrder>();
            CreateMap<UpdatePurchaseOrderDto, PurchaseOrder>();

            // PurchaseOrderItem mappings
            CreateMap<PurchaseOrderItem, PurchaseOrderItemDto>()
                .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.Product != null ? src.Product.Code : string.Empty))
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : string.Empty));

            // Role mappings
            CreateMap<Role, RoleDto>();
            CreateMap<CreateRoleDto, Role>();
            CreateMap<UpdateRoleDto, Role>();

            // Permission mappings
            CreateMap<Permission, PermissionDto>();
            CreateMap<CreatePermissionDto, Permission>();
            CreateMap<UpdatePermissionDto, Permission>();

            // RefreshToken mappings
            CreateMap<RefreshToken, RefreshTokenDto>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User != null ? src.User.Username : string.Empty));
        }
    }
}