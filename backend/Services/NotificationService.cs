using backend.DTOs;
using backend.Models;
using backend.Repository;

namespace backend.Services;

public class NotificationService
{
    private readonly NotificationRepository _repository;

    public NotificationService(NotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<NotificationConfigDto> GetConfigAsync()
    {
        var emails = await _repository.GetAllEmailsAsync();
        var events = await _repository.GetAllEventsAsync();
        return new NotificationConfigDto
        {
            Emails = emails.Select(e => e.Email).ToList(),
            OnEntry = events.FirstOrDefault(ev => ev.EventKey == "onEntry")?.Enabled ?? true,
            OnExit = events.FirstOrDefault(ev => ev.EventKey == "onExit")?.Enabled ?? true,
            OnExpiry = events.FirstOrDefault(ev => ev.EventKey == "onExpiry")?.Enabled ?? true,
        };
    }

    public async Task<List<NotificationEmail>> GetEmailsAsync() =>
        await _repository.GetAllEmailsAsync();

    public async Task<NotificationEmail> AddEmailAsync(AddEmailDto dto) =>
        await _repository.AddEmailAsync(dto.Email);

    public async Task<bool> RemoveEmailAsync(string email) =>
        await _repository.RemoveEmailAsync(email);

    public async Task<List<NotificationEvent>> GetEventsAsync() =>
        await _repository.GetAllEventsAsync();

    public async Task<NotificationEvent?> UpdateEventAsync(string eventKey, bool enabled) =>
        await _repository.UpdateEventAsync(eventKey, enabled);
}