using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _service;

    public NotificationsController(NotificationService service) => _service = service;

    [HttpGet("config")]
    public async Task<IActionResult> GetConfig() =>
        Ok(await _service.GetConfigAsync());

    [HttpGet("emails")]
    public async Task<IActionResult> GetEmails() =>
        Ok(await _service.GetEmailsAsync());

    [HttpPost("emails")]
    public async Task<IActionResult> AddEmail([FromBody] AddEmailDto dto)
    {
        var created = await _service.AddEmailAsync(dto);
        return CreatedAtAction(nameof(GetEmails), new { id = created.Id }, created);
    }

    [HttpDelete("emails/{email}")]
    public async Task<IActionResult> RemoveEmail(string email)
    {
        var deleted = await _service.RemoveEmailAsync(email);
        return deleted ? NoContent() : NotFound();
    }

    [HttpGet("events")]
    public async Task<IActionResult> GetEvents() =>
        Ok(await _service.GetEventsAsync());

    [HttpPatch("events/{eventKey}")]
    public async Task<IActionResult> UpdateEvent(string eventKey, [FromBody] bool enabled)
    {
        var updated = await _service.UpdateEventAsync(eventKey, enabled);
        return updated is null ? NotFound() : Ok(updated);
    }
}