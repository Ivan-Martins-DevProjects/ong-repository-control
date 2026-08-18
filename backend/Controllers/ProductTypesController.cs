using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/product-types")]
public class ProductTypesController : ControllerBase
{
    private readonly ProductTypeService _service;

    public ProductTypesController(ProductTypeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _service.GetAllAsync());

    [HttpGet("paged")]
    public async Task<IActionResult> GetPaged([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;
        return Ok(await _service.GetPagedAsync(page, pageSize));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pt = await _service.GetByIdAsync(id);
        return pt is null ? NotFound() : Ok(pt);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] ProductTypeDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] ProductTypeDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("{id}/items")]
    public async Task<IActionResult> GetItems(int id) => Ok(await _service.GetItemsAsync(id));

    [HttpPost("{id}/items")]
    public async Task<IActionResult> AddItem(int id, [FromBody] CreateItemFromTypeDto dto)
    {
        var created = await _service.AddItemAsync(id, dto);
        return CreatedAtAction(nameof(GetItems), new { id }, created);
    }
}