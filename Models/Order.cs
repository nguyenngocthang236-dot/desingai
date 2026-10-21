namespace CafeManager.Models;

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
