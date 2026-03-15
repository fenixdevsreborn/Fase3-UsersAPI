# Build — context = raiz do repositório do serviço.
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY src/Fcg.Users.Api/Fcg.Users.Api.csproj src/Fcg.Users.Api/
COPY src/Fcg.Users.Application/Fcg.Users.Application.csproj src/Fcg.Users.Application/
COPY src/Fcg.Users.Contracts/Fcg.Users.Contracts.csproj src/Fcg.Users.Contracts/
COPY src/Fcg.Users.Domain/Fcg.Users.Domain.csproj src/Fcg.Users.Domain/
COPY src/Fcg.Users.Infrastructure/Fcg.Users.Infrastructure.csproj src/Fcg.Users.Infrastructure/
COPY src/Fcg.Users.ServiceDefaults/Fcg.Users.ServiceDefaults.csproj src/Fcg.Users.ServiceDefaults/

RUN dotnet restore src/Fcg.Users.Api/Fcg.Users.Api.csproj
COPY src src
RUN dotnet publish src/Fcg.Users.Api/Fcg.Users.Api.csproj -c Release -o /app/publish --no-restore

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Fcg.Users.Api.dll"]
