namespace backend.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string Role { get; set; } = "admin";
    public DateTime CreatedAt { get; set; }
}



public class InboundProcess
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Status { get; set; } = "active";
    public string Type { get; set; } = "entry";
    public DateTime CreatedAt { get; set; }
}

public class InboundItem
{
    public int Id { get; set; }
    public int ProcessId { get; set; }
    public int? ProductTypeId { get; set; }
    public int? ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Unit { get; set; } = "unidades";
    public DateTime? ExpiryDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateInboundProcessDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Type { get; set; } = "entry";
}

public class AddInboundItemDto
{
    public int? ProductTypeId { get; set; }
    public int? ItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string Unit { get; set; } = "unidades";
    public DateTime? ExpiryDate { get; set; }
}
