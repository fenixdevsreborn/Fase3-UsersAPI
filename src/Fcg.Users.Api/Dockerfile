# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Fcg.Users.Api/Fcg.Users.Api.csproj", "src/Fcg.Users.Api/"]
COPY ["src/Fcg.Users.Application/Fcg.Users.Application.csproj", "src/Fcg.Users.Application/"]
COPY ["src/Fcg.Users.Domain/Fcg.Users.Domain.csproj", "src/Fcg.Users.Domain/"]
COPY ["src/Fcg.Users.Contracts/Fcg.Users.Contracts.csproj", "src/Fcg.Users.Contracts/"]
COPY ["src/Fcg.Users.Infrastructure/Fcg.Users.Infrastructure.csproj", "src/Fcg.Users.Infrastructure/"]
RUN dotnet restore "./src/Fcg.Users.Api/Fcg.Users.Api.csproj"
COPY . .
WORKDIR "/src/src/Fcg.Users.Api"
RUN dotnet build "./Fcg.Users.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Fcg.Users.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Fcg.Users.Api.dll"]