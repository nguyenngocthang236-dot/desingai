namespace CafeManager.Models;

public enum TableStatus { Empty, Serving, WaitingPayment }

public class CafeTable
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public TableStatus Status { get; set; } = TableStatus.Empty;
}
