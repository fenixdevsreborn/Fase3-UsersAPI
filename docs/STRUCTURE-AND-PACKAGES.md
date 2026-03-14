# Fase3-UsersAPI — Estrutura e pacotes

## Árvore final de pastas (principais)

```
Fase3-UsersAPI/
├── src/
│   ├── Fcg.Users.Api/
│   │   ├── Authentication/
│   │   │   └── JwtBearerExtensions.cs          # AddFcgJwtBearer (validação JWT)
│   │   ├── Authorization/
│   │   │   ├── AuthorizationExtensions.cs      # AddFcgAuthorization, RequireScopePolicyName
│   │   │   ├── UserClaimsExtensions.cs        # GetUserId, GetRole, IsAdmin, HasScope
│   │   │   └── OwnerAuthorization.cs          # CanAccessResource
│   │   ├── Observability/
│   │   │   ├── ObservabilityOptions.cs
│   │   │   ├── ObservabilityContext.cs        # GetCurrentTraceId, CorrelationId (Activity)
│   │   │   ├── IObservabilityContextAccessor.cs
│   │   │   ├── ObservabilityContextAccessor.cs
│   │   │   ├── FcgLogPropertyNames.cs
│   │   │   ├── FcgMetricNames.cs               # HTTP + users.created/deleted + exceptions
│   │   │   ├── FcgMeters.cs                    # Meter facade
│   │   │   ├── CorrelationIdMiddleware.cs     # X-Correlation-ID
│   │   │   ├── HttpMetricsMiddleware.cs      # request count/duration/active
│   │   │   ├── ExceptionObservabilityMiddleware.cs  # log + exceptions.count, rethrow
│   │   │   ├── ObservabilityServiceCollectionExtensions.cs  # AddProjectObservability
│   │   │   └── ObservabilityApplicationBuilderExtensions.cs # UseFcgObservability
│   │   ├── Middleware/
│   │   │   └── ExceptionHandlingMiddleware.cs # Domain ex → HTTP JSON response
│   │   ├── Extensions/
│   │   │   └── ServiceCollectionExtensions.cs  # AddUsersApiAuth, AddUsersApiObservability
│   │   ├── OpenApi/
│   │   │   └── BearerSecuritySchemeTransformer.cs  # Scalar Bearer JWT
│   │   ├── Controllers/
│   │   │   ├── UsersController.cs
│   │   │   └── AuthController.cs
│   │   ├── Program.cs
│   │   └── Program.IntegrationTests.cs
│   ├── Fcg.Users.Application/
│   ├── Fcg.Users.Domain/
│   ├── Fcg.Users.Infrastructure/
│   │   ├── Services/
│   │   │   └── JwtTokenService.cs              # Geração JWT (usa Contracts.Auth)
│   │   └── Extensions/
│   └── Fcg.Users.Contracts/
│       └── Auth/
│           ├── FcgClaimTypes.cs
│           ├── FcgRoles.cs
│           ├── FcgScopes.cs
│           ├── FcgPolicies.cs
│           ├── JwtOptions.cs
│           ├── LoginRequest.cs
│           └── LoginResponse.cs
├── tests/
│   ├── Fcg.Users.UnitTests/
│   │   ├── Authorization/
│   │   │   └── UserClaimsExtensionsTests.cs
│   │   ├── Observability/
│   │   │   └── FcgMetersTests.cs
│   │   └── Services/
│   └── Fcg.Users.IntegrationTests/
└── docs/
    └── STRUCTURE-AND-PACKAGES.md
```

## Comandos NuGet necessários

Pacotes já referenciados nos `.csproj`; para adicionar manualmente (caso precise recriar):

**Fcg.Users.Api:**

```bash
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.3
dotnet add package Microsoft.AspNetCore.OpenApi --version 10.0.3
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore --version 10.0.0
dotnet add package Microsoft.OpenApi --version 2.0.0
dotnet add package Scalar.AspNetCore --version 2.13.6
```

**Fcg.Users.Infrastructure:**

```bash
dotnet add package BCrypt.Net-Next --version 4.0.3
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer --version 10.0.3
dotnet add package Microsoft.EntityFrameworkCore --version 10.0.0
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 10.0.0
```

**Testes:**

```bash
# UnitTests
dotnet add package Moq --version 4.20.72
dotnet add package xunit --version 2.9.3
dotnet add package Microsoft.NET.Test.Sdk --version 17.14.1

# IntegrationTests
dotnet add package Microsoft.AspNetCore.Mvc.Testing --version 10.0.3
dotnet add package xunit --version 2.9.3
```

Nenhum pacote do Fase3-Shared é necessário; toda a autenticação, autorização e observabilidade estão internalizadas no repositório.
