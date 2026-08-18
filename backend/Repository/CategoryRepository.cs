using Npgsql;
using backend.Models;

namespace backend.Repository;

public class CategoryRepository
{
    private readonly string _connectionString;

    public CategoryRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    public async Task<List<Category>> GetAllAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.GetAllCategories(), conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            var categories = new List<Category>();
            while (await reader.ReadAsync())
                categories.Add(MapCategory(reader));
            return categories;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao buscar categorias.", ex);
        }
    }

    public async Task<Category> CreateAsync(string name)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.InsertCategory(), conn);
            cmd.Parameters.AddWithValue("name", name);
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
                return MapCategory(reader);
            throw new ApplicationException("Erro ao criar categoria.");
        }
        catch (PostgresException pg) when (pg.SqlState == "23505")
        {
            throw new ApplicationException($"Categoria '{name}' já existe.");
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao criar categoria.", ex);
        }
    }

    public async Task<Category?> UpdateAsync(int id, string name)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();

            var oldName = await GetNameByIdAsync(conn, id);
            if (oldName is null) return null;

            await using var tx = await conn.BeginTransactionAsync();

            await using (var rnTypes = new NpgsqlCommand(QueryProvider.RenameCategoryInProductTypes(), conn, tx))
            {
                rnTypes.Parameters.AddWithValue("oldName", oldName);
                rnTypes.Parameters.AddWithValue("newName", name);
                await rnTypes.ExecuteNonQueryAsync();
            }
            await using (var rnItems = new NpgsqlCommand(QueryProvider.RenameCategoryInItems(), conn, tx))
            {
                rnItems.Parameters.AddWithValue("oldName", oldName);
                rnItems.Parameters.AddWithValue("newName", name);
                await rnItems.ExecuteNonQueryAsync();
            }
            await using var cmd = new NpgsqlCommand(QueryProvider.UpdateCategoryById(), conn, tx);
            cmd.Parameters.AddWithValue("id", id);
            cmd.Parameters.AddWithValue("name", name);
            Category? updated = null;
            await using (var r = await cmd.ExecuteReaderAsync())
            {
                if (await r.ReadAsync()) updated = MapCategory(r);
            }
            await tx.CommitAsync();
            return updated;
        }
        catch (PostgresException pg) when (pg.SqlState == "23505")
        {
            throw new ApplicationException($"Categoria '{name}' já existe.");
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Erro ao atualizar categoria {id}.", ex);
        }
    }

    public async Task<bool> DeleteAsync(string name)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync();

            await using (var clearTypes = new NpgsqlCommand(QueryProvider.ClearCategoryFromProductTypes(), conn, tx))
            {
                clearTypes.Parameters.AddWithValue("name", name);
                await clearTypes.ExecuteNonQueryAsync();
            }
            await using (var clearItems = new NpgsqlCommand(QueryProvider.ClearCategoryFromItems(), conn, tx))
            {
                clearItems.Parameters.AddWithValue("name", name);
                await clearItems.ExecuteNonQueryAsync();
            }
            await using var cmd = new NpgsqlCommand(QueryProvider.DeleteCategoryByName(), conn, tx);
            cmd.Parameters.AddWithValue("name", name);
            var affected = await cmd.ExecuteNonQueryAsync();
            await tx.CommitAsync();
            return affected > 0;
        }
        catch (Exception ex)
        {
            throw new ApplicationException($"Erro ao excluir categoria '{name}'.", ex);
        }
    }

    private static async Task<string?> GetNameByIdAsync(NpgsqlConnection conn, int id)
    {
        await using var cmd = new NpgsqlCommand(QueryProvider.GetCategoryById(), conn);
        cmd.Parameters.AddWithValue("id", id);
        await using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? r.GetString(r.GetOrdinal("name")) : null;
    }

    private static Category MapCategory(NpgsqlDataReader reader) => new()
    {
        Id = reader.GetInt32(reader.GetOrdinal("id")),
        Name = reader.GetString(reader.GetOrdinal("name")),
        Unit = reader.IsDBNull(reader.GetOrdinal("unit")) ? "unidades" : reader.GetString(reader.GetOrdinal("unit")),
        CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
    };
}