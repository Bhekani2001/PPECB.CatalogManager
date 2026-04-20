using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using PPECB.CatalogManager.Core.Data;
using PPECB.CatalogManager.Core.Entities;
using PPECB.CatalogManager.IBusinessLogic;
using PPECB.CatalogManager.IRepositories;
using PPECB.CatalogManager.Repositories;
using PPECB.CatalogManager.BusinessLogic;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.MigrationsAssembly("PPECB.CatalogManager.WebApp")
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

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();
app.UseSession();

app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

    await dbContext.Database.EnsureCreatedAsync();

    if (!dbContext.Users.Any(u => u.Email == "admin@ppecb.com"))
    {
        var salt = GenerateSalt();
        var passwordHash = HashPassword("Admin@123", salt);

        var adminUser = new User
        {
            Email = "admin@ppecb.com",
            Username = "admin",
            FirstName = "System",
            LastName = "Administrator",
            PasswordHash = passwordHash,
            Salt = salt,
            IsActive = true,
            IsEmailConfirmed = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "System"
        };

        await dbContext.Users.AddAsync(adminUser);
        await dbContext.SaveChangesAsync();

        var adminRole = await dbContext.Roles.FirstOrDefaultAsync(r => r.Name == "Admin");
        if (adminRole != null)
        {
            var userRole = new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                AssignedDate = DateTime.UtcNow,
                AssignedBy = "System"
            };
            await dbContext.UserRoles.AddAsync(userRole);
            await dbContext.SaveChangesAsync();
        }

        Console.WriteLine("Default admin user created: admin@ppecb.com / Admin@123");
    }
}

app.Run();
static string GenerateSalt()
{
    byte[] saltBytes = new byte[32];
    using (var rng = RandomNumberGenerator.Create())
    {
        rng.GetBytes(saltBytes);
    }
    return Convert.ToBase64String(saltBytes);
}

static string HashPassword(string password, string salt)
{
    using var sha256 = SHA256.Create();
    var combined = Encoding.UTF8.GetBytes(password + salt);
    var hash = sha256.ComputeHash(combined);
    return Convert.ToBase64String(hash);
}