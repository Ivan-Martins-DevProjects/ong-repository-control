using backend.Models;
using backend.Repository;

namespace backend.Services;

public class InboundProcessService
{
    private readonly InboundProcessRepository _repo;
    private readonly StockRepository _stockRepo;
    private readonly ProductTypeRepository _productTypeRepo;
    private readonly MovementRepository _movementRepo;

    public InboundProcessService(
        InboundProcessRepository repo,
        StockRepository stockRepo,
        ProductTypeRepository productTypeRepo,
        MovementRepository movementRepo)
    {
        _repo = repo;
        _stockRepo = stockRepo;
        _productTypeRepo = productTypeRepo;
        _movementRepo = movementRepo;
    }

    public async Task<InboundProcess> CreateAsync(CreateInboundProcessDto dto)
    {
        var p = new InboundProcess
        {
            Name = dto.Name,
            Description = dto.Description,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            Type = dto.Type,
        };
        return await _repo.CreateAsync(p);
    }

    public async Task<List<InboundProcess>> GetAllAsync() => await _repo.GetAllAsync();
    public async Task<PagedResult<InboundProcess>> GetPagedAsync(int page, int pageSize) =>
        await _repo.GetPagedAsync(page, pageSize);
    public async Task<InboundProcess?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);

    public async Task<InboundProcess?> PauseAsync(int id) => await _repo.UpdateStatusAsync(id, "paused");
    public async Task<InboundProcess?> ResumeAsync(int id) => await _repo.UpdateStatusAsync(id, "active");

    public async Task<InboundProcess?> FinishAsync(int id)
    {
        var process = await _repo.GetByIdAsync(id);
        if (process is null) return null;
        if (process.Status == "completed") return process;

        var items = await _repo.GetItemsAsync(id);
        var now = DateTime.UtcNow;

        if (process.Type == "exit")
        {
            foreach (var item in items)
            {
                if (!item.ItemId.HasValue) continue;
                var stockItem = await _stockRepo.GetByIdAsync(item.ItemId.Value);
                if (stockItem == null) continue;
                await _movementRepo.InsertAsync(stockItem.Name, "exit", item.Quantity, $"Saída: {process.Name}", now);
                await _stockRepo.DeleteAsync(item.ItemId.Value);
            }
        }
        else
        {
            var productTypes = await _productTypeRepo.GetAllAsync();

            foreach (var item in items)
            {
                var typeName = productTypes.FirstOrDefault(t => t.Id == item.ProductTypeId)?.Name;

                var existing = await _stockRepo.GetAllAsync();
                var match = existing.FirstOrDefault(s =>
                    s.ProductTypeId == item.ProductTypeId &&
                    s.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase));

                if (match != null)
                {
                    await _repo.AddQuantityToStockAsync(match.Id, item.Quantity);
                    await _movementRepo.InsertAsync(match.Name, "entry", item.Quantity, $"Entrada: {process.Name}", now);
                }
                else
                {
                    var newId = await _repo.CreateStockItemAsync(item.Name, item.ProductTypeId, item.Unit, item.Quantity);
                    var created = await _stockRepo.GetByIdAsync(newId);
                    if (created != null)
                    {
                        await _stockRepo.UpdateEntryDateAsync(newId, now);
                        if (item.ExpiryDate.HasValue)
                            await _stockRepo.UpdateExpiryDateAsync(newId, item.ExpiryDate.Value);
                        await _movementRepo.InsertAsync(created.Name, "entry", item.Quantity, $"Entrada: {process.Name}", now);
                    }
                }
            }
        }

        return await _repo.UpdateStatusAsync(id, "completed");
    }

    public async Task<InboundProcess?> CancelAsync(int id)
    {
        var process = await _repo.GetByIdAsync(id);
        if (process is null) return null;
        if (process.Status == "completed") return process;

        await _repo.DeleteItemsByProcessIdAsync(id);
        return await _repo.UpdateStatusAsync(id, "cancelled");
    }

    public async Task<InboundItem> AddItemAsync(int processId, AddInboundItemDto dto)
    {
        var process = await _repo.GetByIdAsync(processId);
        if (process is null) throw new ApplicationException("Processo não encontrado.");
        if (process.Status == "completed") throw new ApplicationException("Processo já finalizado.");

        var item = new InboundItem
        {
            ProcessId = processId,
            ProductTypeId = dto.ProductTypeId,
            ItemId = dto.ItemId,
            Name = dto.Name,
            Quantity = dto.Quantity,
            Unit = dto.Unit,
            ExpiryDate = dto.ExpiryDate,
        };
        return await _repo.AddItemAsync(item);
    }

    public async Task<List<InboundItem>> GetItemsAsync(int processId) => await _repo.GetItemsAsync(processId);
    public async Task<PagedResult<InboundItem>> GetItemsPagedAsync(int processId, int page, int pageSize) =>
        await _repo.GetItemsPagedAsync(processId, page, pageSize);

    public async Task<bool> DeleteItemAsync(int processId, int itemId)
    {
        var process = await _repo.GetByIdAsync(processId);
        if (process is null) throw new ApplicationException("Processo não encontrado.");
        if (process.Status == "completed") throw new ApplicationException("Não é possível excluir itens de um processo finalizado.");
        return await _repo.DeleteItemAsync(itemId);
    }
}
