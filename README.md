# ExpenseManager API

A REST API for managing employee expenses with an approval workflow, built with Clean Architecture, CQRS, and ASP.NET Core 8.

## Tech Stack

- **.NET 8** (LTS) — ASP.NET Core Web API
- **Entity Framework Core 8** with **PostgreSQL**
- **MediatR** — CQRS (commands/queries separated with handlers)
- **FluentValidation** — request validation via MediatR pipeline behavior
- **AutoMapper** — entity-to-DTO mapping
- **ASP.NET Core Identity** — user management with JWT bearer authentication
- **Serilog** — structured logging
- **Swagger/OpenAPI** — interactive API documentation
- **xUnit + NSubstitute + FluentAssertions** — unit and integration tests

## Architecture

```
┌──────────────────────────────────────────────────────┐
│                    API Layer                          │
│  Controllers · Middleware · Auth · Swagger            │
├──────────────────────────────────────────────────────┤
│                Application Layer                     │
│  MediatR Handlers · Commands · Queries · Validators  │
├──────────────────────────────────────────────────────┤
│               Infrastructure Layer                   │
│  EF Core · Repositories · Identity · Services        │
├──────────────────────────────────────────────────────┤
│                 Domain Layer                         │
│  Entities · Value Objects · Enums · Interfaces       │
└──────────────────────────────────────────────────────┘
```

**Dependency rule**: each layer only depends on the layer below it. Domain has zero external dependencies.

- **Domain** — `Expense` and `Category` entities with private setters and encapsulated state transitions (`Submit`, `Approve`, `Reject`). `Money` value object. Repository interfaces defined here.
- **Application** — Feature-organized MediatR handlers. Every command/query returns `Result<T>` instead of throwing exceptions. `ValidationBehavior` pipeline runs FluentValidation before handlers.
- **Infrastructure** — EF Core `DbContext`, repository implementations, Identity setup, `AuditableEntitySaveChangesInterceptor` for automatic `CreatedAtUtc`/`UpdatedAtUtc` timestamps.
- **API** — Thin controllers that send MediatR commands and map `Result<T>` to HTTP responses. Global exception middleware returns `ProblemDetails`.

## Expense Workflow

```
Draft ──Submit──▶ Submitted ──Approve──▶ Approved
                      │
                      └──Reject───▶ Rejected
```

- Employees create expenses in **Draft** status and can edit/delete/submit them.
- Managers can **approve** or **reject** submitted expenses (with a reason for rejection).
- Self-approval is blocked at the domain level.

## Running Locally

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [PostgreSQL](https://www.postgresql.org/download/) (or use Docker)

### With Docker (recommended)

```bash
docker compose up --build
```

The API will be available at `http://localhost:8080`. Swagger UI at `http://localhost:8080/swagger`.

### Without Docker

1. Start a PostgreSQL instance and update the connection string in `src/ExpenseManager.API/appsettings.json`.

2. Run the API:

```bash
dotnet run --project src/ExpenseManager.API
```

The API auto-applies EF Core migrations and seeds roles + categories on startup in Development mode.

## Running Tests

```bash
dotnet test
```

Tests use an in-memory SQLite database — no PostgreSQL required.

**75 tests** across three projects:

| Project | Count | What's tested |
|---|---|---|
| Domain.Tests | 40 | Entity state transitions, guard clauses, value object equality |
| Application.Tests | 22 | MediatR handlers (mocked repos), FluentValidation validators |
| API.Tests | 13 | Integration tests via `WebApplicationFactory` (auth, CRUD, role checks) |

## API Endpoints

### Auth

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| POST | `/api/auth/register` | Register a new user | No |
| POST | `/api/auth/login` | Login and receive JWT tokens | No |
| POST | `/api/auth/refresh` | Refresh an expired access token | No |

### Expenses

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/expenses` | List expenses (paginated, filterable) | Bearer |
| GET | `/api/expenses/{id}` | Get expense by ID | Bearer |
| POST | `/api/expenses` | Create a draft expense | Bearer |
| PUT | `/api/expenses/{id}` | Update a draft expense | Bearer |
| DELETE | `/api/expenses/{id}` | Delete a draft expense | Bearer |
| POST | `/api/expenses/{id}/submit` | Submit for approval | Bearer |
| POST | `/api/expenses/{id}/approve` | Approve (Manager only) | Bearer |
| POST | `/api/expenses/{id}/reject` | Reject with reason (Manager only) | Bearer |

### Categories

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/categories` | List all categories | Bearer |
| GET | `/api/categories/{id}` | Get category by ID | Bearer |
| POST | `/api/categories` | Create category (Manager only) | Bearer |
| PUT | `/api/categories/{id}` | Update category (Manager only) | Bearer |
| DELETE | `/api/categories/{id}` | Delete category (Manager only) | Bearer |

### Reports

| Method | Endpoint | Description | Auth |
|---|---|---|---|
| GET | `/api/reports/summary` | Expense summary with status counts | Bearer |
| GET | `/api/reports/by-category/{id}` | Expenses grouped by category | Bearer |
| GET | `/api/reports/monthly/{year}/{month}` | Monthly report by category | Bearer |

## Project Structure

```
ExpenseManager/
├── src/
│   ├── ExpenseManager.Domain/            # Entities, value objects, interfaces (zero dependencies)
│   ├── ExpenseManager.Application/       # MediatR handlers, DTOs, validators, Result<T>
│   ├── ExpenseManager.Infrastructure/    # EF Core, repositories, Identity, services
│   └── ExpenseManager.API/              # Controllers, middleware, auth, Swagger
├── tests/
│   ├── ExpenseManager.Domain.Tests/
│   ├── ExpenseManager.Application.Tests/
│   └── ExpenseManager.API.Tests/
├── Directory.Build.props                 # Shared build settings (net8.0, nullable, warnings-as-errors)
├── Dockerfile                            # Multi-stage build
├── docker-compose.yml                    # API + PostgreSQL
└── ExpenseManager.sln
```
