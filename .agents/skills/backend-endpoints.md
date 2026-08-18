# Backend — Criação de Endpoints

## Arquitetura em camadas (obrigatória)

```
Controller → Service → Repository → QueryProvider
```

Cada endpoint novo DEVE seguir exatamente esta estrutura de 4 camadas:

```
backend/
├── Controllers/    → 1. Recebe requisição HTTP, valida dados, retorna resposta
├── Services/       → 2. Lógica de negócio, orquestra chamadas ao repository
├── Repository/     → 3. Executa queries no banco, trata erros de conexão
├── Repository/QueryProvider.cs  → 4. Strings SQL estáticas (apenas queries)
├── Models/         → Entidades que mapeiam as tabelas
└── DTOs/           → Objetos de requisição/resposta
```

## Passo a passo para criar um novo endpoint

### 1. Model (`Models/`)
Criar classe com as propriedades que mapeiam a tabela:

```csharp
namespace backend.Models;

public class MeuModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}
```

### 2. DTOs (`DTOs/DTOs.cs`)
Adicionar classes de request/response:

```csharp
namespace backend.DTOs;

public class CriarMeuModelDto
{
    [Required, MaxLength(100)] public string Nome { get; set; } = string.Empty;
}

public class MeuModelResponseDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}
```

### 3. QueryProvider (`Repository/QueryProvider.cs`)
Adicionar as queries como métodos **static** que retornam `string`:

```csharp
public static string GetMeusModels() =>
    "SELECT id, nome FROM meus_models ORDER BY id";

public static string InsertMeuModel() =>
    "INSERT INTO meus_models (nome) VALUES (@nome) RETURNING id, nome";
```

### 4. Repository (`Repository/`)
Classe que injeta `IConfiguration` para connection string, executa queries, trata exceções:

```csharp
namespace backend.Repository;

public class MeuModelRepository
{
    private readonly string _connectionString;

    public MeuModelRepository(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string not found.");
    }

    public async Task<List<MeuModel>> GetAllAsync()
    {
        try
        {
            await using var conn = new NpgsqlConnection(_connectionString);
            await conn.OpenAsync();
            await using var cmd = new NpgsqlCommand(QueryProvider.GetMeusModels(), conn);
            await using var reader = await cmd.ExecuteReaderAsync();
            var list = new List<MeuModel>();
            while (await reader.ReadAsync())
                list.Add(Map(reader));
            return list;
        }
        catch (Exception ex)
        {
            throw new ApplicationException("Erro ao buscar.", ex);
        }
    }
    // ...
}
```

### 5. Service (`Services/`)
Injeta o Repository, aplica regras de negócio:

```csharp
namespace backend.Services;

public class MeuModelService
{
    private readonly MeuModelRepository _repository;

    public MeuModelService(MeuModelRepository repo) => _repository = repo;

    public async Task<List<MeuModel>> GetAllAsync() =>
        await _repository.GetAllAsync();
}
```

### 6. Controller (`Controllers/`)
- Atributo `[Authorize]` na classe (protege todos os endpoints do controller)
- Atributo `[Route("api/[controller]")]`
- Construtor recebe Service por DI

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using backend.DTOs;
using backend.Services;

namespace backend.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MeusModelsController : ControllerBase
{
    private readonly MeuModelService _service;

    public MeusModelsController(MeuModelService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CriarMeuModelDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), new { id = created.Id }, created);
    }
}
```

### 7. Registrar DI em `Program.cs`

```csharp
builder.Services.AddScoped<MeuModelRepository>();
builder.Services.AddScoped<MeuModelService>();
```

## Autenticação (JWT + Cookie)

### Como funciona
- `POST /api/auth/login` → valida `admin@ong.org` / `admin` → gera JWT com **8h de expiração** → seta cookie `auth_token` **HttpOnly**
- Toda requisição protegida lê o token automaticamente do cookie via `JwtBearerEvents.OnMessageReceived`
- Sem cookie ou token inválido → `401 Unauthorized`

### Configuração em `Program.cs`
```
AddAuthentication → AddJwtBearer com TokenValidationParameters
  - ValidateIssuerSigningKey = true (HMAC SHA-256 com SecretKey do appsettings)
  - ValidateIssuer = true ("repositorycontrol")
  - ValidateAudience = true ("repositorycontrol")
  - ValidateLifetime = true
  - ClockSkew = TimeSpan.Zero
  - OnMessageReceived lê cookie "auth_token"
```

### Cookies
- Nome: `auth_token`
- HttpOnly: true (inacessível via JS)
- Secure: false (desenvolvimento)
- SameSite: Lax
- Path: /

### Controllers
- Adicionar `[Authorize]` na classe
- `AuthController`:
  - `POST /api/auth/login` → público
  - `POST /api/auth/logout` → público (remove cookie)
  - `GET /api/auth/me` → `[Authorize]` (retorna email e nome do token)

## Convenções

### Nomenclatura
- Controller: `[Modulo]Controller.cs`, rota `api/[controller]`
- Service: `[Modulo]Service.cs`
- Repository: `[Modulo]Repository.cs`
- DTOs: `Create[Nome]Dto`, `Update[Nome]Dto`, `[Nome]ResponseDto`
- Métodos: `GetAllAsync`, `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `DeleteAsync`

### Respostas HTTP
- `GET` list → `Ok(list)`
- `GET {id}` → `Ok(item)` ou `NotFound()`
- `POST` → `CreatedAtAction(nameof(GetAll), new { id }, created)`
- `PATCH {id}` → `Ok(updated)` ou `NotFound()`
- `DELETE {id}` → `NoContent()` ou `NotFound()`

### Tratamento de erros no Repository
- Todo método try/catch com `ApplicationException`
- Exceções específicas do PostgreSQL (`PostgresException` com `SqlState == "23505"` para unique violation)
- Sempre usar `await using` para NpgsqlConnection e NpgsqlCommand

### Queries no QueryProvider
- **SEMPRE** usar `AS` alias para colunas que mapeiam propriedades PascalCase
- Ex: `min_quantity AS MinQuantity`
- Parâmetros nomeados com `@nomeParametro`
- Queries estáticas e imutáveis (sem concatenação de strings SQL)

### CORS
```csharp
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```
- `AllowCredentials()` obrigatório para cookies
- Origem fixa em `localhost:4200`

### Docker
- Backend com `dotnet watch run` (hot reload)
- `Npgsql` já adicionado ao `.csproj`
- Connection string via variável de ambiente `ConnectionStrings__DefaultConnection`