namespace backend.Models;

public class Movement
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Type { get; set; } = "entry";
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Source { get; set; } = "item";
    public DateTime CreatedAt { get; set; }
}