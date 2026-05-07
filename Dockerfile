FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ms-users.csproj", "./"]
RUN dotnet restore "ms-users.csproj"

COPY . .
RUN dotnet build "ms-users.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ms-users.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ms-users.dll"]