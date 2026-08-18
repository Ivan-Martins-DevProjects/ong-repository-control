using Npgsql;
using backend.Models;

namespace backend.Repository;

public class NotificationRepository
{
    private readonly string _connectionString;

    public NotificationRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    // =========================== EMAILS ===========================
    public async Task<List<NotificationEmail>> GetAllEmailsAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.GetAllEmails(), conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            var list = new List<NotificationEmail>();
            while (await reader.ReadAsync())
                list.Add(MapEmail(reader));
            return list;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao buscar emails.", ex);
        }
    }

    public async Task<NotificationEmail> AddEmailAsync(string email)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.InsertEmail(), conn);
            cmd.Parameters.AddWithValue("email", email);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapEmail(reader);
            throw new ApplicationException("Erro ao adicionar email.");
        }
        catch (PostgresException pg) when (pg.SqlState == "23505")
        {
            throw new ApplicationException($"Email '{email}' já cadastrado.");
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao adicionar email.", ex);
        }
    }

    public async Task<bool> RemoveEmailAsync(string email)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.DeleteEmail(), conn);
            cmd.Parameters.AddWithValue("email", email);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Erro ao remover email '{email}'.", ex);
        }
    }

    // =========================== EVENTS ===========================
    public async Task<List<NotificationEvent>> GetAllEventsAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.GetAllEvents(), conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            var list = new List<NotificationEvent>();
            while (await reader.ReadAsync())
                list.Add(MapEvent(reader));
            return list;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao buscar eventos.", ex);
        }
    }

    public async Task<NotificationEvent?> UpdateEventAsync(string eventKey, bool enabled)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.UpdateEventEnabled(), conn);
            cmd.Parameters.AddWithValue("eventKey", eventKey);
            cmd.Parameters.AddWithValue("enabled", enabled);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapEvent(reader);
            return null;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Erro ao atualizar evento '{eventKey}'.", ex);
        }
    }

    // =========================== MOVEMENTS ===========================
    public async Task<Movement> InsertMovementAsync(int itemId, string itemName, string type, int quantity, string description)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.InsertMovement(), conn);
            cmd.Parameters.AddWithValue("itemId", itemId);
            cmd.Parameters.AddWithValue("itemName", itemName);
            cmd.Parameters.AddWithValue("type", type);
            cmd.Parameters.AddWithValue("quantity", quantity);
            cmd.Parameters.AddWithValue("description", description);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapMovement(reader);
            throw new ApplicationException("Erro ao registrar movimento.");
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao registrar movimento.", ex);
        }
    }

    private static NotificationEmail MapEmail(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        Email = reader.GetString(reader.GetOrdinal("email")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
    };

    private static NotificationEvent MapEvent(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        EventKey = reader.GetString(reader.GetOrdinal("EventKey")),
        Enabled = reader.GetBoolean(reader.GetOrdinal("enabled")),
        Label = reader.GetString(reader.GetOrdinal("label")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
    };

    private static Movement MapMovement(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        ItemId = reader.GetInt32(reader.GetOrdinal("ItemId")),
        ItemName = reader.GetString(reader.GetOrdinal("ItemName")),
        Type = reader.GetString(reader.GetOrdinal("type")),
        Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
        Date = reader.GetDateTime(reader.GetOrdinal("date")),
        Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString(reader.GetOrdinal("description")),
        Source = reader.IsDBNull(reader.GetOrdinal("source")) ? "item" : reader.GetString(reader.GetOrdinal("source")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
    };
}