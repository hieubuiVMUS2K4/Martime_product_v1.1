# Build context = repository root (để include shared/ project)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY shared/Maritime.Shared.csproj shared/
COPY backend/product-api.csproj backend/
RUN dotnet restore backend/product-api.csproj

COPY shared/ shared/
COPY backend/ backend/
RUN dotnet publish backend/product-api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish ./
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "product-api.dll"]
