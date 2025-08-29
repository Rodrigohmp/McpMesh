FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR "/src/McpMesh"

COPY ["McpMesh.csproj", "./"]
RUN dotnet restore "McpMesh.csproj"

COPY . .

RUN dotnet build "McpMesh.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "McpMesh.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Install system dependencies and external tools
RUN apt-get update && apt-get install -y \
    curl \
    nodejs \
    npm \
    python3 \
    python3-pip \
    && curl -LsSf https://astral.sh/uv/install.sh | sh \
    && rm -rf /var/lib/apt/lists/*

# Post-installation configuration
RUN mv /root/.local/bin/uv /usr/local/bin/ \
    && mv /root/.local/bin/uvx /usr/local/bin/ \
    && ln -s /usr/bin/python3 /usr/bin/python

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:80
ENV ASPNETCORE_ENVIRONMENT=Production

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:80/health/live || exit 1

ENTRYPOINT ["dotnet", "McpMesh.dll"]