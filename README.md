# 🛒 PosSystem.Api

> A production-ready, secure Point-of-Sale (POS) Web API built with **ASP.NET Core**, **Entity Framework Core**, and **SQL Server**, following Clean Architecture principles.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4?style=for-the-badge&logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4?style=for-the-badge&logo=aspdotnet)](https://dotnet.microsoft.com/apps/aspnet)
[![EF Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4?style=for-the-badge&logo=entityframework)](https://learn.microsoft.com/ef/)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?style=for-the-badge&logo=microsoftsqlserver)](https://www.microsoft.com/sql-server/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg?style=for-the-badge)](LICENSE)

---

## 📋 Table of Contents

- [🚀 Features](#-features)
- [🏗️ Architecture](#️-architecture)
- [📁 Project Structure](#-project-structure)
- [🛠️ Getting Started](#-getting-started)
- [⚙️ Configuration](#-configuration)
- [🔐 Authentication & Authorization](#-authentication--authorization)
- [📚 API Endpoints](#-api-endpoints)
- [💳 Bakong Payment Integration](#-bakong-payment-integration)
- [🧪 Testing with Postman](#-testing-with-postman)
- [🔒 Security](#-security)
- [📦 Deployment](#-deployment)
- [🤝 Contributing](#-contributing)
- [📄 License](#-license)

---

## 🚀 Features

| Category | Features |
|----------|----------|
| **Product Management** | CRUD operations, stock tracking, image upload, categories |
| **Sales** | Create sales, sale items, sales history, reporting |
| **Customer Management** | Customer profiles, purchase history |
| **Inventory** | Stock levels, stock adjustments, low-stock alerts |
| **Users & Roles** | User registration, role-based access control (RBAC), refresh tokens |
| **Authentication** | JWT Bearer tokens, API Key authentication, refresh token rotation |
| **Security** | Rate limiting, security headers, anti-forgery, request validation, audit logging |
| **Payments** | Bakong payment integration (QR code, callback handling) |
| **File Handling** | File upload service, product image management |
| **Database** | Entity Framework Core Migrations, SQL Server |

---

## 🏗️ Architecture

The solution follows **Clean Architecture** with clear separation of concerns:

```
PosSystem.Api          → Presentation layer (Controllers, Middleware, Filters)
PosSystem.Application  → Business logic (Services, DTOs, Interfaces)
PosSystem.Domain       → Domain entities, value objects
PosSystem.Infrastructure → Data access (EF Core, Repositories, Migrations)
PosSystem.Shared       → Shared kernels, common utilities
```

### Dependency Flow

```
Api → Application → Domain
     ↑             ↓
Infrastructure ←──┘
```

---

## 📁 Project Structure

```
Pos_WebApi/
├── README.md
├── PosSystem.Api.sln
├── demos/
│   ├── bakong-payment-integration.md
│   ├── bakong-payment-test.html
│   ├── join-example.md
│   └── payment-methods-integration.md
├── PosSystem.Api/
│   ├── Controllers/          # API Controllers
│   ├── Security/             # Auth middleware, JWT, API keys
│   ├── Middleware/           # Custom middleware components
│   ├── Filters/              # Action filters
│   ├── Authorization/        # Policies and handlers
│   ├── Extensions/           # Service extensions
│   ├── Services/             # File service, etc.
│   ├── Models/               # Request/Response models
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   ├── Program.cs
│   └── PosSystem.Api.http    # HTTP request file for testing
├── PosSystem.Application/
│   ├── Services/             # Business logic services
│   ├── DTOs/                 # Data transfer objects
│   ├── Interfaces/           # Service and repository interfaces
│   └── Mapping/              # AutoMapper profiles
├── PosSystem.Domain/
│   ├── Entities/             # Domain entities
│   └── ValueObjects/         # Value objects (e.g., Money)
├── PosSystem.Infrastructure/
│   ├── Persistence/          # EF Core DbContext, Configurations, Repositories
│   ├── Migrations/           # Database migrations
│   ├── Services/             # Infrastructure services
│   └── DependencyInjection.cs
└── PosSystem.Shared/
    └── Shared kernels
```

---

## 🛠️ Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- [SQL Server](https://www.microsoft.com/sql-server/) (Express, LocalDB, or full edition)
- [Postman](https://www.postman.com/) (optional, for API testing)

### 1. Clone the Repository

```bash
git clone https://github.com/your-org/PosSystem.Api.git
cd Pos_WebApi
```

### 2. Configure the Database

Update the connection string in [`PosSystem.Api/appsettings.json`](PosSystem.Api/appsettings.json:8):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=PosSystem_DB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

### 3. Apply Migrations

```bash
dotnet ef database update --project PosSystem.Infrastructure --startup-project PosSystem.Api
```

Or let the application apply migrations automatically on startup (configured in [`Program.cs`](PosSystem.Api/Program.cs:164)).

### 4. Run the Application

```bash
dotnet run --project PosSystem.Api
```

The API will be available at:
- **HTTP:** `http://localhost:5000`
- **HTTPS:** `https://localhost:5001`

---

## ⚙️ Configuration

### appsettings.json

Key configuration sections in [`PosSystem.Api/appsettings.json`](PosSystem.Api/appsettings.json:1):

| Section | Key | Description |
|---------|-----|-------------|
| `ConnectionStrings` | `DefaultConnection` | SQL Server connection string |
| `ApiKey` | `HeaderName` | API key header name (default: `X-API-Key`) |
| `ApiKey` | `Key` | API key value for authentication |
| `Jwt` | `Key` | Secret key for JWT token signing |
| `Jwt` | `Issuer` | JWT token issuer |
| `Jwt` | `Audience` | JWT token audience |
| `Jwt` | `ExpireMinutes` | Access token expiration (minutes) |
| `Jwt` | `RefreshTokenExpireDays` | Refresh token expiration (days) |
| `AllowedHosts` | `*` | Allowed hosts for the application |

### Environment Variables

You can override any setting using environment variables:

```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=...
Jwt__Key=YourSuperSecretKey
```

---

## 🔐 Authentication & Authorization

### Authentication Methods

The API supports **two authentication methods**:

1. **JWT Bearer Token** — Obtain via `/api/auth/login`, include in `Authorization: Bearer <token>` header
2. **API Key** — Include in `X-API-Key` header

### User Roles

| Role | Permissions |
|------|-------------|
| `Admin` | Full access to all endpoints |
| `Manager` | Product, customer, category, stock, sales management |
| `Cashier` | Create sales, view sales, manage stock |

### Authorization Policies

Defined in [`Program.cs`](PosSystem.Api/Program.cs:89):

- `CanCreateProduct`, `CanUpdateProduct`, `CanDeleteProduct`
- `CanCreateCustomer`, `CanUpdateCustomer`, `CanDeleteCustomer`
- `CanSell`, `CanManageUsers`, `CanViewReports`
- `CanManageStock`, `CanViewSales`, `CanManageCategories`

---

## 📚 API Endpoints

### Authentication

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/auth/register` | Register a new user | None |
| `POST` | `/api/auth/login` | Login and get JWT token | None |
| `POST` | `/api/auth/refresh` | Refresh access token | Refresh Token |

### Products

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/products` | Get all products | JWT |
| `GET` | `/api/products/{id}` | Get product by ID | JWT |
| `POST` | `/api/products` | Create a product | JWT (Admin/Manager) |
| `PUT` | `/api/products/{id}` | Update a product | JWT (Admin/Manager) |
| `DELETE` | `/api/products/{id}` | Delete a product | JWT (Admin) |

### Categories

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/categories` | Get all categories | JWT |
| `POST` | `/api/categories` | Create a category | JWT (Admin/Manager) |
| `PUT` | `/api/categories/{id}` | Update a category | JWT (Admin/Manager) |
| `DELETE` | `/api/categories/{id}` | Delete a category | JWT (Admin) |

### Customers

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/customers` | Get all customers | JWT |
| `POST` | `/api/customers` | Create a customer | JWT (Admin/Manager) |
| `PUT` | `/api/customers/{id}` | Update a customer | JWT (Admin/Manager) |
| `DELETE` | `/api/customers/{id}` | Delete a customer | JWT (Admin) |

### Sales

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/sales` | Get all sales | JWT |
| `POST` | `/api/sales` | Create a sale | JWT (Admin/Cashier) |
| `GET` | `/api/sales/{id}` | Get sale by ID | JWT |
| `PUT` | `/api/sales/{id}` | Update a sale | JWT |

### Stock

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/stock` | Get all stock items | JWT |
| `POST` | `/api/stock` | Create stock entry | JWT (Admin/Manager) |
| `PUT` | `/api/stock/{id}` | Update stock | JWT (Admin/Manager) |

### Roles & Users

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `GET` | `/api/roles` | Get all roles | JWT (Admin) |
| `POST` | `/api/roles` | Create a role | JWT (Admin) |
| `POST` | `/api/users/assign-role` | Assign role to user | JWT (Admin) |

### Bakong Payment

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/bakong/payment` | Initiate Bakong payment | JWT |
| `POST` | `/api/bakong/callback` | Bakong payment callback | None |

### File Upload

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| `POST` | `/api/files/upload` | Upload a file | JWT |

> 📖 Full API documentation is available via the [`PosSystem.Api.http`](PosSystem.Api/PosSystem.Api.http:1) file for use with Postman or VS Code REST Client.

---

## 💳 Bakong Payment Integration

The API integrates with **Bakong** (Cambodia's digital payment system) for QR code payments.

- **Demo:** [`demos/bakong-payment-test.html`](demos/bakong-payment-test.html:1)
- **Integration Guide:** [`demos/bakong-payment-integration.md`](demos/bakong-payment-integration.md:1)
- **Payment Methods:** [`demos/payment-methods-integration.md`](demos/payment-methods-integration.md:1)

Configure Bakong settings in `appsettings.json` under the `bakong` section.

---

## 🧪 Testing with Postman

A collection of HTTP requests is provided in [`PosSystem.Api.http`](PosSystem.Api/PosSystem.Api.http:1). You can use:

- **Postman** — Import the `.http` file or create requests manually
- **VS Code REST Client** — Open the `.http` file directly in VS Code

### Example: Login Request

```http
POST https://localhost:5001/api/auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "Admin123!"
}
```

### Example: Create Product (with JWT)

```http
POST https://localhost:5001/api/products
Content-Type: application/json
Authorization: Bearer <your-jwt-token>

{
  "name": "Sample Product",
  "price": 29.99,
  "categoryId": 1,
  "stockQuantity": 100
}
```

---

## 🔒 Security

| Feature | Implementation |
|---------|---------------|
| **JWT Authentication** | Bearer token with expiration and refresh tokens |
| **API Key Authentication** | `X-API-Key` header validation |
| **Rate Limiting** | Fixed window rate limiter (100 req/min) |
| **Security Headers** | X-Content-Type-Options, X-Frame-Options, CSP, etc. |
| **Anti-Forgery** | CSRF protection middleware |
| **Request Size Limiting** | Max 10MB request body |
| **Request Validation** | Model validation and sanitization |
| **Exception Handling** | Global exception middleware |
| **Security Audit** | Audit logging for auth/authz events |
| **Password Hashing** | BCrypt password hashing |

---

## 📦 Deployment

### Docker (Recommended)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["PosSystem.Api/PosSystem.Api.csproj", "PosSystem.Api/"]
RUN dotnet restore "PosSystem.Api/PosSystem.Api.csproj"
COPY . .
WORKDIR "/src/PosSystem.Api"
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "PosSystem.Api.dll"]
```

### IIS / Windows Server

1. Publish the application: `dotnet publish -c Release`
2. Deploy to IIS with ASP.NET Core Module
3. Configure connection string in `appsettings.json` or environment variables
4. Ensure SQL Server is accessible from the application server

---

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -m 'Add some feature'`
4. Push to the branch: `git push origin feature/your-feature`
5. Open a Pull Request

### Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep controllers thin; business logic in services

---

## 📄 License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- [ASP.NET Core](https://dotnet.microsoft.com/apps/aspnet) — Web framework
- [Entity Framework Core](https://learn.microsoft.com/ef/) — ORM
- [Swashbuckle.AspNetCore](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) — API documentation
- [BCrypt.Net-Next](https://github.com/BcryptNet/bcrypt.net) — Password hashing
- [Humanizer](https://humanizr.net/) — String manipulation

---

<div align="center">
  <b>Built with ❤️ for modern POS systems</b>
</div>
