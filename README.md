# Personal Expense Tracker

## Overview

Personal income and expense tracking, account management, transfers, and monthly budgets. The Vue interface runs against a real ASP.NET Core API and PostgreSQL database; the dashboard does not use sample data. Phases 2–15 were implemented in order. Transactions (5), transfers (6), and the dashboard (8) were developed and verified separately. Implementation record: [docs/PHASES.md](docs/PHASES.md).

Features include registration, login, logout, a read-only profile, income and expense CRUD, filtering, sorting, pagination, account and category management, transfers between accounts, monthly category budgets, and a responsive dashboard. Forms include loading, error, empty, confirmation, and operation feedback states.

The interface supports Turkish (default) and English. Language selectors are available on the login and registration screens and throughout the signed-in application, including mobile layouts. The selection takes effect immediately and is remembered in browser local storage. Dates, amounts, percentages, and default category names follow the selected language. User-entered names and descriptions are preserved. Changing the interface language does not change the account currency or financial time zone. Translation messages are maintained in `frontend/src/i18n/en.json`, using Turkish source text as keys.

## Architecture

```text
Domain          -> no dependencies
Application     -> Domain
Infrastructure  -> Application -> Domain (transitive)
Api             -> Application + Infrastructure
```

Domain contains business rules; Application contains use cases, DTOs, validation, and abstractions; Infrastructure contains PostgreSQL/EF mappings, migrations, hashing, and JWT generation; API handles HTTP, authentication, and composition. Controllers call Application services.

Application deliberately uses the provider-independent EF Core DbSet/LINQ API through IAppDbContext; it is not ORM-independent. The concrete DbContext, Npgsql provider, and database configuration live in Infrastructure. Domain has no framework dependencies. No unnecessary repository or MediatR layer was added.

Financial totals are calculated through backend SQL aggregate/projection queries. The account list uses one SQL query, and the dashboard summary uses at most five; query counts are tested. Missing months in the monthly chart are filled using at most 12 summary rows returned by SQL. The frontend only calculates chart geometry and display formatting.

### Financial Rules

- Monetary fields use PostgreSQL `numeric(18,2)` / C# decimal. Income, expense, transfer, and budget amounts must be positive with at most two decimal places. Opening balances may be negative; negative balances are allowed.
- The MVP user currency is TRY. Accounts must use the same currency, and their currency cannot be changed. The model also supports USD/EUR codes; there is no currency conversion or user currency settings screen.
- Current balance = opening balance + income − expenses + incoming transfers − outgoing transfers. Current balances are not stored in the database. Inactive accounts are included in the total balance but cannot be used for new or updated transactions. Account DELETE deactivates an account instead of physically deleting it.
- A transfer is an immutable record separate from income and expenses. It connects two different, active accounts belonging to the same user and using the same currency within a serializable database transaction. Partial transfers cannot occur. Transfers do not contribute to total income, expenses, or budget spending. There are no transfer update/delete endpoints.
- Financial dates use DateOnly, and audit timestamps use UTC. Future-dated records can be created and appear in lists, but they do not contribute to balances, the dashboard, or budget spending until their date arrives. The default financial time zone is Europe/Istanbul.
- Budgets can only be assigned to expense categories, once per user/category/month/year. Remaining amounts can be negative, and percentages can exceed 100. The dashboard netBalance is this month's income minus expenses.
- Registration atomically creates 15 default categories with the user. System categories cannot be changed or deleted. Custom category types cannot be changed after creation; deleting a linked category returns 409.

## Tech Stack

.NET 10, ASP.NET Core, EF Core 10, Npgsql, PostgreSQL 17, FluentValidation, JWT; Vue 3 Composition API, strict TypeScript, Vite, Pinia, Vue Router, Axios, plain CSS, and SVG charts. xUnit, integration tests against real PostgreSQL, and Playwright Chromium E2E tests. Exact versions are recorded in project and lock files.

## Requirements

.NET 10 SDK, Node.js 24 with npm, and PostgreSQL 17. Docker is optional. Unless stated otherwise, commands target PowerShell from the repository root. The development API port is 5080; the frontend port is 5173.

## Local PostgreSQL

Enter your existing PostgreSQL username and password in the connection string. The local development default is:

```text
Host=localhost;Port=5432;Database=ExpenseTracker;Username=postgres;Password=postgres
```

Run `CREATE DATABASE "ExpenseTracker";` in PostgreSQL if needed. Do not use the default local password in production. The test user needs CREATEDB permission to create separate test databases.

## Docker PostgreSQL

```powershell
docker compose up -d postgres
docker compose ps
```

Only PostgreSQL runs in a container. Its port is exposed on 127.0.0.1:5432, and data is stored in a named volume. Do not start both options if local PostgreSQL already uses this port. `docker compose down` stops the container; `down -v` deletes its data. The Compose configuration is provided but was not verified by running Docker in this environment.

## Backend Setup

```powershell
dotnet tool restore
dotnet restore backend/ExpenseTracker.sln
$env:ASPNETCORE_ENVIRONMENT = 'Development'

# Run once: generate a local JWT signing key and store it in user-secrets.
$jwtBytes = New-Object byte[] 48
$jwtRng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$jwtRng.GetBytes($jwtBytes)
$jwtRng.Dispose()
$jwtSecret = [Convert]::ToBase64String($jwtBytes)
dotnet user-secrets set 'Jwt:Key' $jwtSecret --project backend/src/ExpenseTracker.Api

# If your connection differs from the default, configure your actual local connection:
# dotnet user-secrets set 'ConnectionStrings:DefaultConnection' 'YOUR_CONNECTION_STRING' --project backend/src/ExpenseTracker.Api

dotnet ef database update --project backend/src/ExpenseTracker.Infrastructure --startup-project backend/src/ExpenseTracker.Api
dotnet build backend/ExpenseTracker.sln --no-restore
dotnet run --project backend/src/ExpenseTracker.Api --launch-profile http
```

The JWT key must contain at least 32 UTF-8 bytes; no key is stored in the repository. Invalid or missing required settings cause startup to fail. The backend does not automatically read `.env` files.

## Frontend Setup

In a separate terminal:

```powershell
cd frontend
Copy-Item .env.example .env
npm install
npm run dev
```

Frontend: http://localhost:5173. Create a new account; no preset demo password is provided. Restart Vite after changing `.env`. The `npm run build` output is written to `frontend/dist`. On static hosting, route Vue Router history URLs to `index.html`.

## Environment Variables

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings__DefaultConnection` | PostgreSQL connection |
| `Jwt__Key` | Secret signing key; user-secrets is preferred for development |
| `Jwt__Issuer` / `Jwt__Audience` | Defaults: ExpenseTracker / ExpenseTracker.Web |
| `Jwt__ExpirationMinutes` | Default: 15; valid range: 1–60 |
| `Finance__TimeZone` | Default: Europe/Istanbul |
| `Cors__AllowedOrigins__0` | Frontend origin; development: http://localhost:5173 |
| `AllowedHosts` | API hostnames; separate multiple values with `;` |
| `ASPNETCORE_ENVIRONMENT` | Development / Production |
| `ASPNETCORE_URLS` | HTTP server listening addresses |
| `VITE_API_BASE_URL` | http://localhost:5080; do not append /api |
| `VITE_FINANCIAL_TIME_ZONE` | Must match backend Finance:TimeZone |
| `TEST_POSTGRES_CONNECTION` | Administrative database connection for integration tests |

`VITE_` values are exposed to the browser and must not contain secrets. The production connection string and CORS list are empty by default. Production TLS, secret management, AllowedHosts, and trusted reverse proxy configuration must be set up in the hosting environment.

JWTs are held in browser memory; tokens are not stored in localStorage or sessionStorage. Refreshing the page or token expiration requires logging in again. Only the interface language preference is persisted in localStorage. Logout clears the client session; there is no server-side token revocation or refresh token. Passwords are hashed with PBKDF2. The API enforces fallback authorization, ownership filters, owner-qualified foreign keys, authentication rate limiting, and restricted CORS. Errors return ProblemDetails; stack traces, passwords, and tokens are not written to application logs. Package scans are not a substitute for a comprehensive security audit.

## EF Core Migration Commands

Six migrations exist: InitialCreate, AddCategories, AddAccounts, AddTransactions, AddTransfers, and AddBudgets. Do not add InitialCreate again.

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet ef database update --project backend/src/ExpenseTracker.Infrastructure --startup-project backend/src/ExpenseTracker.Api
dotnet ef migrations has-pending-model-changes --project backend/src/ExpenseTracker.Infrastructure --startup-project backend/src/ExpenseTracker.Api
# Only when the model changes in the future:
# dotnet ef migrations add DescribeYourChange --project backend/src/ExpenseTracker.Infrastructure --startup-project backend/src/ExpenseTracker.Api --output-dir Persistence/Migrations
```

Neither EnsureCreated nor automatic Migrate is called at startup. Apply production migrations as a controlled deployment step. Foreign key delete behavior protects financial history; composite foreign keys preserve account/category ownership with the user. Unique constraints cover normalized email, category name/type, and monthly budgets. Indexes support ownership, date, and relationship queries.

## Running

API: http://localhost:5080. Application: http://localhost:5173. `/health/live` checks process health, and `/health/ready` checks database connectivity; readiness does not verify that the schema is up to date. For HTTPS development, run `dotnet dev-certs https --trust`, then use `--launch-profile https` and set the frontend API URL to https://localhost:7080. The `npm run preview` port, 4173, requires an additional CORS origin.

| API | Purpose |
| --- | --- |
| POST /api/auth/register, /api/auth/login | Anonymous registration/login |
| GET /api/auth/me | Current user's profile |
| /api/accounts, /api/categories, /api/transactions, /api/budgets | GET list/id, POST, PUT id, DELETE id |
| /api/transfers | POST, GET list/id |
| GET /api/dashboard/summary | Balance, monthly income/expenses, budget summary |
| GET /api/dashboard/monthly?months=6 | Summary for 6 or 12 months |
| GET /api/dashboard/category-expenses | This month's category breakdown |
| GET /api/dashboard/recent-transactions | Last 10 realized income/expense records |
| GET /api/dashboard/budgets | This month's budgets |

DTO fields use camelCase. Enums are strings: Income/Expense and Cash/Bank/CreditCard/Savings/Other. Financial dates use YYYY-MM-DD; audit timestamps use UTC ISO 8601. Pagination returns `items/page/pageSize/totalCount/totalPages`. Transaction filters are `accountId`, `categoryId`, `type`, `startDate`, `endDate`, `minAmount`, `maxAmount`, and `search`; sorting uses `sortBy=transactionDate|amount|createdAt` and `sortDirection=asc|desc`. The maximum `pageSize` is 100. Budget lists accept `month/year`.

Successful creation returns 201; deletion/deactivation returns 204. Validation errors return 400, authentication failures 401, missing records or records owned by another user 404, relationship/uniqueness conflicts 409, and rate limits 429. Clients cannot change ownership by submitting UserId.

## Swagger

Development only: http://localhost:5080/swagger and `/openapi/v1.json`. Enter the token from the registration/login response into Swagger's Authorize field. Documentation is disabled in production.

## Testing

```powershell
dotnet restore backend/ExpenseTracker.sln --locked-mode
dotnet build backend/ExpenseTracker.sln --no-restore
# Set your own test PostgreSQL connection if needed:
$env:TEST_POSTGRES_CONNECTION = 'Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres'
dotnet test backend/ExpenseTracker.sln --no-build --no-restore
```

There are 43 backend tests: 6 Domain, 7 Application, and 30 integration tests. Integration tests create their own randomly named `expense_tests_*` databases on real PostgreSQL, apply migrations, and delete only the databases they created when finished. EF InMemory/SQLite is not used. Without PostgreSQL, you can run only the Domain.Tests and Application.Tests projects.

Coverage includes user isolation, anonymous access/JWT signature-issuer-audience-expiry, overposting, transaction filters/balances, rollback after a transfer INSERT failure, budget uniqueness and concurrent creation, cross-owner foreign key rejection, future dates and dashboard aggregate accuracy, and query counts.

```powershell
cd frontend
npm install
npx playwright install chromium
npm run build
npm run type-check
npm run format:check
# The API must be running against a development database; tests start or reuse Vite.
npm run test:e2e
# Language tests mock the API and do not require a running backend:
npm run test:e2e -- tests/i18n.spec.ts
```

Three Chromium E2E scenarios against the real API verify authentication/session behavior, transaction CRUD/filtering, and account/transfer/category/budget/dashboard flows. These tests create unique users and records in the development database; do not use a production connection. They check desktop and 390px mobile layouts, overflow, runtime errors, and modal Escape/focus behavior. Additional language tests cover switching, persistence, English navigation and forms, default categories, and mobile access using mocked API responses. Screenshots are stored in `frontend/test-results`. There is no separate lint script; strict TypeScript and Prettier checks are used.

Previous local baseline verification: backend built with 0 warnings/errors and passed 43/43 tests; frontend build/type-check/format and 3/3 E2E tests passed. NuGet and npm scans reported no known vulnerabilities. A real PostgreSQL 17.11 test server was used at 127.0.0.1:55432 during that verification; the standard setup defaults to 5432.

## Supabase Migration

There is no Supabase SDK or Supabase-specific domain code. The same Npgsql provider and migrations are used; changing `ConnectionStrings__DefaultConnection` switches the application's target PostgreSQL server. Add the target's SSL/pooling parameters to the connection and use a connection with DDL support and appropriate permissions for migrations. Applying migrations to the target and moving existing data are separate operational steps; a connection string does not migrate data.

2026-09-06: A real Supabase Session pooler connection was enabled. All six migrations, API health, registration/login, and the dashboard were verified. Tables are in the private `expense_tracker` schema. Local data was not migrated; full TLS certificate validation and deployment were not completed. Details: [Supabase connection](docs/SUPABASE.md).

## Project Structure

```text
backend/
  ExpenseTracker.sln
  src/
    ExpenseTracker.Domain/{Common,Entities}
    ExpenseTracker.Application/{Abstractions,Common,Features}
    ExpenseTracker.Infrastructure/{Authentication,Persistence}
    ExpenseTracker.Api/{Authentication,Configuration,Controllers,Middleware}
  tests/
    ExpenseTracker.Domain.Tests/
    ExpenseTracker.Application.Tests/
    ExpenseTracker.IntegrationTests/
frontend/
  src/{api,components,composables,features,i18n,layouts,router,stores,types,utils,views}
  tests/
  playwright.config.ts
  .env.example
docs/PHASES.md
docker-compose.yml
dotnet-tools.json
global.json
```

## Known Limitations

Live deployment, Docker execution, and load testing have not been performed. The real Supabase connection was verified separately; see docs/SUPABASE.md. Profiles are read-only, and the MVP uses TRY as the single user currency. Refresh tokens, server-side token revocation, password reset, email verification, 2FA, and transfer modification/cancellation are not implemented. Negative balances are possible. Authentication rate limits are held in process memory; deployments with multiple instances require consideration of distributed limits or a proxy. These checks do not replace production operations or independent security testing.

## Future Improvements

Recurring transactions, subscriptions, savings goals, multiple currencies and exchange rates, receipt upload/OCR, CSV import/export, Excel reports, advanced analytics, notifications, dark mode, PWA/mobile, Google authentication, 2FA, refresh/session management, and transfer correction workflows.
