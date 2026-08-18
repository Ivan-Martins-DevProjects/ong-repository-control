using Npgsql;
using backend.Models;

namespace backend.Repository;

public class UserRepository
{
    private readonly string _cs;
    public UserRepository(IConfiguration config) =>
        _cs = config.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException();

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            await using var conn = new NpgsqlConnection(_cs);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(
                "SELECT id, name, email, password_hash AS PasswordHash, password_salt AS PasswordSalt, role, created_at AS CreatedAt " +
                "FROM users WHERE email = @email", conn);
            cmd.Parameters.AddWithValue("email", email);
            await using var r = await cmd.ExecuteReaderAsync();
            if (await r.ReadAsync())
            {
                return new User
                {
                    Id = r.GetInt32(r.GetOrdinal("id")),
                    Name = r.GetString(r.GetOrdinal("name")),
                    Email = r.GetString(r.GetOrdinal("email")),
                    PasswordHash = r.GetString(r.GetOrdinal("PasswordHash")),
                    PasswordSalt = r.GetString(r.GetOrdinal("PasswordSalt")),
                    Role = r.GetString(r.GetOrdinal("role")),
                    CreatedAt = r.GetDateTime(r.GetOrdinal("CreatedAt")),
                };
            }
            return null;
        }
        catch (Exception ex) { throw new ApplicationException("Erro ao buscar usuário.", ex); }
    }
}
