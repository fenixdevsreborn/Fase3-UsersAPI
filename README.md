# FCG Users API

API de usuários da FCG Cloud Platform (.NET 10, Clean Architecture, PostgreSQL, JWT).

## Decisões

- **Soft delete**: Sim. `DeletedAt` preenchido em vez de remover o registro (auditoria, integridade referencial com outros microsserviços).
- **Bootstrap admin**: Na primeira execução, se não existir nenhum admin, um é criado com os valores de `Bootstrap` em appsettings (configurável).
- **Hash de senha**: BCrypt (work factor 12).
- **JWT**: Issuer `Fcg.Users.Api`, Audience `fcg-cloud-platform`, claims: `sub` (userId), `email`, `name`, `role`, `scope`. Chave de assinatura configurável (mín. 32 caracteres).
- **Autorização**: `[Authorize(Roles = "admin")]` para rotas administrativas; `[Authorize]` para `/users/me`. Policy-based por role.
- **Erros**: Middleware global retorna JSON `{ "message": "..." }` e status HTTP adequado (401, 404, 409, 500).
- **Paginação**: `PageNumber` (1-based), `PageSize` (1–100, default 10).
- **Filtros**: `Name`, `Email` (contains), `Role` (exato), `IsActive` (bool).

## Pré-requisitos

- .NET 10 SDK
- PostgreSQL (ou use `UseInMemoryDatabase`/ambiente `Testing` para testes)

## Pacotes (já referenciados)

```bash
# Api
dotnet add src/Fcg.Users.Api package Microsoft.AspNetCore.OpenApi --version 10.0.3
dotnet add src/Fcg.Users.Api package Microsoft.OpenApi --version 2.0.0
dotnet add src/Fcg.Users.Api package Scalar.AspNetCore --version 2.13.6
dotnet add src/Fcg.Users.Api package Microsoft.EntityFrameworkCore.Design --version 10.0.0

# Infrastructure
dotnet add src/Fcg.Users.Infrastructure package BCrypt.Net-Next --version 4.0.3
dotnet add src/Fcg.Users.Infrastructure package Microsoft.EntityFrameworkCore --version 10.0.0
dotnet add src/Fcg.Users.Infrastructure package Microsoft.EntityFrameworkCore.InMemory --version 10.0.0
dotnet add src/Fcg.Users.Infrastructure package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.3
dotnet add src/Fcg.Users.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.0
dotnet add src/Fcg.Users.Infrastructure package Microsoft.EntityFrameworkCore.Design --version 10.0.0

# Testes
dotnet add tests/Fcg.Users.UnitTests package Moq --version 4.20.72
dotnet add tests/Fcg.Users.IntegrationTests package Microsoft.AspNetCore.Mvc.Testing --version 10.0.3
```

## Migrations

```bash
# Criar migration
dotnet ef migrations add InitialCreate --project src/Fcg.Users.Infrastructure --startup-project src/Fcg.Users.Api

# Aplicar (na execução da API ou manualmente)
dotnet ef database update --project src/Fcg.Users.Infrastructure --startup-project src/Fcg.Users.Api
```

A API aplica migrations automaticamente na subida (`MigrateAsync`) quando não está em `UseInMemoryDatabase`.

## Configuração

Em `appsettings.json` (ou variáveis de ambiente):

- **ConnectionStrings:UsersDb** – connection string PostgreSQL
- **Jwt:SigningKey** – chave com pelo menos 32 caracteres
- **Jwt:Issuer**, **Jwt:Audience**, **Jwt:ExpirationSeconds**
- **Bootstrap:CreateAdminIfNone**, **AdminEmail**, **AdminPassword**, **AdminName**

## Executar

```bash
dotnet run --project src/Fcg.Users.Api
```

- Documentação: **https://localhost:5xxx/scalar**
- OpenAPI JSON: **https://localhost:5xxx/openapi/v1.json**

## Testes

```bash
dotnet test Fcg.Users.slnx
```

Os testes de integração usam banco em memória (ambiente `Testing`).

---

## Exemplos de request/response

### POST /auth/login (público)

**Request:**
```json
{
  "email": "admin@fcg.local",
  "password": "ChangeMe@123"
}
```

**Response 200:**
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresInSeconds": 3600,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "System Admin",
    "email": "admin@fcg.local",
    "role": "admin"
  }
}
```

### POST /users (Admin)

**Headers:** `Authorization: Bearer <token>`

**Request:**
```json
{
  "name": "João Silva",
  "email": "joao@example.com",
  "password": "SenhaSegura123",
  "role": "user",
  "isActive": true,
  "bio": "Dev backend"
}
```

**Response 201:** corpo igual a `UserResponse` (id, name, email, role, isActive, avatarUrl, bio, createdAt, updatedAt).

### GET /users (Admin)

**Query:** `?pageNumber=1&pageSize=10&name=joão&email=example&role=user&isActive=true`

**Response 200:**
```json
{
  "items": [
    {
      "id": "...",
      "name": "João Silva",
      "email": "joao@example.com",
      "role": "user",
      "isActive": true,
      "avatarUrl": null,
      "bio": "Dev backend",
      "createdAt": "2025-03-12T...",
      "updatedAt": "2025-03-12T..."
    }
  ],
  "totalCount": 1,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 1,
  "hasPreviousPage": false,
  "hasNextPage": false
}
```

### GET /users/me (autenticado)

**Headers:** `Authorization: Bearer <token>`

**Response 200:** mesmo formato de `UserResponse` do usuário logado.

### PUT /users/me (autenticado)

**Request:**
```json
{
  "name": "Novo Nome",
  "avatarUrl": "https://...",
  "bio": "Minha bio"
}
```

**Response 200:** `UserResponse` atualizado.

### DELETE /users/me (autenticado)

**Response 204** (conta soft-deleted).

### GET /users/{id}, PUT /users/{id}, DELETE /users/{id}

Apenas **Admin**. Formato de resposta igual aos exemplos acima (GET/PUT retornam `UserResponse`; DELETE retorna 204).
