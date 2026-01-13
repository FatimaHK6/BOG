# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run Commands

### Backend (.NET 8)
```bash
# Build solution
dotnet build src/Backend/BOG.sln

# Run API (from solution root)
dotnet run --project src/Backend/BOG.API

# Run with watch mode
dotnet watch run --project src/Backend/BOG.API
```

### Frontend (Angular 13)
```bash
# Install dependencies
cd src/Frontend/bog-app && npm install

# Run development server (http://localhost:4200)
cd src/Frontend/bog-app && ng serve

# Build for production
cd src/Frontend/bog-app && ng build --configuration production

# Run tests
cd src/Frontend/bog-app && ng test
```

### Database (EF Core)
```bash
# Add migration
dotnet ef migrations add <MigrationName> --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API

# Update database
dotnet ef database update --project src/Backend/BOG.DbModel --startup-project src/Backend/BOG.API
```

## Architecture

This is a Legal Case Management System (مشروع إدارة الدعاوى) with clean architecture following SOLID principles.

### Backend Layers (src/Backend/)

```
BOG.API          → ASP.NET Core Web API entry point, DI setup, controllers
    ↓
BOG.BL           → Business logic services (IUserBL → UserBL)
    ↓
BOG.DAL          → Data access, generic Repository<T>, Unit of Work pattern
    ↓
BOG.DbModel      → EF Core DbContext, entities inheriting BaseEntity, migrations
```

Supporting projects:
- **BOG.DTO** - Data Transfer Objects for API input (UserCreateDTO, UserUpdateDTO)
- **BOG.VM** - View Models for API output with computed properties (UserVM.FullName)
- **BOG.Integration** - External service integrations (placeholder)

### Key Patterns

- **Repository Pattern**: Generic `IRepository<T>` with `Repository<TEntity>` implementation
- **Unit of Work**: Transaction management via `IUnitOfWork`
- **Soft Delete**: All entities have `IsDeleted` flag, queries filter deleted records
- **Base Entity**: All entities inherit `BaseEntity` (Id, CreatedDate, ModifiedDate, IsDeleted)

### Database

- SQL Server LocalDB: `Server=(localdb)\mssqllocaldb;Database=BOG`
- Connection string in `src/Backend/BOG.API/appsettings.json`

### API Base URL

- Development: `https://localhost:5001` or `http://localhost:5000`
- Swagger UI available at `/swagger`
- CORS enabled for Angular frontend at `http://localhost:4200`

## Dependency Injection

Services registered in `BOG.API/Extensions/ServiceCollectionExtensions.cs`:
- DbContext, repositories, and business logic services
- Add new services following existing pattern: Interface in BL → Implementation in BL → Register in extensions
