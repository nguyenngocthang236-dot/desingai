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
        base.OnModelCreating(b);

        b.Entity<MenuItem>().Property(x => x.Price).HasPrecision(18, 2);
        b.Entity<Order>().Property(x => x.Discount).HasPrecision(18, 2);
        b.Entity<Order>().Property(x => x.Total).HasPrecision(18, 2);
        b.Entity<OrderItem>().Property(x => x.UnitPrice).HasPrecision(18, 2);

        b.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Cà phê" },
            new Category { Id = 2, Name = "Trà" },
            new Category { Id = 3, Name = "Sinh tố" },
            new Category { Id = 4, Name = "Bánh" });

        b.Entity<CafeTable>().HasData(
            Enumerable.Range(1, 10).Select(i => new CafeTable { Id = i, Name = $"Bàn {i}" }));
    }
}
