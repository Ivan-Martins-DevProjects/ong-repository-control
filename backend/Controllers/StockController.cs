using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class StockController : ControllerBase
{
  private readonly StockService _service;

  public StockController(StockService service) => _service = service;

  [HttpGet]
  public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
  {
    if (pageSize <= 0) pageSize = 20;
    if (page <= 0) page = 1;
    return Ok(await _service.GetPagedAsync(page, pageSize));
  }

  [HttpGet("all")]
  public async Task<IActionResult> GetAllUnpaged() =>
      Ok(await _service.GetAllAsync());

  [HttpGet("search")]
  public async Task<IActionResult> Search([FromQuery] string q, [FromQuery] int? productTypeId = null)
  {
    if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 3)
      return Ok(new List<object>());
    return Ok(await _service.SearchAsync(q.Trim(), productTypeId));
  }

  [HttpGet("{id}")]
  public async Task<IActionResult> GetById(int id)
  {
    var item = await _service.GetByIdAsync(id);
    return item is null ? NotFound() : Ok(item);
  }

  [HttpPost]
  public async Task<IActionResult> Create([FromBody] CreateItemDto dto)
  {
    var created = await _service.CreateAsync(dto);
    return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
  }

  [HttpPatch("{id}")]
  public async Task<IActionResult> Update(int id, [FromBody] UpdateItemDto dto)
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

  [HttpPost("{id}/entry")]
  public async Task<IActionResult> RegisterEntry(int id, [FromBody] RegisterMovementDto dto)
  {
    dto.ItemId = id;
    var item = await _service.RegisterEntryAsync(dto);
    return item is null ? NotFound() : Ok(item);
  }

  [HttpPost("{id}/exit")]
  public async Task<IActionResult> RegisterExit(int id, [FromBody] RegisterMovementDto dto)
  {
    dto.ItemId = id;
    var item = await _service.RegisterExitAsync(dto);
    return item is null ? BadRequest("Quantidade insuficiente ou item não encontrado.") : Ok(item);
  }

}
