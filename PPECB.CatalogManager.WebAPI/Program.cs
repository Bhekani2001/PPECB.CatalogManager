using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.IBusinessLogic;
using PPECB.CatalogManager.IRepositories;
using PPECB.CatalogManager.Repositories;
using PPECB.CatalogManager.BusinessLogic;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "PPECB Catalog Manager API",
        Version = "v1",
        Description = "API for managing products, categories, inventory, and users",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "PPECB",
            Email = "support@ppecb.com"
        }
    });
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsAssembly("PPECB.CatalogManager.WebAPI")
    )
);

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<ISupplierRepository, SupplierRepository>();
builder.Services.AddScoped<IBrandRepository, BrandRepository>();
builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
builder.Services.AddScoped<IInventoryTransactionRepository, InventoryTransactionRepository>();
builder.Services.AddScoped<IPriceHistoryRepository, PriceHistoryRepository>();
builder.Services.AddScoped<IPurchaseOrderRepository, PurchaseOrderRepository>();
builder.Services.AddScoped<IPurchaseOrderItemRepository, PurchaseOrderItemRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();

builder.Services.AddScoped<ICategoryBusinessLogic, CategoryBusinessLogic>();
builder.Services.AddScoped<IProductBusinessLogic, ProductBusinessLogic>();
builder.Services.AddScoped<IUserBusinessLogic, UserBusinessLogic>();
builder.Services.AddScoped<IAuditLogBusinessLogic, AuditLogBusinessLogic>();
builder.Services.AddScoped<ISupplierBusinessLogic, SupplierBusinessLogic>();
builder.Services.AddScoped<IBrandBusinessLogic, BrandBusinessLogic>();
builder.Services.AddScoped<IWarehouseBusinessLogic, WarehouseBusinessLogic>();
builder.Services.AddScoped<IInventoryTransactionBusinessLogic, InventoryTransactionBusinessLogic>();
builder.Services.AddScoped<IProductImageBusinessLogic, ProductImageBusinessLogic>();

builder.Services.AddAutoMapper(typeof(MappingProfile));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "PPECB Catalog Manager API v1");
    });
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();