using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MovementsController : ControllerBase
{
    private readonly MovementService _service;

    public MovementsController(MovementService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? q = null,
        [FromQuery] string? source = null,
        [FromQuery] string? type = null,
        [FromQuery] string? from = null,
        [FromQuery] string? to = null)
    {
        if (pageSize <= 0) pageSize = 20;
        if (page <= 0) page = 1;

        DateTime? fromDate = null, toDate = null;
        if (DateTime.TryParse(from, out var fd)) fromDate = fd.Date;
        if (DateTime.TryParse(to, out var td)) toDate = td.Date.AddDays(1).AddTicks(-1);

        return Ok(await _service.GetPagedAsync(page, pageSize, q, source, type, fromDate, toDate));
    }

    [HttpGet("{id}/items")]
    public async Task<IActionResult> GetItems(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (pageSize <= 0) pageSize = 10;
        if (page <= 0) page = 1;
        var result = await _service.GetGroupItemsPagedAsync(id, page, pageSize);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMovementDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }
}