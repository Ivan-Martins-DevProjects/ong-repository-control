using backend.DTOs;
using backend.Models;
using backend.Repository;

namespace backend.Services;

public class CategoryService
{
    private readonly CategoryRepository _repository;

    public CategoryService(CategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Category>> GetAllAsync() =>
        await _repository.GetAllAsync();

    public async Task<Category> CreateAsync(CategoryDto dto) =>
        await _repository.CreateAsync(dto.Name);

    public async Task<Category?> UpdateAsync(int id, CategoryDto dto) =>
        await _repository.UpdateAsync(id, dto.Name);

    public async Task<bool> DeleteAsync(string name) =>
        await _repository.DeleteAsync(name);
}