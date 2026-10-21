using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CafeManager.Data;
using CafeManager.Models;

namespace CafeManager.Controllers;

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
