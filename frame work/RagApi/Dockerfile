# ──────────────────────────────────────────────
#  Stage 1: Build
# ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project file and restore dependencies (layer caching)
COPY RagApi.csproj .
RUN dotnet restore

# Copy everything else and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish --no-restore

# ──────────────────────────────────────────────
#  Stage 2: Runtime
# ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copy published output
COPY --from=build /app/publish .

# Copy Documents folder (knowledge base)
COPY Documents/ ./Documents/

# Expose the API port
EXPOSE 5000

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5000
ENV ASPNETCORE_ENVIRONMENT=Production



ENTRYPOINT ["dotnet", "RagApi.dll"]
