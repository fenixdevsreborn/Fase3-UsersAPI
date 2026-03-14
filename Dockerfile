# Build — use context = FASE3 (repo root): docker build -f Fase3-UsersAPI/Dockerfile .
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY Fase3-UsersAPI/src/Fcg.Users.Api/Fcg.Users.Api.csproj Fase3-UsersAPI/src/Fcg.Users.Api/
COPY Fase3-UsersAPI/src/Fcg.Users.Application/Fcg.Users.Application.csproj Fase3-UsersAPI/src/Fcg.Users.Application/
COPY Fase3-UsersAPI/src/Fcg.Users.Contracts/Fcg.Users.Contracts.csproj Fase3-UsersAPI/src/Fcg.Users.Contracts/
COPY Fase3-UsersAPI/src/Fcg.Users.Domain/Fcg.Users.Domain.csproj Fase3-UsersAPI/src/Fcg.Users.Domain/
COPY Fase3-UsersAPI/src/Fcg.Users.Infrastructure/Fcg.Users.Infrastructure.csproj Fase3-UsersAPI/src/Fcg.Users.Infrastructure/

RUN dotnet restore Fase3-UsersAPI/src/Fcg.Users.Api/Fcg.Users.Api.csproj
COPY Fase3-UsersAPI/src Fase3-UsersAPI/src
RUN dotnet publish Fase3-UsersAPI/src/Fcg.Users.Api/Fcg.Users.Api.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Fcg.Users.Api.dll"]
