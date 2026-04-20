using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.IBusinessLogic;
using PPECB.CatalogManager.IRepositories;
using PPECB.CatalogManager.Repositories;
using PPECB.CatalogManager.BusinessLogic;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Configure Entity Framework with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsAssembly("PPECB.CatalogManager.WebApp")
    )
);

// Register Repositories
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

// Register Business Logic
builder.Services.AddScoped<ICategoryBusinessLogic, CategoryBusinessLogic>();
builder.Services.AddScoped<IProductBusinessLogic, ProductBusinessLogic>();
builder.Services.AddScoped<IUserBusinessLogic, UserBusinessLogic>();
builder.Services.AddScoped<IAuditLogBusinessLogic, AuditLogBusinessLogic>();
builder.Services.AddScoped<ISupplierBusinessLogic, SupplierBusinessLogic>();
builder.Services.AddScoped<IBrandBusinessLogic, BrandBusinessLogic>();
builder.Services.AddScoped<IWarehouseBusinessLogic, WarehouseBusinessLogic>();
builder.Services.AddScoped<IInventoryTransactionBusinessLogic, InventoryTransactionBusinessLogic>();
builder.Services.AddScoped<IProductImageBusinessLogic, ProductImageBusinessLogic>();

// Register AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// ===== ADD AUTHENTICATION =====
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/ShowLoginForm";
        options.LogoutPath = "/Account/ProcessLogout";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(2);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.Name = "PPECB.Auth";
    });

// Add Authorization
builder.Services.AddAuthorization();

// Add HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Add Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ===== IMPORTANT: Add Authentication & Authorization middleware in correct order =====
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();