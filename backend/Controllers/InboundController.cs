using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.Models;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/inbound")]
public class InboundController : ControllerBase
{
    private readonly InboundProcessService _service;

    public InboundController(InboundProcessService service) => _service = service;

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

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var p = await _service.GetByIdAsync(id);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInboundProcessDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPatch("{id}/pause")]
    public async Task<IActionResult> Pause(int id)
    {
        var p = await _service.PauseAsync(id);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPatch("{id}/resume")]
    public async Task<IActionResult> Resume(int id)
    {
        var p = await _service.ResumeAsync(id);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPatch("{id}/finish")]
    public async Task<IActionResult> Finish(int id)
    {
        var p = await _service.FinishAsync(id);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPatch("{id}/cancel")]
    public async Task<IActionResult> Cancel(int id)
    {
        var p = await _service.CancelAsync(id);
        return p is null ? NotFound() : Ok(p);
    }

    [HttpPost("{id}/items")]
    public async Task<IActionResult> AddItem(int id, [FromBody] AddInboundItemDto dto)
    {
        var item = await _service.AddItemAsync(id, dto);
        return CreatedAtAction(nameof(GetItems), new { id }, item);
    }

    [HttpGet("{id}/items")]
    public async Task<IActionResult> GetItems(int id, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        if (pageSize <= 0) pageSize = 50;
        if (page <= 0) page = 1;
        return Ok(await _service.GetItemsPagedAsync(id, page, pageSize));
    }

    [HttpDelete("{id}/items/{itemId}")]
    public async Task<IActionResult> DeleteItem(int id, int itemId)
    {
        try
        {
            var ok = await _service.DeleteItemAsync(id, itemId);
            return ok ? NoContent() : NotFound();
        }
        catch (ApplicationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
