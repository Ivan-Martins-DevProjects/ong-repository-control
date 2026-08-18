using backend.DTOs;
using backend.Models;
using backend.Repository;

namespace backend.Services;

public class StockService
{
    private readonly StockRepository _repository;
    private readonly NotificationRepository _notificationRepo;

    public StockService(StockRepository repository, NotificationRepository notificationRepo)
    {
        _repository = repository;
        _notificationRepo = notificationRepo;
    }

    public async Task<List<Item>> GetAllAsync() =>
        await _repository.GetAllAsync();

    public async Task<PagedResult<Item>> GetPagedAsync(int page, int pageSize) =>
        await _repository.GetPagedAsync(page, pageSize);

    public async Task<Item?> GetByIdAsync(int id) =>
        await _repository.GetByIdAsync(id);

    public async Task<List<Item>> SearchAsync(string q, int? productTypeId) =>
        await _repository.SearchAsync(q, productTypeId);

    public async Task<Item> CreateAsync(CreateItemDto dto)
    {
        var item = new Item
        {
            Name = dto.Name,
            Description = dto.Description,
            Category = dto.Category,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            MinQuantity = dto.MinQuantity,
            Donor = dto.Donor,
            EntryDate = dto.EntryDate,
            ExpiryDate = dto.ExpiryDate,
        };
        var created = await _repository.CreateAsync(item);
        await _notificationRepo.InsertMovementAsync(created.Id, created.Name, "entry", created.Quantity, "Entrada inicial");
        return created;
    }

    public async Task<Item?> UpdateAsync(int id, UpdateItemDto dto)
    {
        var item = new Item
        {
            Name = dto.Name ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            Category = dto.Category ?? string.Empty,
            Quantity = dto.Quantity ?? 0,
            Unit = dto.Unit ?? string.Empty,
            MinQuantity = dto.MinQuantity ?? 0,
            Donor = dto.Donor ?? string.Empty,
            EntryDate = dto.EntryDate ?? default,
            ExpiryDate = dto.ExpiryDate,
        };
        return await _repository.UpdateAsync(id, item);
    }

    public async Task<bool> DeleteAsync(int id) =>
        await _repository.DeleteAsync(id);

    public async Task<Item?> RegisterEntryAsync(RegisterMovementDto dto)
    {
        var itemId = dto.ItemId ?? 0;
        var item = await _repository.AdjustQuantityAsync(itemId, dto.Quantity);
        if (item != null)
            await _notificationRepo.InsertMovementAsync(itemId, item.Name, "entry", dto.Quantity, dto.Description);
        return item;
    }

    public async Task<Item?> RegisterExitAsync(RegisterMovementDto dto)
    {
        var itemId = dto.ItemId ?? 0;
        var item = await _repository.AdjustQuantityAsync(itemId, -dto.Quantity);
        if (item != null)
            await _notificationRepo.InsertMovementAsync(itemId, item.Name, "exit", dto.Quantity, dto.Description);
        return item;
    }
}