# FCG Users API

API de usuários da **FCG Cloud Platform**: autenticação JWT (RS256), CRUD de usuários, perfis e integração com orquestração AWS (ECS/ECR). Desenvolvida em **.NET 10**, **Clean Architecture**, persistência **PostgreSQL** e documentação OpenAPI (Scalar).

---

## Índice

- [O que este repositório faz](#o-que-este-repositório-faz)
- [Estrutura da solução](#estrutura-da-solução)
- [Decisões de design](#decisões-de-design)
- [Pré-requisitos](#pré-requisitos)
- [Configuração](#configuração)
- [Execução](#execução)
- [Docker](#docker)
- [CI/CD](#cicd)
- [Migrations](#migrations)
- [Testes](#testes)
- [Endpoints](#endpoints)
- [Exemplos de request/response](#exemplos-de-requestresponse)
- [Health, OpenAPI e descoberta](#health-openapi-e-descoberta)
- [Observabilidade](#observabilidade)

---

## O que este repositório faz

- **Autenticação**: login por e-mail ou username + senha; emissão de JWT (RS256) com suporte a JWKS e descoberta OIDC para API Gateway / clientes.
- **Usuários**: criação (admin), listagem com paginação e filtros, obtenção/atualização/remoção por ID (admin) e perfis do próprio usuário (`/users/me`).
- **Segurança**: hash de senha (BCrypt), soft delete, políticas de autorização por role (admin vs usuário autenticado).
- **Bootstrap**: na primeira execução, se não existir nenhum admin, um usuário admin é criado automaticamente (credenciais padrão configuráveis).
- **Infraestrutura**: Docker (imagem só API ou all-in-one Postgres + API), GitHub Actions para CI e publicação de imagem no Amazon ECR, integração opcional com orquestrador (deploy ECS).
- **Observabilidade**: health checks, OpenTelemetry (métricas/rastreamento), logs estruturados.

---

## Estrutura da solução

| Projeto | Descrição |
|--------|-----------|
| **Fcg.Users.Api** | Web API (controllers, middleware, auth, OpenAPI). |
| **Fcg.Users.Application** | Casos de uso, serviços, exceções de domínio. |
| **Fcg.Users.Contracts** | DTOs, requests/responses, opções JWT e paginação. |
| **Fcg.Users.Domain** | Entidades, enums, repositórios (interfaces). |
| **Fcg.Users.Infrastructure** | EF Core, PostgreSQL, repositórios, JWT (RS256), BCrypt, seeders, provedores de chave RSA (File, Environment, AWS). |
| **Fcg.Users.ServiceDefaults** | Configurações compartilhadas para ambientes Aspire/serviço. |
| **Fcg.Users.AppHost** | Host Aspire: orquestra Postgres + API para desenvolvimento local. |
| **Fcg.Users.UnitTests** | Testes unitários. |
| **Fcg.Users.IntegrationTests** | Testes de integração (API + banco em memória, ambiente `Testing`). |

Solution: `Fcg.Users.slnx` (inclui também `docker-compose.dcproj` e AppHost).

---

## Decisões de design

| Tópico | Decisão |
|--------|---------|
| **Soft delete** | Sim. `DeletedAt` preenchido em vez de remover o registro (auditoria e integridade referencial com outros microsserviços). |
| **Bootstrap admin** | Na primeira execução, se não existir nenhum admin, um é criado pelo `FirstAdminSeeder` (username `admin`, email `admin@fcg.local`, senha `ChangeMe@123`, nome "System Admin"). Valores atualmente fixos no seeder; configurável via extensão futura em appsettings. |
| **Hash de senha** | BCrypt (work factor 12). |
| **JWT** | Algoritmo **RS256**. Issuer e Audience configuráveis (`Jwt:Issuer`, `Jwt:Audience`). Claims incluem identificação do usuário e role. Chaves RSA via **Jwt:Signing** (File, Environment, AwsParameterStore, AwsSecretsManager). |
| **Autorização** | Policy-based: `FcgPolicies.RequireAdmin` para rotas administrativas; `FcgPolicies.RequireAuthenticatedUser` para `/users/me`. |
| **Erros** | Middleware global (`ExceptionHandlingMiddleware`) retorna JSON `{ "message": "..." }` e status HTTP adequado (400, 401, 404, 409, 500). |
| **Paginação** | `PageNumber` (1-based), `PageSize` (1–100, default 10). Resposta no formato `PagedResponse<T>`. |
| **Filtros (listagem)** | `Name`, `Username`, `Email` (contains), `Role` (exato), `IsActive` (bool opcional). |

---

## Pré-requisitos

- **.NET 10 SDK**
- **PostgreSQL** (ou uso de `UseInMemoryDatabase` / ambiente `Testing` para testes)
- Para publicação de imagem: **Docker** e acesso AWS (ECR) configurado no GitHub (OIDC)

---

## Configuração

Configuração via `appsettings.json` e/ou **variáveis de ambiente** (override).

### ConnectionStrings

| Chave | Descrição |
|-------|-----------|
| `ConnectionStrings:UsersDb` | Connection string PostgreSQL (obrigatória em produção quando não usa in-memory). |

### JWT

| Chave | Descrição |
|-------|-----------|
| `Jwt:Issuer` | URL do emissor (ex.: `https://users-api.example.com`). Obrigatória em produção; usada no token e na descoberta OIDC. |
| `Jwt:Audience` | Audience do token (ex.: `fcg-cloud-platform`). |
| `Jwt:ExpirationSeconds` | Tempo de vida do access token em segundos (ex.: 3600). |

### Jwt:Signing (chave RSA)

| Chave | Descrição |
|-------|-----------|
| `Jwt:Signing:Provider` | `File`, `Environment`, `AwsParameterStore` ou `AwsSecretsManager`. |
| `Jwt:Signing:CurrentKeyId` | ID da chave atual (ex.: `default`, `dev-1`). Usado como `kid` no header do JWT. |
| `Jwt:Signing:FilePath` | (Provider=File) Caminho para o PEM da chave privada (ex.: `keys/private.pem`). |
| `Jwt:Signing:EnvironmentVariableName` | (Provider=Environment) Nome da variável de ambiente com o PEM (ex.: `JWT_PRIVATE_KEY_PEM`). |
| `Jwt:Signing:AwsParameterName` | (Provider=AwsParameterStore) Nome do parâmetro SSM. |
| `Jwt:Signing:AwsSecretId` | (Provider=AwsSecretsManager) ID/ARN do secret. |

### Outros

- **Bootstrap**: primeiro admin é criado pelo seeder se não houver nenhum admin (valores atualmente no código do `FirstAdminSeeder`).
- **UseInMemoryDatabase**: quando `true` (ex.: ambiente `Testing`), não usa PostgreSQL e não aplica migrations de disco.
- **EnableOpenApi**: em produção, definir como `true` se quiser expor `/openapi/v1.json` e Scalar.

---

## Execução

### Local (API + PostgreSQL manual)

```bash
dotnet run --project src/Fcg.Users.Api
```

- Documentação: **https://localhost:5xxx/scalar/v1**
- OpenAPI JSON: **https://localhost:5xxx/openapi/v1.json**
- Health: **https://localhost:5xxx/health**

### Aspire (Postgres + API orquestrados)

```bash
dotnet run --project src/Fcg.Users.AppHost
```

O AppHost sobe o container Postgres e a API com referência ao banco `UsersDb`.

### Testes

```bash
dotnet test Fcg.Users.slnx
```

Os testes de integração usam banco em memória (ambiente `Testing`).

---

## Docker

### Docker Compose (PostgreSQL 17 + API)

O `docker-compose.yml` sobe **dois containers**: primeiro o **PostgreSQL 17** (com volume persistente), depois a **API** (que só inicia após o Postgres estar saudável).

- **postgres**: imagem `postgres:17-bookworm`, volume `users_pgdata` para persistência, healthcheck; sobe primeiro.
- **fcg.users.api**: depende de `postgres` com `condition: service_healthy`; conecta em `Host=postgres`.

```bash
# Build e execução (Postgres sobe primeiro, depois a API)
docker compose build
docker compose up -d

# API em http://localhost:8080 | Postgres em localhost:5432 (usuário/senha: postgres, database: fcg_users)
docker compose logs -f
```

Para rodar mais de uma API (Users, Payments, Games) ao mesmo tempo na mesma máquina, altere a porta do Postgres em um dos compose (ex.: `"5433:5432"`) para evitar conflito.

### Dockerfile (somente API)

- **Contexto**: raiz do repositório.
- **Build**: multi-stage com SDK .NET 10; publica a API em Release.
- **Runtime**: imagem `aspnet:10.0-preview`; porta **8080**; `ASPNETCORE_URLS=http://+:8080`.
- Uso: quando o PostgreSQL está em outro container ou serviço gerenciado.

```bash
docker build -f Dockerfile -t fcg-users-api .
docker run -p 8080:8080 -e ConnectionStrings__UsersDb="Host=host.docker.internal;..." fcg-users-api
```

### Dockerfile.postgres (all-in-one: Postgres + API)

- **Contexto**: raiz do repositório.
- **Build**: mesmo estágio de build da API; estágio final baseado em `postgres:17-bookworm` com .NET runtime copiado.
- **Runtime**: entrypoint customizado `docker/entrypoint.sh` sobe o Postgres, espera ficar pronto (`pg_isready`) e executa `dotnet Fcg.Users.Api.dll`.
- **Porta**: 8080 (API). Variáveis `POSTGRES_USER`, `POSTGRES_PASSWORD` para o Postgres interno.
- Uso: deploy em ECS ou ambientes em que a task roda um único container com API + banco (ConnectionString com `Host=localhost`).

```bash
docker build -f Dockerfile.postgres -t fcg-users-api:postgres .
docker run -p 8080:8080 -e POSTGRES_PASSWORD=postgres fcg-users-api:postgres
```

O workflow **Publish image** do GitHub Actions faz build com `Dockerfile.postgres` e push para o ECR.

---

## CI/CD

### Workflow: CI (`.github/workflows/ci.yml`)

- **Trigger**: push e pull_request na branch `junonn/mvp-aws`.
- **Etapas**: Checkout → Setup .NET 10 → Restore → Build (Release) → Test (Release). Upload opcional dos resultados (.trx) como artefato.

### Workflow: Publish image (`.github/workflows/publish-image.yml`)

- **Trigger**: push na branch `junonn/mvp-aws`.
- **Etapas**: Checkout → Configure AWS (OIDC) → Login ECR → Definir tag da imagem → Build com `Dockerfile.postgres` → Push para ECR.
- **Imagem**: tag no formato `users-api-latest` no repositório ECR (nome do repositório configurável via variable `ECR_REPOSITORY_NAME`, ex.: `fcg/fase03`).

**Secrets/variables** (ver `.github/workflows/README.md`):

- **Secrets**: `AWS_ROLE_ARN_ECR` (role IAM para OIDC e push ECR).
- **Variables**: `AWS_REGION`, `ECR_REPOSITORY_NAME`, etc.

---

## Migrations

```bash
# Criar migration
dotnet ef migrations add NomeDaMigration --project src/Fcg.Users.Infrastructure --startup-project src/Fcg.Users.Api

# Aplicar
dotnet ef database update --project src/Fcg.Users.Infrastructure --startup-project src/Fcg.Users.Api
```

A API aplica migrations automaticamente na subida (`MigrateAsync`) quando **não** está em `UseInMemoryDatabase`. O `PostgresDatabaseEnsurer` garante que o banco exista antes de aplicar migrations.

---

## Testes

```bash
dotnet test Fcg.Users.slnx
```

- **UnitTests**: serviços e lógica isolada.
- **IntegrationTests**: API com `WebApplicationFactory`, banco em memória (ambiente `Testing`).

---

## Endpoints

| Método | Rota | Autenticação | Descrição |
|--------|------|--------------|-----------|
| POST | `/auth/login` | Público | Login (email ou username + senha); retorna JWT e dados do usuário. |
| GET | `/.well-known/openid-configuration` | Público | Descoberta OIDC (issuer, jwks_uri, etc.). |
| GET | `/.well-known/jwks.json` | Público | JWKS (chaves públicas para validação RS256). |
| GET | `/api/discovery` | Público | Metadados do serviço (nome, basePath, URLs OpenAPI e health). |
| POST | `/users` | Admin | Criar usuário. |
| GET | `/users` | Admin | Listar usuários (paginação + filtros). |
| GET | `/users/{id}` | Admin | Obter usuário por ID. |
| PUT | `/users/{id}` | Admin | Atualizar usuário por ID. |
| DELETE | `/users/{id}` | Admin | Soft delete do usuário por ID. |
| GET | `/users/me` | Autenticado | Perfil do usuário logado. |
| PUT | `/users/me` | Autenticado | Atualizar próprio perfil (name, avatarUrl, bio). |
| DELETE | `/users/me` | Autenticado | Soft delete da própria conta. |
| GET | `/health` | Público | Health check (liveness). |
| GET | `/health/ready` | Público | Readiness (inclui checagem do DbContext). |

---

## Exemplos de request/response

### POST /auth/login (público)

**Request:**

```json
{
  "login": "admin@fcg.local",
  "password": "ChangeMe@123"
}
```

(`login` pode ser e-mail ou username.)

**Response 200:**

```json
{
  "accessToken": "eyJhbGciOiJSUzI1NiIs...",
  "tokenType": "Bearer",
  "expiresInSeconds": 3600,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "System Admin",
    "username": "admin",
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
  "username": "joao",
  "email": "joao@example.com",
  "password": "SenhaSegura123",
  "role": "user",
  "isActive": true,
  "avatarUrl": null,
  "bio": "Dev backend"
}
```

**Response 201:** corpo no formato `UserResponse` (id, name, username, email, role, isActive, avatarUrl, bio, createdAt, updatedAt); header `Location` apontando para `GET /users/{id}`.

### GET /users (Admin)

**Query:** `?pageNumber=1&pageSize=10&name=joão&username=joao&email=example&role=user&isActive=true`

**Response 200:**

```json
{
  "items": [
    {
      "id": "...",
      "name": "João Silva",
      "username": "joao",
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

**Response 204:** conta em soft delete.

### GET /users/{id}, PUT /users/{id}, DELETE /users/{id}

Apenas **Admin**. GET/PUT retornam `UserResponse`; DELETE retorna 204.

---

## Health, OpenAPI e descoberta

- **Health**: `/health` (liveness), `/health/ready` (readiness; inclui verificação do banco).
- **OpenAPI**: `/openapi/v1.json` (habilitado fora de produção ou quando `EnableOpenApi=true`).
- **Documentação interativa**: Scalar em `/scalar/v1` (mesma condição que OpenAPI).
- **OIDC**: `GET /.well-known/openid-configuration` retorna issuer, `jwks_uri`, etc., para integração com API Gateway (JWT authorizer) ou clientes OIDC.
- **JWKS**: `GET /.well-known/jwks.json` (cache 60s); apenas chaves públicas, algoritmo RS256.
- **Discovery da API**: `GET /api/discovery` retorna nome do serviço, basePath, URLs de OpenAPI e health.

---

## Observabilidade

- **Logging**: Console e Debug; níveis configuráveis em `Logging:LogLevel`.
- **Métricas**: OpenTelemetry (Fcg.Users.Api); contadores para eventos como criação/remoção de usuário (`FcgMeters`).
- **Tracing**: OpenTelemetry configurado via `AddOpenTelemetryObservability` (configurável).
- **Forwarded headers**: `X-Forwarded-For` e `X-Forwarded-Proto` para uso atrás de proxy/load balancer.

---

## Pacotes principais (referência)

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

---

Para detalhes dos workflows (secrets, variables, payload do orquestrador), consulte **`.github/workflows/README.md`**.
