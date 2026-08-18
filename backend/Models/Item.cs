namespace backend.Models;

public class Item
{
    public int Id { get; set; }
    public int ProductTypeId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Unit { get; set; } = "unidades";
    public int MinQuantity { get; set; }
    public string Donor { get; set; } = string.Empty;
    public DateTime EntryDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class ProductType
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public int TotalQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
}