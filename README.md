public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<MenuItem> Items { get; set; } = new();
}

public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
}

public enum TableStatus { Empty, Serving, WaitingPayment }

public class CafeTable
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public TableStatus Status { get; set; } = TableStatus.Empty;
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Role { get; set; } = "Staff"; // Manager, Cashier, Staff
    public bool IsLocked { get; set; }
}

public class Order
{
    public int Id { get; set; }
    public int? TableId { get; set; }          // null = mang đi
    public CafeTable? Table { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public decimal Discount { get; set; }
    public decimal Total { get; set; }
    public bool IsPaid { get; set; }
    public string? PaymentMethod { get; set; } // Cash, Card, Transfer
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int MenuItemId { get; set; }
    public MenuItem? MenuItem { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}


















using Microsoft.EntityFrameworkCore;
using CafeManager.Models;

namespace CafeManager.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<CafeTable> Tables => Set<CafeTable>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<MenuItem>().Property(x => x.Price).HasPrecision(18, 2);
        b.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Cà phê" },
            new Category { Id = 2, Name = "Trà" },
            new Category { Id = 3, Name = "Sinh tố" },
            new Category { Id = 4, Name = "Bánh" });
        b.Entity<CafeTable>().HasData(
            Enumerable.Range(1, 10).Select(i => new CafeTable { Id = i, Name = $"Bàn {i}" }));
    }
}












. Program.cs (đăng nhập bằng Cookie + phân quyền)using Microsoft.AspNetCore.Authentication.Cookies;
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

// Tạo DB + tài khoản admin mặc định
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.Users.Any())
    {
        db.Users.Add(new User
        {
            Username = "admin",
            FullName = "Quản lý",
            Role = "Manager",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123")
        });
        db.SaveChanges();
    }
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();





BÀN ĐƠN HÀNG VÀ THANH TOÁN 
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using CafeManager.Data;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    public AccountController(AppDbContext db) => _db = db;

    [HttpGet] public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        var user = _db.Users.FirstOrDefault(u => u.Username == username && !u.IsLocked);
        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu";
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role)
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
        return RedirectToAction("Index", "Home");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Login");
    }

    public IActionResult Denied() => View();
}


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeManager.Data;
using CafeManager.Models;

[Authorize] // phải đăng nhập
public class MenuController : Controller
{
    private readonly AppDbContext _db;
    public MenuController(AppDbContext db) => _db = db;

    // Mọi vai trò đều xem được; có tìm kiếm + lọc danh mục
    public async Task<IActionResult> Index(string? q, int? categoryId)
    {
        var query = _db.MenuItems.Include(m => m.Category).AsQueryable();
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(m => m.Name.Contains(q));
        if (categoryId.HasValue)
            query = query.Where(m => m.CategoryId == categoryId);

        ViewBag.Categories = await _db.Categories.ToListAsync();
        return View(await query.ToListAsync());
    }

    [Authorize(Roles = "Manager")]
    public IActionResult Create()
    {
        ViewBag.Categories = _db.Categories.ToList();
        return View();
    }

    [HttpPost, Authorize(Roles = "Manager")]
    public async Task<IActionResult> Create(MenuItem item)
    {
        _db.MenuItems.Add(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = "Manager")]
    public async Task<IActionResult> Edit(int id)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item == null) return NotFound();
        ViewBag.Categories = _db.Categories.ToList();
        return View(item);
    }

    [HttpPost, Authorize(Roles = "Manager")]
    public async Task<IActionResult> Edit(MenuItem item)
    {
        _db.Update(item);
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, Authorize(Roles = "Manager")]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item != null) { _db.MenuItems.Remove(item); await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }

    // Bật/tắt trạng thái còn bán
    [HttpPost, Authorize(Roles = "Manager")]
    public async Task<IActionResult> Toggle(int id)
    {
        var item = await _db.MenuItems.FindAsync(id);
        if (item != null) { item.IsAvailable = !item.IsAvailable; await _db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Index));
    }
}


[Authorize(Roles = "Manager,Cashier,Staff")]
public class OrderController : Controller
{
    private readonly AppDbContext _db;
    public OrderController(AppDbContext db) => _db = db;

    // Sơ đồ bàn
    public IActionResult Tables() => View(_db.Tables.ToList());

    // Mở đơn cho bàn (hoặc lấy đơn đang mở)
    public async Task<IActionResult> Open(int tableId)
    {
        var order = await _db.Orders.Include(o => o.Items).ThenInclude(i => i.MenuItem)
            .FirstOrDefaultAsync(o => o.TableId == tableId && !o.IsPaid);

        if (order == null)
        {
            order = new Order { TableId = tableId };
            _db.Orders.Add(order);
            (await _db.Tables.FindAsync(tableId))!.Status = TableStatus.Serving;
            await _db.SaveChangesAsync();
        }
        ViewBag.Menu = await _db.MenuItems.Where(m => m.IsAvailable).ToListAsync();
        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> AddItem(int orderId, int menuItemId, int quantity = 1)
    {
        var menu = await _db.MenuItems.FindAsync(menuItemId);
        var order = await _db.Orders.Include(o => o.Items).FirstAsync(o => o.Id == orderId);
        if (menu == null || order.IsPaid) return BadRequest();

        var line = order.Items.FirstOrDefault(i => i.MenuItemId == menuItemId);
        if (line == null)
            order.Items.Add(new OrderItem { MenuItemId = menuItemId, Quantity = quantity, UnitPrice = menu.Price });
        else
            line.Quantity += quantity;

        order.Total = order.Items.Sum(i => i.Quantity * i.UnitPrice) - order.Discount;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Open), new { tableId = order.TableId });
    }

    [HttpPost]
    public async Task<IActionResult> RemoveItem(int itemId)
    {
        var line = await _db.OrderItems.FindAsync(itemId);
        if (line == null) return NotFound();
        var order = await _db.Orders.Include(o => o.Items).FirstAsync(o => o.Id == line.OrderId);
        _db.OrderItems.Remove(line);
        order.Total = order.Items.Where(i => i.Id != itemId).Sum(i => i.Quantity * i.UnitPrice) - order.Discount;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Open), new { tableId = order.TableId });
    }

    // Thanh toán: chỉ Manager/Cashier
    [HttpPost, Authorize(Roles = "Manager,Cashier")]
    public async Task<IActionResult> Pay(int orderId, string method, decimal discount = 0)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstAsync(o => o.Id == orderId);
        order.Discount = discount;
        order.Total = Math.Max(0, order.Items.Sum(i => i.Quantity * i.UnitPrice) - discount);
        order.PaymentMethod = method;
        order.IsPaid = true;

        if (order.TableId.HasValue)
            (await _db.Tables.FindAsync(order.TableId))!.Status = TableStatus.Empty; // giải phóng bàn

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Invoice), new { id = orderId });
    }

    public async Task<IActionResult> Invoice(int id) =>
        View(await _db.Orders.Include(o => o.Table).Include(o => o.Items)
            .ThenInclude(i => i.MenuItem).FirstAsync(o => o.Id == id));

    public async Task<IActionResult> History() =>
        View(await _db.Orders.Where(o => o.IsPaid).OrderByDescending(o => o.CreatedAt).ToListAsync());
}

