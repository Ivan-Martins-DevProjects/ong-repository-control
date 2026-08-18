using backend.DTOs;
using backend.Models;
using backend.Repository;

namespace backend.Services;

public class ProductTypeService
{
    private readonly ProductTypeRepository _repo;
    private readonly StockRepository _stockRepo;

    public ProductTypeService(ProductTypeRepository repo, StockRepository stockRepo)
    {
        _repo = repo;
        _stockRepo = stockRepo;
    }

    public async Task<List<ProductType>> GetAllAsync() => await _repo.GetAllAsync();
    public async Task<PagedResult<ProductType>> GetPagedAsync(int page, int pageSize) =>
        await _repo.GetPagedAsync(page, pageSize);
    public async Task<ProductType?> GetByIdAsync(int id) => await _repo.GetByIdAsync(id);
    public async Task<ProductType> CreateAsync(ProductTypeDto dto) =>
        await _repo.CreateAsync(dto.Name, dto.Category ?? "");

    public async Task<ProductType?> UpdateAsync(int id, ProductTypeDto dto) =>
        await _repo.UpdateAsync(id, dto.Name, dto.Category ?? "");

    public async Task<bool> DeleteAsync(int id) => await _repo.DeleteAsync(id);

    public async Task<List<Item>> GetItemsAsync(int productTypeId) =>
        await _stockRepo.GetByProductTypeAsync(productTypeId);

    public async Task<Item> AddItemAsync(int productTypeId, CreateItemFromTypeDto dto)
    {
        var type = await _repo.GetByIdAsync(productTypeId)
            ?? throw new ApplicationException("Tipo de produto não encontrado.");
        var item = new Item
        {
            Name = type.Name,
            ProductTypeId = productTypeId,
            Description = dto.Description,
            Quantity = 1,
            Unit = "unidades",
            Donor = string.Empty,
            EntryDate = DateTime.Now,
            ExpiryDate = null,
            Category = type.Category,
        };
        return await _stockRepo.CreateAsync(item);
    }
}