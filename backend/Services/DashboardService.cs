using backend.DTOs;
using backend.Models;
using backend.Repository;

namespace backend.Services;

public class DashboardService
{
    private readonly StockRepository _stockRepo;
    private readonly MovementRepository _movementRepo;
    private readonly ProductTypeRepository _productTypeRepo;

    public DashboardService(
        StockRepository stockRepo,
        MovementRepository movementRepo,
        ProductTypeRepository productTypeRepo)
    {
        _stockRepo = stockRepo;
        _movementRepo = movementRepo;
        _productTypeRepo = productTypeRepo;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var items = await _stockRepo.GetAllAsync();
        var allMovements = await _movementRepo.GetAllAsync();
        var allTypes = await _productTypeRepo.GetAllAsync();

        var now = DateTime.UtcNow;
        var expiringSoon = items.Count(i =>
            i.ExpiryDate.HasValue
            && i.ExpiryDate.Value >= now
            && i.ExpiryDate.Value <= now.AddDays(30));

        var recent = allMovements.OrderByDescending(m => m.Date).Take(10).Select(MapMovement).ToList();

        var months = new[] { "Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez" };
        var monthly = Enumerable.Range(0, 12).Select(i => new MonthlyDataPoint
        {
            Label = months[i],
            Entries = allMovements.Where(m => m.Type == "entry" && m.Date.Month == i + 1 && m.Date.Year == now.Year).Sum(m => m.Quantity),
            Exits = allMovements.Where(m => m.Type == "exit" && m.Date.Month == i + 1 && m.Date.Year == now.Year).Sum(m => m.Quantity),
        }).ToList();

        var totalEntries = allMovements.Where(m => m.Type == "entry").Sum(m => m.Quantity);
        var totalExits = allMovements.Where(m => m.Type == "exit").Sum(m => m.Quantity);

        return new DashboardDto
        {
            TotalTypes = allTypes.Count,
            TotalUnits = items.Sum(i => i.Quantity),
            ExpiringSoon = expiringSoon,
            RecentMovements = recent,
            MonthlyData = monthly,
            PieData = new PieChartData { Entries = totalEntries, Exits = totalExits },
        };
    }

    private static MovementDto MapMovement(Movement m) => new()
    {
        Id = m.Id,
        ItemId = m.ItemId,
        ItemName = m.ItemName,
        Type = m.Type,
        Quantity = m.Quantity,
        Date = m.Date,
        Description = m.Description,
        Source = m.Source,
    };
}