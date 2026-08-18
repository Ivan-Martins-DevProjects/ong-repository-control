using Npgsql;
using backend.Models;

namespace backend.Repository;

public class StockRepository
{
    private readonly string _connectionString;

    public StockRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<List<Item>> GetAllAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.GetAllItems(), conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<Item>();
            while (await reader.ReadAsync())
                items.Add(MapItem(reader));
            return items;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao buscar itens.", ex);
        }
    }

    public async Task<PagedResult<Item>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var countCmd = new NpgsqlCommand(QueryProvider.CountAllItems(), conn);
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            await using var cmd = new NpgsqlCommand(QueryProvider.WrapPaged(QueryProvider.GetAllItems()), conn);
            cmd.Parameters.AddWithValue("page", page);
            cmd.Parameters.AddWithValue("pageSize", pageSize);
            await using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<Item>();
            while (await reader.ReadAsync())
                items.Add(MapItem(reader));
            return new PagedResult<Item> { Data = items, Page = page, PageSize = pageSize, Total = total };
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao buscar itens.", ex);
        }
    }

    public async Task<List<Item>> GetByProductTypeAsync(int productTypeId)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.GetItemsByProductType(), conn);
            cmd.Parameters.AddWithValue("productTypeId", productTypeId);
            await using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<Item>();
            while (await reader.ReadAsync())
                items.Add(MapItem(reader));
            return items;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Erro ao buscar itens do tipo {productTypeId}.", ex);
        }
    }

    public async Task<List<Item>> SearchAsync(string q, int? productTypeId)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.SearchItems(), conn);
            cmd.Parameters.AddWithValue("q", $"%{q}%");
            cmd.Parameters.AddWithValue("productTypeId", productTypeId ?? 0);
            await using var reader = await cmd.ExecuteReaderAsync();
            var items = new List<Item>();
            while (await reader.ReadAsync())
                items.Add(MapItem(reader));
            return items;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao pesquisar itens.", ex);
        }
    }

    public async Task<Item?> GetByIdAsync(int id)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.GetItemById(), conn);
            cmd.Parameters.AddWithValue("id", id);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapItem(reader);
            return null;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Erro ao buscar item {id}.", ex);
        }
    }

    public async Task<Item> CreateAsync(Item item)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.InsertItem(), conn);
            AddItemParameters(cmd, item);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapItem(reader);
            throw new ApplicationException("Erro ao criar item.");
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao criar item.", ex);
        }
    }

    public async Task<Item?> UpdateAsync(int id, Item item)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.UpdateItem(), conn);
            cmd.Parameters.AddWithValue("id", id);
            AddNullableParameters(cmd, item);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapItem(reader);
            return null;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Erro ao atualizar item {id}.", ex);
        }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.DeleteItem(), conn);
            cmd.Parameters.AddWithValue("id", id);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Erro ao excluir item {id}.", ex);
        }
    }

    public async Task<Item?> AdjustQuantityAsync(int id, int delta)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.AdjustQuantity(), conn);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("delta", delta);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapItem(reader);
            return null;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Erro ao ajustar quantidade do item {id}.", ex);
        }
    }

    public async Task UpdateCategoryAsync(int id, string category)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(QueryProvider.UpdateItemCategory(), conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("category", category);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateEntryDateAsync(int id, DateTime date)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(QueryProvider.UpdateItemEntryDate(), conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("date", date);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateExpiryDateAsync(int id, DateTime date)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand(QueryProvider.UpdateItemExpiryDate(), conn);
        cmd.Parameters.AddWithValue("id", id);
        cmd.Parameters.AddWithValue("date", date);
        await cmd.ExecuteNonQueryAsync();
    }

    private static Item MapItem(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        ProductTypeId = reader.GetInt32(reader.GetOrdinal("ProductTypeId")),
        Name = reader.GetString(reader.GetOrdinal("name")),
        Description = reader.IsDBNull(reader.GetOrdinal("description")) ? "" : reader.GetString(reader.GetOrdinal("description")),
        Category = reader.GetString(reader.GetOrdinal("category")),
        Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
        Unit = reader.GetString(reader.GetOrdinal("unit")),
        MinQuantity = reader.GetInt32(reader.GetOrdinal("MinQuantity")),
        Donor = reader.IsDBNull(reader.GetOrdinal("donor")) ? "" : reader.GetString(reader.GetOrdinal("donor")),
        EntryDate = reader.GetDateTime(reader.GetOrdinal("EntryDate")),
        ExpiryDate = reader.IsDBNull(reader.GetOrdinal("ExpiryDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ExpiryDate")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
        UpdatedAt = reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
    };

    private static void AddItemParameters(NpgsqlCommand cmd, Item item)
    {
        cmd.Parameters.AddWithValue("name", item.Name);
        cmd.Parameters.AddWithValue("productTypeId", item.ProductTypeId);
        cmd.Parameters.AddWithValue("description", item.Description);
        cmd.Parameters.AddWithValue("category", item.Category);
        cmd.Parameters.AddWithValue("quantity", item.Quantity);
        cmd.Parameters.AddWithValue("unit", item.Unit);
        cmd.Parameters.AddWithValue("minQuantity", item.MinQuantity);
        cmd.Parameters.AddWithValue("donor", item.Donor);
        cmd.Parameters.AddWithValue("entryDate", item.EntryDate);
        cmd.Parameters.AddWithValue("expiryDate", item.ExpiryDate ?? (object)DBNull.Value);
    }

    private static void AddNullableParameters(NpgsqlCommand cmd, Item item)
    {
        cmd.Parameters.AddWithValue("name", string.IsNullOrEmpty(item.Name) ? (object)DBNull.Value : item.Name);
        cmd.Parameters.AddWithValue("description", string.IsNullOrEmpty(item.Description) ? (object)DBNull.Value : item.Description);
        cmd.Parameters.AddWithValue("category", string.IsNullOrEmpty(item.Category) ? (object)DBNull.Value : item.Category);
        cmd.Parameters.AddWithValue("quantity", item.Quantity == 0 ? (object)DBNull.Value : item.Quantity);
        cmd.Parameters.AddWithValue("unit", string.IsNullOrEmpty(item.Unit) ? (object)DBNull.Value : item.Unit);
        cmd.Parameters.AddWithValue("minQuantity", item.MinQuantity == 0 ? (object)DBNull.Value : item.MinQuantity);
        cmd.Parameters.AddWithValue("donor", string.IsNullOrEmpty(item.Donor) ? (object)DBNull.Value : item.Donor);
        cmd.Parameters.AddWithValue("entryDate", item.EntryDate == default ? (object)DBNull.Value : item.EntryDate);
        cmd.Parameters.AddWithValue("expiryDate", item.ExpiryDate ?? (object)DBNull.Value);
    }
}