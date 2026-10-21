using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeManager.Data;
using CafeManager.Models;

namespace CafeManager.Controllers;

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
            var table = await _db.Tables.FindAsync(tableId);
            if (table != null)
            {
                table.Status = TableStatus.Serving;
            }
            await _db.SaveChangesAsync();
        }
        ViewBag.Menu = await _db.MenuItems.Where(m => m.IsAvailable).ToListAsync();
        ViewBag.Table = await _db.Tables.FindAsync(tableId);
        return View(order);
    }

    [HttpPost]
    public async Task<IActionResult> AddItem(int orderId, int menuItemId, int quantity = 1)
    {
        var menu = await _db.MenuItems.FindAsync(menuItemId);
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (menu == null || order == null || order.IsPaid) return BadRequest();

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
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == line.OrderId);
        if (order == null) return NotFound();
        _db.OrderItems.Remove(line);
        order.Total = order.Items.Where(i => i.Id != itemId).Sum(i => i.Quantity * i.UnitPrice) - order.Discount;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Open), new { tableId = order.TableId });
    }

    // Thanh toán: chỉ Manager/Cashier
    [HttpPost, Authorize(Roles = "Manager,Cashier")]
    public async Task<IActionResult> Pay(int orderId, string method, decimal discount = 0)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return NotFound();

        order.Discount = discount;
        order.Total = Math.Max(0, order.Items.Sum(i => i.Quantity * i.UnitPrice) - discount);
        order.PaymentMethod = method;
        order.IsPaid = true;

        if (order.TableId.HasValue)
        {
            var table = await _db.Tables.FindAsync(order.TableId);
            if (table != null) table.Status = TableStatus.Empty; // giải phóng bàn
        }

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Invoice), new { id = orderId });
    }

    public async Task<IActionResult> Invoice(int id)
    {
        var order = await _db.Orders.Include(o => o.Table).Include(o => o.Items)
            .ThenInclude(i => i.MenuItem).FirstOrDefaultAsync(o => o.Id == id);
        if (order == null) return NotFound();
        return View(order);
    }

    public async Task<IActionResult> History() =>
        View(await _db.Orders.Include(o => o.Table).Where(o => o.IsPaid).OrderByDescending(o => o.CreatedAt).ToListAsync());
}
