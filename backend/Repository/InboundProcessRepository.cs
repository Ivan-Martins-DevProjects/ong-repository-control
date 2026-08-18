using Npgsql;
using backend.Models;

namespace backend.Repository;

public class InboundProcessRepository
{
    private readonly string _cs;
    public InboundProcessRepository(IConfiguration config) =>
        _cs = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException();

    public async Task<InboundProcess> CreateAsync(InboundProcess p)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.InsertInboundProcess(), conn);
            cmd.Parameters.AddWithValue("name", p.Name);
            cmd.Parameters.AddWithValue("description", p.Description);
            cmd.Parameters.AddWithValue("startDate", p.StartDate);
            cmd.Parameters.AddWithValue("endDate", p.EndDate);
            cmd.Parameters.AddWithValue("type", p.Type);
            await using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync()) return MapProcess(r);
            throw new ApplicationException("Erro ao criar processo.");
        }
        catch (Exception ex) { throw new ApplicationException("Erro ao criar processo.", ex); }
    }

    public async Task<List<InboundProcess>> GetAllAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.GetAllInboundProcesses(), conn);
            await using var r = await cmd.ExecuteReaderAsync();
            var list = new List<InboundProcess>();
            while (await r.ReadAsync()) list.Add(MapProcess(r));
            return list;
        }
        catch (Exception ex) { throw new ApplicationException("Erro ao buscar processos.", ex); }
    }

    public async Task<PagedResult<InboundProcess>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var countCmd = new NpgsqlCommand(QueryProvider.CountAllInboundProcesses(), conn);
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            await using var cmd = new NpgsqlCommand(QueryProvider.WrapPaged(QueryProvider.GetAllInboundProcesses()), conn);
            cmd.Parameters.AddWithValue("page", page);
            cmd.Parameters.AddWithValue("pageSize", pageSize);
            await using var r = await cmd.ExecuteReaderAsync();
            var list = new List<InboundProcess>();
            while (await r.ReadAsync()) list.Add(MapProcess(r));
            return new PagedResult<InboundProcess> { Data = list, Page = page, PageSize = pageSize, Total = total };
        }
        catch (Exception ex) { throw new ApplicationException("Erro ao buscar processos.", ex); }
    }

    public async Task<InboundProcess?> GetByIdAsync(int id)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.GetInboundProcessById(), conn);
            cmd.Parameters.AddWithValue("id", id);
            await using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync()) return MapProcess(r);
            return null;
        }
        catch (Exception ex) { throw new ApplicationException($"Erro ao buscar processo {id}.", ex); }
    }

    public async Task<InboundProcess?> UpdateStatusAsync(int id, string status)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.UpdateInboundProcessStatus(), conn);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("status", status);
            await using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync()) return MapProcess(r);
            return null;
        }
        catch (Exception ex) { throw new ApplicationException($"Erro ao atualizar status do processo {id}.", ex); }
    }

    public async Task<InboundItem> AddItemAsync(InboundItem item)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.InsertInboundItem(), conn);
            cmd.Parameters.AddWithValue("processId", item.ProcessId);
            cmd.Parameters.AddWithValue("productTypeId", item.ProductTypeId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("itemId", item.ItemId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("name", item.Name);
            cmd.Parameters.AddWithValue("quantity", item.Quantity);
            cmd.Parameters.AddWithValue("unit", item.Unit);
            cmd.Parameters.AddWithValue("expiryDate", item.ExpiryDate ?? (object)DBNull.Value);
            await using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync()) return MapItem(r);
            throw new ApplicationException("Erro ao adicionar item.");
        }
        catch (Exception ex) { throw new ApplicationException("Erro ao adicionar item.", ex); }
    }

    public async Task<List<InboundItem>> GetItemsAsync(int processId)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.GetInboundItemsByProcessId(), conn);
            cmd.Parameters.AddWithValue("processId", processId);
            await using var r = await cmd.ExecuteReaderAsync();
            var list = new List<InboundItem>();
            while (await r.ReadAsync()) list.Add(MapItem(r));
            return list;
        }
        catch (Exception ex) { throw new ApplicationException($"Erro ao buscar itens do processo {processId}.", ex); }
    }

    public async Task<PagedResult<InboundItem>> GetItemsPagedAsync(int processId, int page, int pageSize)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var countCmd = new NpgsqlCommand(QueryProvider.CountInboundItemsByProcessId(), conn);
            countCmd.Parameters.AddWithValue("processId", processId);
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            await using var cmd = new NpgsqlCommand(QueryProvider.WrapPaged(QueryProvider.GetInboundItemsByProcessId()), conn);
            cmd.Parameters.AddWithValue("processId", processId);
            cmd.Parameters.AddWithValue("page", page);
            cmd.Parameters.AddWithValue("pageSize", pageSize);
            await using var r = await cmd.ExecuteReaderAsync();
            var list = new List<InboundItem>();
            while (await r.ReadAsync()) list.Add(MapItem(r));
            return new PagedResult<InboundItem> { Data = list, Page = page, PageSize = pageSize, Total = total };
        }
        catch (Exception ex) { throw new ApplicationException($"Erro ao buscar itens do processo {processId}.", ex); }
    }

    public async Task<bool> DeleteItemAsync(int itemId)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.DeleteInboundItem(), conn);
            cmd.Parameters.AddWithValue("id", itemId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        catch (Exception ex) { throw new ApplicationException($"Erro ao excluir item {itemId}.", ex); }
    }

    public async Task DeleteItemsByProcessIdAsync(int processId)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.DeleteInboundItemsByProcessId(), conn);
            cmd.Parameters.AddWithValue("processId", processId);
            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex) { throw new ApplicationException($"Erro ao excluir itens do processo {processId}.", ex); }
    }

    public async Task AddQuantityToStockAsync(int itemId, int delta)
    {
        await using var conn = new NpgsqlConnection(_cs);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(QueryProvider.UpdateItemQuantity(), conn);
        cmd.Parameters.AddWithValue("id", itemId);
        cmd.Parameters.AddWithValue("delta", delta);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<int> CreateStockItemAsync(string name, int? productTypeId, string unit, int quantity)
    {
        await using var conn = new NpgsqlConnection(_cs);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(QueryProvider.InsertStockItemFromInbound(), conn);
        cmd.Parameters.AddWithValue("name", name);
        cmd.Parameters.AddWithValue("productTypeId", productTypeId ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("unit", unit);
        cmd.Parameters.AddWithValue("quantity", quantity);
        var result = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(result);
    }

    private static InboundProcess MapProcess(NpgsqlDataReader r) => new()
    {
        Id = r.GetInt32(r.GetOrdinal("id")),
        Name = r.GetString(r.GetOrdinal("name")),
        Description = r.IsDBNull(r.GetOrdinal("description")) ? "" : r.GetString(r.GetOrdinal("description")),
        StartDate = r.GetDateTime(r.GetOrdinal("StartDate")),
        EndDate = r.GetDateTime(r.GetOrdinal("EndDate")),
        Status = r.GetString(r.GetOrdinal("status")),
        Type = r.IsDBNull(r.GetOrdinal("type")) ? "entry" : r.GetString(r.GetOrdinal("type")),
        CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
    };

    private static InboundItem MapItem(NpgsqlDataReader r) => new()
    {
        Id = r.GetInt32(r.GetOrdinal("id")),
        ProcessId = r.GetInt32(r.GetOrdinal("ProcessId")),
        ProductTypeId = r.IsDBNull(r.GetOrdinal("ProductTypeId")) ? null : r.GetInt32(r.GetOrdinal("ProductTypeId")),
        ItemId = r.IsDBNull(r.GetOrdinal("ItemId")) ? null : r.GetInt32(r.GetOrdinal("ItemId")),
        Name = r.GetString(r.GetOrdinal("name")),
        Quantity = r.GetInt32(r.GetOrdinal("quantity")),
        Unit = r.GetString(r.GetOrdinal("unit")),
        ExpiryDate = r.IsDBNull(r.GetOrdinal("ExpiryDate")) ? null : r.GetDateTime(r.GetOrdinal("ExpiryDate")),
        CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
    };
}
