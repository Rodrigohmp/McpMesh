FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY McpMesh.csproj .
RUN dotnet restore
COPY . .
RUN dotnet build "McpMesh.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "McpMesh.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Install Node.js for MCP servers that require it
RUN apt-get update && \
    apt-get install -y curl && \
    curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs && \
    rm -rf /var/lib/apt/lists/*

ENTRYPOINT ["dotnet", "McpMesh.dll"]