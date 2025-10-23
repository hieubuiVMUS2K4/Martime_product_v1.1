Backend API (ASP.NET Core 8)

Commands:
- dotnet restore
- dotnet run
- dotnet ef migrations add Initial (if you have EF tools)

Configuration:
- Uses appsettings.json and environment variables for ConnectionStrings and JWT
- Example endpoint: GET /api/ships

Docker:
- docker build -t product-backend ./backend
- docker run -p 5000:5000 --env-file .env product-backend
