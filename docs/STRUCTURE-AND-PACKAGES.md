# Fase3-UsersAPI — Estrutura e pacotes

Documentação de arquitetura JWT RS256, OIDC e JWKS: **[docs/JWT-RS256-OIDC-JWKS.md](JWT-RS256-OIDC-JWKS.md)**.

## Árvore final de pastas (principais)

```
Fase3-UsersAPI/
├── src/
│   ├── Fcg.Users.Api/
│   │   ├── Authentication/
│   │   │   ├── JwtBearerExtensions.cs          # AddFcgJwtBearer (validação JWT)
│   │   │   └── JwtBearerPostConfigureOptions.cs # RS256, IssuerSigningKeyResolver por kid
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
│   │   │   ├── AuthController.cs
│   │   │   └── OpenIdDiscoveryController.cs   # GET /.well-known/jwks.json
│   │   ├── Program.cs
│   │   └── Program.IntegrationTests.cs
│   ├── Fcg.Users.Application/
│   ├── Fcg.Users.Domain/
│   ├── Fcg.Users.Infrastructure/
│   │   ├── Authentication/
│   │   │   ├── IRsaKeyProvider.cs
│   │   │   ├── FileRsaKeyProvider.cs
│   │   │   ├── EnvironmentRsaKeyProvider.cs
│   │   │   ├── AwsParameterStoreRsaKeyProvider.cs
│   │   │   └── AwsSecretsManagerRsaKeyProvider.cs
│   │   ├── Services/
│   │   │   ├── JwtTokenService.cs              # Geração JWT RS256 (usa Contracts.Auth)
│   │   │   ├── IJwksService.cs, JwksService.cs
│   │   │   └── ...
│   │   └── Extensions/
│   └── Fcg.Users.Contracts/
│       └── Auth/
│           ├── FcgClaimTypes.cs
│           ├── FcgRoles.cs
│           ├── FcgScopes.cs
│           ├── FcgPolicies.cs
│           ├── JwtOptions.cs
│           ├── JwtSigningOptions.cs
│           ├── LoginRequest.cs
│           └── LoginResponse.cs
├── tests/
│   ├── Fcg.Users.UnitTests/
│   │   ├── Authorization/
│   │   │   └── UserClaimsExtensionsTests.cs
│   │   ├── Observability/
│   │   │   └── FcgMetersTests.cs
│   │   └── Services/
│   └── Fcg.Users.IntegrationTests/   # AuthIntegrationTests: login, discovery, jwks, validação manual; 1 teste pendente (ver JWT-RS256-OIDC-JWKS.md)
└── docs/
    ├── JWT-RS256-OIDC-JWKS.md      # Arquitetura JWT RS256, OIDC, JWKS; dev/prod; rotação; teste pendente
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
dotnet add package AWSSDK.Extensions.NETCore.Setup
dotnet add package AWSSDK.SimpleSystemsManagement
dotnet add package AWSSDK.SecretsManager
dotnet add package Microsoft.Extensions.Hosting.Abstractions --version 10.0.0
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
