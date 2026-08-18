using System.ComponentModel.DataAnnotations;

namespace backend.DTOs;

public class CreateItemDto
{
    [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    [Required, MaxLength(100)] public string Category { get; set; } = string.Empty;
    [Required, Range(0, int.MaxValue)] public int Quantity { get; set; }
    [Required, MaxLength(50)] public string Unit { get; set; } = "unidades";
    [Range(0, int.MaxValue)] public int MinQuantity { get; set; }
    [MaxLength(255)] public string Donor { get; set; } = string.Empty;
    [Required] public DateTime EntryDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class UpdateItemDto
{
    [MaxLength(255)] public string? Name { get; set; }
    public string? Description { get; set; }
    [MaxLength(100)] public string? Category { get; set; }
    [Range(0, int.MaxValue)] public int? Quantity { get; set; }
    [MaxLength(50)] public string? Unit { get; set; }
    [Range(0, int.MaxValue)] public int? MinQuantity { get; set; }
    [MaxLength(255)] public string? Donor { get; set; }
    public DateTime? EntryDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
}

public class RegisterMovementDto
{
    [Range(1, int.MaxValue)] public int? ItemId { get; set; }
    [Required, Range(1, int.MaxValue)] public int Quantity { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class CategoryDto
{
    [Required, MaxLength(100)] public string Name { get; set; } = string.Empty;
}

public class NotificationConfigDto
{
    public bool OnEntry { get; set; }
    public bool OnExit { get; set; }
    public bool OnExpiry { get; set; }
    public List<string> Emails { get; set; } = new();
}

public class AddEmailDto
{
    [Required, EmailAddress, MaxLength(255)] public string Email { get; set; } = string.Empty;
}

public class DashboardDto
{
    public int TotalTypes { get; set; }
    public int TotalUnits { get; set; }
    public int ExpiringSoon { get; set; }
    public List<MovementDto> RecentMovements { get; set; } = new();
    public List<MonthlyDataPoint> MonthlyData { get; set; } = new();
    public PieChartData PieData { get; set; } = new();
}

public class MovementDto
{
    public int Id { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Source { get; set; } = "item";
}

public class MonthlyDataPoint
{
    public string Label { get; set; } = string.Empty;
    public int Entries { get; set; }
    public int Exits { get; set; }
}

public class PieChartData
{
    public int Entries { get; set; }
    public int Exits { get; set; }
}

public class LoginDto
{
    [Required, EmailAddress] public string Email { get; set; } = string.Empty;
    [Required] public string Password { get; set; } = string.Empty;
}

public class LoginResponseDto
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

public class CreateMovementDto
{
    [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
    [Required, RegularExpression("^(entry|exit)$")] public string Type { get; set; } = "entry";
    [Required, Range(1, int.MaxValue)] public int Quantity { get; set; }
    public string? Description { get; set; }
    [Required] public DateTime Date { get; set; }
}

public class ProductTypeDto
{
    [Required, MaxLength(255)] public string Name { get; set; } = string.Empty;
    [MaxLength(100)] public string? Category { get; set; }
}

public class CreateItemFromTypeDto
{
    [MaxLength(255)] public string Description { get; set; } = string.Empty;
}