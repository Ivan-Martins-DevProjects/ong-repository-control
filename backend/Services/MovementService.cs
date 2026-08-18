using backend.DTOs;
using backend.Models;
using backend.Repository;

namespace backend.Services;

public class MovementService
{
    private readonly MovementRepository _repository;

    public MovementService(MovementRepository repository) => _repository = repository;

    public async Task<List<Movement>> GetAllAsync() =>
        await _repository.GetAllAsync();

    public async Task<PagedResult<Movement>> GetPagedAsync(int page, int pageSize, string? q = null, string? source = null, string? type = null, DateTime? from = null, DateTime? to = null) =>
        await _repository.GetPagedAsync(page, pageSize, q, source, type, from, to);

    public async Task<Movement> CreateAsync(CreateMovementDto dto) =>
        await _repository.InsertAsync(dto.Name, dto.Type, dto.Quantity, dto.Description ?? "", dto.Date);

    public async Task<PagedResult<Movement>?> GetGroupItemsPagedAsync(int movementId, int page, int pageSize) =>
        await _repository.GetGroupItemsPagedAsync(movementId, page, pageSize);
}