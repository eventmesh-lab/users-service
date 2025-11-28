FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy all .csproj files and restore dependencies (this layer will be cached)
# Order matters: domain -> application -> infrastructure -> api
COPY src/users-service.domain/users-service.domain.csproj ./src/users-service.domain/
COPY src/users-service.application/users-service.application.csproj ./src/users-service.application/
COPY src/users-service.infrastructure/users-service.infrastructure.csproj ./src/users-service.infrastructure/
COPY src/users-service.api/users-service.api.csproj ./src/users-service.api/

# Restore dependencies starting from the API project (it will restore all dependencies transitively)
WORKDIR /src/src/users-service.api
RUN dotnet restore

# Copy the rest of the source code
WORKDIR /src
COPY src/ ./src/

# Build and publish
WORKDIR /src/src/users-service.api
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish ./
EXPOSE 7181
ENTRYPOINT ["dotnet", "users-service.api.dll"]