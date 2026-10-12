# TechMove GLMS — Global Logistics Management System

A full-stack logistics management system for tracking clients, contracts, and service requests across global operations. Built as a monorepo with an ASP.NET Core MVC frontend, a JWT-secured Web API, and an xUnit test suite.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-MVC%20%7C%20Web%20API-512BD4)
![Tests](https://img.shields.io/badge/tests-39%20passing-brightgreen)
![Docker](https://img.shields.io/badge/docker-compose-2496ED?logo=docker)

---

## Overview

TechMove GLMS manages the full lifecycle of logistics contracts — from onboarding a client, through contract negotiation and activation, to the day-to-day service requests billed against each contract. Currency conversion (USD → ZAR) is handled live via a third-party exchange-rate API, and signed agreements are uploaded, validated, and stored server-side.

The solution demonstrates:

- **Clean separation of concerns** between the MVC frontend and the API
- **JWT authentication** for the API with cookie-based sessions for the MVC app
- **Repository and factory patterns** in the API layer
- **Integration + unit testing** with `WebApplicationFactory` and Moq
- **Dockerized deployment** with a SQL Server container

---

## Architecture

```
┌──────────────────────┐     HTTPS      ┌──────────────────────┐     EF Core     ┌────────────────┐
│  TechMoveGLMS-Web    │ ─────────────► │  TechMoveGLMS.API    │ ──────────────► │   SQL Server   │
│  ASP.NET Core MVC    │                │  ASP.NET Core Web API│                 │   (LocalDB /   │
│  Cookie auth         │ ◄───────────── │  JWT auth            │ ◄────────────── │    Docker)     │
└──────────────────────┘                └──────────────────────┘                 └────────────────┘
                                                    │
                                                    │ HTTP
                                                    ▼
                                       ┌──────────────────────┐
                                       │  ExchangeRate-API    │
                                       │  (USD → ZAR)         │
                                       └──────────────────────┘
```

| Project | Description |
|---|---|
| `TechMoveGLMS-Web` | ASP.NET Core MVC frontend — controllers, views, view models, session-based cookie auth. Talks to the API over HTTP using a shared `IApiService`. |
| `TechMoveGLMS.API` | ASP.NET Core Web API — JWT-issued tokens, EF Core data access, repository pattern, file upload service, currency conversion, Swagger UI. |
| `TechMoveGLMS.Tests` | xUnit test suite — unit tests (Moq) and integration tests (`WebApplicationFactory`). |

---

## Features

### Client Management
- Full CRUD for clients
- Region and contact tracking
- Contract count per client

### Contract Lifecycle
- Status workflow: `Draft → Active → On Hold → Expired`
- Service levels: `Basic`, `Standard`, `Premium`, `Enterprise`
- Date-range and status filtering
- **Signed agreement upload** — PDF only, validated on type and size
- Server-side storage with original filename tracking
- Download endpoint with JWT-protected access

### Service Requests
- Created against an active contract
- Auto-converted from USD to ZAR using the **live exchange rate**
- Stores the exact rate used at creation time for audit purposes
- Status workflow: `Pending → Approved → In Progress → Completed / Cancelled`

### Security
- **API:** JWT bearer tokens with issuer/audience/lifetime validation
- **Web:** Cookie authentication with sliding expiration
- No secrets in source control — all sensitive values via user-secrets or environment variables
- Password and key validation at startup (fails fast if misconfigured)

### DevOps
- `docker-compose` brings up SQL Server 2022, the API, and the Web app in one command
- Healthchecks ensure the API waits for SQL Server before starting
- `.env`-driven configuration for local Docker runs

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 9 |
| Frontend | ASP.NET Core MVC, Razor Views, Bootstrap 5 |
| API | ASP.NET Core Web API, Swagger / OpenAPI |
| Auth | JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`), cookie auth |
| Data | Entity Framework Core 9, SQL Server 2022 (LocalDB in dev) |
| External | [ExchangeRate-API](https://app.exchangerate-api.com/) |
| Testing | xUnit, Moq, `Microsoft.AspNetCore.Mvc.Testing` |
| Containerization | Docker, Docker Compose |

---

## Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server LocalDB (ships with Visual Studio) **or** a running SQL Server instance
- (Optional) [Docker Desktop](https://www.docker.com/products/docker-desktop/) for the containerized path
- (Optional) A free [ExchangeRate-API key](https://app.exchangerate-api.com/sign-up) — the app falls back to a default rate of 19.50 ZAR per USD if unavailable

### 1. Clone the repository

```bash
git clone https://github.com/AllyKat9/techmove-glms.git
cd techmove-glms
```

### 2. Configure local secrets

Real values **never** go in `appsettings.json`. Use user-secrets for local development.

From `TechMoveGLMS.API`:

```bash
cd TechMoveGLMS.API
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "<a-32+-character-random-string>"
dotnet user-secrets set "DemoCredentials:Password" "<your-demo-login-password>"
dotnet user-secrets set "ExchangeRateApi:ApiKey" "<your-exchangerate-api-key>"
```

Generate a JWT key with:
```bash
openssl rand -base64 32
```

Or PowerShell:
```powershell
[Convert]::ToBase64String((1..32 | ForEach-Object { Get-Random -Maximum 256 }))
```

### 3. Configure the Web app's API URL

Create `TechMoveGLMS-Web/appsettings.Development.json` with the port your API listens on locally (check `TechMoveGLMS.API/Properties/launchSettings.json`):

```json
{
  "Logging": { "LogLevel": { "Default": "Information" } },
  "ApiSettings": {
    "BaseUrl": "https://localhost:7061"
  }
}
```

This file is `.gitignore`d — it stays local.

### 4. Run migrations & start

```bash
dotnet ef database update --project TechMoveGLMS.API
```

Then run both apps (in separate terminals, or as multiple startup projects in Visual Studio):

```bash
dotnet run --project TechMoveGLMS.API
dotnet run --project TechMoveGLMS-Web
```

Browse to the Web app (see its `launchSettings.json`) and log in with:
- **Username:** `admin`
- **Password:** the value you set for `DemoCredentials:Password` above

### Docker (alternative)

Copy `.env.example` to `.env` and fill in real values:

```bash
cp .env.example .env
```

Then:

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| Web app | http://localhost:8080 |
| API | http://localhost:5000 |
| Swagger UI | http://localhost:5000/swagger |

---

## Testing

```bash
dotnet test
```

The suite contains **39 tests**:

- **Unit tests** — currency conversion math, file-validation edge cases, service-request factory business rules (`Moq`-driven)
- **Integration tests** — end-to-end HTTP tests against the API using `WebApplicationFactory`, covering auth, CRUD, filtering, and error paths

Integration tests authenticate against `/api/auth/login`. Set the demo password as an environment variable before running:

```bash
# Windows PowerShell
$env:TEST_ADMIN_PASSWORD = "<your-demo-login-password>"
dotnet test
```

```bash
# macOS / Linux
export TEST_ADMIN_PASSWORD="<your-demo-login-password>"
dotnet test
```

---

## Project Structure

```
techmove-glms/
├── TechMoveGLMS-Web/            # ASP.NET Core MVC frontend
│   ├── Controllers/
│   ├── Models/
│   ├── Services/                # IApiService + domain API services
│   ├── Views/
│   ├── Data/DTOs/
│   ├── Migrations/
│   └── wwwroot/
├── TechMoveGLMS.API/            # ASP.NET Core Web API
│   ├── Controllers/             # Auth, Clients, Contracts, ServiceRequests
│   ├── DTOs/
│   ├── Services/                # Repository, Factory, FileUpload, Currency
│   ├── wwwroot/uploads/contracts/
│   └── Program.cs
├── TechMoveGLMS.Tests/          # xUnit test suite
│   ├── Tests/Unit/
│   └── Tests/Integration/
├── docker-compose.yml
├── .env.example
├── .dockerignore
├── .gitignore
└── README.md
```

---

## API Reference

### Auth

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/login` | Issue a JWT for the demo user |

### Clients

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/clients` | List all clients |
| `GET` | `/api/clients/{id}` | Get a client by ID |
| `POST` | `/api/clients` | Create a client |
| `PUT` | `/api/clients/{id}` | Update a client |
| `DELETE` | `/api/clients/{id}` | Delete a client (fails if they have contracts) |

### Contracts

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/contracts?status=&clientId=&startDateFrom=&endDateTo=` | List contracts with filters |
| `GET` | `/api/contracts/{id}` | Get a contract by ID |
| `POST` | `/api/contracts` | Create a contract (`multipart/form-data`, optional PDF) |
| `PATCH` | `/api/contracts/{id}/status` | Update contract status |
| `GET` | `/api/contracts/{id}/agreement` | Download the signed agreement |
| `DELETE` | `/api/contracts/{id}` | Delete a contract |

### Service Requests

| Method | Endpoint | Description |
|---|---|---|
| `GET` | `/api/ServiceRequests` | List all service requests |
| `GET` | `/api/ServiceRequests/{id}` | Get a service request by ID |
| `POST` | `/api/ServiceRequests` | Create a service request (auto-converts USD → ZAR) |
| `GET` | `/api/ServiceRequests/exchange-rate` | Current USD → ZAR exchange rate |

All endpoints except `/api/auth/login` require `Authorization: Bearer <token>`.

Swagger UI is available at `/swagger` when running in Development.

---

## Design Decisions

**Why a separate Web and API project?**
The MVC frontend renders HTML and manages the user's session; the API is stateless and JWT-secured. Separating them makes the API reusable (mobile clients, integrations) and forces a clean boundary where DTOs are used instead of EF entities.

**Why a repository pattern in the API?**
`IContractRepository` isolates EF Core from the controllers, making it trivial to swap the data source or mock the repository in unit tests (`ServiceRequestFactoryTests` does exactly that).

**Why a factory for service requests?**
The creation of a `ServiceRequest` has multiple preconditions (contract exists, contract is not expired/on hold, conversion rate must be applied consistently). Centralizing that in `ServiceRequestFactory` keeps the controller thin and the business rules testable in isolation.

**Why does the Web app use `IApiService` rather than `HttpClient` directly?**
The `ApiService` wrapper automatically attaches the JWT (from session) to every outgoing request, handles deserialization, and logs failures. Every domain service (`ClientApiService`, `ContractApiService`, etc.) is built on top of it. No controller or service hand-rolls a `HttpClient` — a mistake that silently breaks auth (as this repo's commit history shows).

**Why environment variables for Docker, user-secrets for local?**
Neither belongs in source control. `dotnet user-secrets` writes outside the repo folder, and Docker Compose reads from `.env` (which is `.gitignore`d). `appsettings.json` contains only placeholders and non-sensitive defaults.

---

## Roadmap

Ideas for extending this project:

- [ ] ASP.NET Identity with hashed passwords and DB-backed users (replacing the demo credential)
- [ ] Refresh tokens for the JWT
- [ ] Role-based access control (Admin vs. Viewer)
- [ ] Pagination and server-side sorting on list endpoints
- [ ] Global exception-handling middleware with ProblemDetails responses
- [ ] CI pipeline (GitHub Actions) running `dotnet test` on every PR
- [ ] Deployment to Azure Container Apps

---

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

---

## Author

**Your Name**
- GitHub: [@AllyKat9](https://github.com/AllyKat9)
- LinkedIn: ([https://www.linkedin.com/in/alwande-gwala-301a071b7/)

---

## Acknowledgements

- [ExchangeRate-API](https://www.exchangerate-api.com/) for the free currency conversion endpoint
- [Bootstrap 5](https://getbootstrap.com/) for the UI toolkit
- The ASP.NET Core team for excellent documentation
