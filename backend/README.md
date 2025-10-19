## App DB migrations
dotnet ef migrations add InitialCreate --context AppDbContext --output-dir Data/Migrations/
dotnet ef database update --context AppDbContext