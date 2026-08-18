using Npgsql;
using backend.Models;

namespace backend.Repository;

public class ProductTypeRepository
{
    private readonly string _cs;
    public ProductTypeRepository(IConfiguration config) =>
        _cs = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException();

    public async Task<List<ProductType>> GetAllAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.GetAllProductTypes(), conn);
            await using var r = await cmd.ExecuteReaderAsync();
            var list = new List<ProductType>();
            while (await r.ReadAsync()) list.Add(Map(r));
            return list;
        }
        catch (Exception ex) { throw new ApplicationException("Erro ao buscar tipos.", ex); }
    }

    public async Task<PagedResult<ProductType>> GetPagedAsync(int page, int pageSize)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var countCmd = new NpgsqlCommand(QueryProvider.CountAllProductTypes(), conn);
            var total = Convert.ToInt32(await countCmd.ExecuteScalarAsync());

            await using var cmd = new NpgsqlCommand(QueryProvider.WrapPaged(QueryProvider.GetAllProductTypes()), conn);
            cmd.Parameters.AddWithValue("page", page);
            cmd.Parameters.AddWithValue("pageSize", pageSize);
            await using var r = await cmd.ExecuteReaderAsync();
            var list = new List<ProductType>();
            while (await r.ReadAsync()) list.Add(Map(r));
            return new PagedResult<ProductType> { Data = list, Page = page, PageSize = pageSize, Total = total };
        }
        catch (Exception ex) { throw new ApplicationException("Erro ao buscar tipos.", ex); }
    }

    public async Task<ProductType?> GetByIdAsync(int id)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.GetProductTypeById(), conn);
            cmd.Parameters.AddWithValue("id", id);
            await using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync()) return Map(r);
            return null;
        }
        catch (Exception ex) { throw new ApplicationException($"Erro ao buscar tipo {id}.", ex); }
    }

    public async Task<ProductType> CreateAsync(string name, string category)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.InsertProductType(), conn);
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("category", category);
            await using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync()) return new ProductType { Id = r.GetInt32(0), Name = r.GetString(1), Category = r.GetString(2), CreatedAt = r.GetDateTime(3) };
            throw new ApplicationException("Erro ao criar tipo.");
        }
        catch (PostgresException pg) when (pg.SqlState == "23505")
        {
            throw new ApplicationException($"Tipo '{name}' já existe.");
        }
        catch (Exception ex) { throw new ApplicationException("Erro ao criar tipo.", ex); }
    }

    public async Task<ProductType?> UpdateAsync(int id, string name, string category)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.UpdateProductType(), conn);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("name", name);
            cmd.Parameters.AddWithValue("category", category);
            await using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync())
                return new ProductType { Id = r.GetInt32(0), Name = r.GetString(1), Category = r.GetString(2), CreatedAt = r.GetDateTime(3) };
            return null;
        }
        catch (PostgresException pg) when (pg.SqlState == "23505")
        {
            throw new ApplicationException($"Nome '{name}' já existe.");
        }
        catch (Exception ex) { throw new ApplicationException($"Erro ao atualizar tipo {id}.", ex); }
    }

    public async Task<bool> DeleteAsync(int id)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();
            await using (var delItems = new NpgsqlCommand(QueryProvider.DeleteItemsByProductType(), conn, tx))
            {
                delItems.Parameters.AddWithValue("productTypeId", id);
                await delItems.ExecuteNonQueryAsync();
            }
            await using var delType = new NpgsqlCommand(QueryProvider.DeleteProductType(), conn, tx);
            delType.Parameters.AddWithValue("id", id);
            var affected = await delType.ExecuteNonQueryAsync();
            await tx.CommitAsync();
            return affected > 0;
        }
        catch (Exception ex) { throw new ApplicationException($"Erro ao excluir tipo {id}.", ex); }
    }

    private static ProductType Map(NpgsqlDataReader r) => new()
    {
        Id = r.GetInt32(r.GetOrdinal("id")),
        Name = r.GetString(r.GetOrdinal("name")),
        Category = r.GetString(r.GetOrdinal("Category")),
        ItemCount = r.GetInt32(r.GetOrdinal("ItemCount")),
        TotalQuantity = r.GetInt32(r.GetOrdinal("TotalQuantity")),
        CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
    };
}