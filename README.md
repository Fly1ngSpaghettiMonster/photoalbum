# PhotoAlbum MVP

A scalable, multi-tier photo album web app MVP built with .NET and deployed on Azure App Service.

## Features

- User registration, login, logout (ASP.NET Core Identity)
- Authenticated photo upload and delete
- User-scoped photo list (each user only sees their own photos)
- Sorting by date or name, ascending/descending
- Photo detail page
- Storage provider abstraction (local or Azure Blob)

## Tech Stack

### Application

- **.NET 10** (`net10.0`)
- **Blazor Web App** with **Interactive Server** render mode
- **ASP.NET Core Identity** for auth
- **Entity Framework Core (SQL Server provider)**
- **Azure.Storage.Blobs** for cloud image storage

### Data + Storage

- **Azure SQL Database** (application + identity data)
- **Azure Blob Storage** (uploaded image binaries)

### Hosting + Delivery

- **Azure App Service (Linux)**
- **GitHub Actions** for CI/CD on push to `main`

## Architecture (MVP)

```text
Browser
  -> Blazor Web App (App Service, Linux)
      -> EF Core -> Azure SQL Database
      -> IPhotoStorage -> Azure Blob Storage (container: photos)
```

The app layer is stateless. Persistent data lives in Azure SQL and Blob Storage.

## Azure Resource Hierarchy

```text
Subscription
└── Resource Group: rg-photoalbum-dev-swe
    ├── App Service Plan (Linux): asp-photoalbum-dev-swe
    │   └── Web App: app-photoalbum-dev-<unique>
    ├── SQL Server (logical): sql-photoalbum-dev-swe
    │   └── SQL Database: sqldb-photoalbum-dev
    └── Storage Account: stphotoalbumdevswe<unique>
        └── Blob Container: photos
```

## Configuration

Set these as **Azure App Service Environment Variables**:

- `ConnectionStrings__DefaultConnection`
- `Storage__Provider` = `AzureBlob`
- `Storage__ConnectionString`
- `Storage__ContainerName` = `photos`
- `Storage__PublicBaseUrl` = `` (empty unless using custom CDN/domain)

Optional for production diagnostics:

- `Logging__LogLevel__Default` = `Information`
- `Logging__LogLevel__Microsoft.AspNetCore` = `Warning`
- `ASPNETCORE_FORWARDEDHEADERS_ENABLED` = `true`

## Local Development

### Prerequisites

- .NET 10 SDK
- SQL Server / LocalDB

### Run

From repository root:

```powershell
dotnet restore .\src\PhotoAlbum\PhotoAlbum.Web\PhotoAlbum.Web.csproj
dotnet run --project .\src\PhotoAlbum\PhotoAlbum.Web\PhotoAlbum.Web.csproj
```

### Notes

- EF Core migrations are applied automatically on startup (`Database.Migrate()` in startup pipeline).
- Local default storage provider is `Local` from `appsettings.json`.

## CI/CD

Workflow file:

- `.github/workflows/main_app-photoalbum-dev.yml`

Current flow:

1. Checkout
2. Restore/build/publish `src/PhotoAlbum/PhotoAlbum.Web`
3. Upload artifact
4. Deploy artifact to Azure Web App

## Security Notes

- Do not commit real secrets in `appsettings.json`.
- Store runtime secrets in Azure App Service environment variables.
- Keep SQL password and storage keys out of source control.

## MVP Status

Implemented and validated:

- Infrastructure provisioning on Azure
- App deployment and startup
- End-to-end flow: register/login, upload, list, detail, delete
- GitHub Actions deployment pipeline

## Next Recommended Steps

- Add a dedicated test project and CI test step
- Add health checks and alerts
- Move secrets to Azure Key Vault
- Add image resizing/thumbnails