using Npgsql;
using backend.Models;

namespace backend.Repository;

public class MovementRepository
{
    private readonly string _connectionString;

    public MovementRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found.");
    }

    public async Task<List<Movement>> GetAllAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.GetAllMovements(), conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            var list = new List<Movement>();
            while (await reader.ReadAsync())
                list.Add(MapMovement(reader));
            return list;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao buscar movimentos.", ex);
        }
    }

    public async Task<PagedResult<Movement>> GetPagedAsync(int page, int pageSize, string? q = null, string? source = null, string? type = null, DateTime? from = null, DateTime? to = null)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var query = QueryProvider.BuildMovementsQuery(q, source, type, from, to);

            await using var countCmd = new NpgsqlCommand(QueryProvider.CountFrom(query), conn);
            AddFilters(countCmd, q, source, type, from, to);
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            await using var cmd = new NpgsqlCommand(QueryProvider.WrapPaged(query), conn);
            cmd.Parameters.AddWithValue("page", page);
            cmd.Parameters.AddWithValue("pageSize", pageSize);
            AddFilters(cmd, q, source, type, from, to);
            await using var reader = await cmd.ExecuteReaderAsync();
            var list = new List<Movement>();
            while (await reader.ReadAsync())
                list.Add(MapMovement(reader));
            return new PagedResult<Movement> { Data = list, Page = page, PageSize = pageSize, Total = total };
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao buscar movimentos.", ex);
        }
    }

    private static void AddFilters(NpgsqlCommand cmd, string? q, string? source, string? type, DateTime? from, DateTime? to)
    {
        if (!string.IsNullOrWhiteSpace(q))
            cmd.Parameters.AddWithValue("q", $"%{q}%");
        if (!string.IsNullOrEmpty(source))
            cmd.Parameters.AddWithValue("source", source);
        if (type == "entry" || type == "exit")
            cmd.Parameters.AddWithValue("type", type);
        if (from.HasValue)
            cmd.Parameters.AddWithValue("from", from.Value);
        if (to.HasValue)
            cmd.Parameters.AddWithValue("to", to.Value);
    }

    public async Task<Movement> InsertAsync(string itemName, string type, int quantity, string description, DateTime date)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.InsertHistoryMovement(), conn);
            cmd.Parameters.AddWithValue("itemName", itemName);
            cmd.Parameters.AddWithValue("type", type);
            cmd.Parameters.AddWithValue("quantity", quantity);
            cmd.Parameters.AddWithValue("description", description);
            cmd.Parameters.AddWithValue("date", date);
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

    public async Task<Movement?> GetByIdAsync(int id)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.GetMovementById(), conn);
            cmd.Parameters.AddWithValue("id", id);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapMovement(reader);
            return null;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Erro ao buscar movimento {id}.", ex);
        }
    }

    public async Task<PagedResult<Movement>?> GetGroupItemsPagedAsync(int movementId, int page, int pageSize)
    {
        var baseMovement = await GetByIdAsync(movementId);
        if (baseMovement is null) return null;

        if (baseMovement.Source != "process")
            return new PagedResult<Movement> { Data = new List<Movement> { baseMovement }, Page = 1, PageSize = pageSize, Total = 1 };

        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            var query = QueryProvider.GetMovementGroupItems();

            await using var countCmd = new NpgsqlCommand(QueryProvider.CountFrom(query), conn);
            countCmd.Parameters.AddWithValue("id", movementId);
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            await using var cmd = new NpgsqlCommand(QueryProvider.WrapPaged(query), conn);
            cmd.Parameters.AddWithValue("id", movementId);
            cmd.Parameters.AddWithValue("page", page);
            cmd.Parameters.AddWithValue("pageSize", pageSize);
            await using var reader = await cmd.ExecuteReaderAsync();
            var list = new List<Movement>();
            while (await reader.ReadAsync())
                list.Add(MapMovement(reader));
            return new PagedResult<Movement> { Data = list, Page = page, PageSize = pageSize, Total = total };
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Erro ao buscar itens da movimentação {movementId}.", ex);
        }
    }

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