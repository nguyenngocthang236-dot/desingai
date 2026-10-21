using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using CafeManager.Data;
using CafeManager.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite("Data Source=cafe.db"));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(o =>
    {
        o.LoginPath = "/Account/Login";
        o.AccessDeniedPath = "/Account/Denied";
    });
builder.Services.AddAuthorization();

var app = builder.Build();

// Tạo DB + dữ liệu ban đầu
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();

    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User
            {
                Username = "admin",
                FullName = "Quản lý",
                Role = "Manager",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
            },
            new User
            {
                Username = "cashier",
                FullName = "Thu ngân",
                Role = "Cashier",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
            },
            new User
            {
                Username = "staff",
                FullName = "Nhân viên phục vụ",
                Role = "Staff",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123456")
            }
        );
        db.SaveChanges();
    }

    if (!db.MenuItems.Any())
    {
        db.MenuItems.AddRange(
            new MenuItem { Name = "Cà phê đen đá", Price = 20000, CategoryId = 1, Description = "Cà phê Robusta rang xay đậm đà", IsAvailable = true },
            new MenuItem { Name = "Cà phê sữa đá", Price = 25000, CategoryId = 1, Description = "Cà phê phin pha sữa đặc béo ngậy", IsAvailable = true },
            new MenuItem { Name = "Bạc xỉu", Price = 28000, CategoryId = 1, Description = "Nhiều sữa đặc và sữa tươi, một chút cà phê", IsAvailable = true },
            new MenuItem { Name = "Trà đào cam sả", Price = 35000, CategoryId = 2, Description = "Trà đen hương đào cùng cam vàng và sả tươi", IsAvailable = true },
            new MenuItem { Name = "Trà vải hoa lài", Price = 35000, CategoryId = 2, Description = "Trà xanh lài thanh nhẹ kết hợp trái vải ngọt", IsAvailable = true },
            new MenuItem { Name = "Sinh tố bơ", Price = 40000, CategoryId = 3, Description = "Bơ Đắk Lắk dẻo mịn béo thơm", IsAvailable = true },
            new MenuItem { Name = "Sinh tố xoài", Price = 35000, CategoryId = 3, Description = "Xoài cát chín vàng ngọt thanh tự nhiên", IsAvailable = true },
            new MenuItem { Name = "Bánh sừng bò (Croissant)", Price = 25000, CategoryId = 4, Description = "Bánh nướng bơ Pháp thơm lừng", IsAvailable = true },
            new MenuItem { Name = "Bánh Tiramisu", Price = 35000, CategoryId = 4, Description = "Bánh kem phô mai vị cà phê cacao Ý", IsAvailable = true }
        );
        db.SaveChanges();
    }
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
