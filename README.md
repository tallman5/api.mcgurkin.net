# api.mcgurkin.net
It's a REST API built using .NET, hosted in Azure and uses an Azure SQL Server database.

## Migrations and Updates
The database is managed using Entity Framework Core.

### Migration Commands
```
dotnet ef migrations add <Migration-Name> -p McGurkin.Api -c IamDbContext -o Features/Iam/Data/Migrations
```

### Update Commands
```
dotnet ef database update -c IamDbContext -p McGurkin.Api --connection "Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=mc-loc;Integrated Security=True"
```
