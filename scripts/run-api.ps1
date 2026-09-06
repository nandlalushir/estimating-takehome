$ErrorActionPreference = "Stop"
docker compose up -d
dotnet restore
dotnet test
dotnet run --project src/CostEstimating.Api
