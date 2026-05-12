# Affalite - Affiliate Marketing Platform

## Overview
Affalite is an affiliate marketing platform built with **.NET 8**, featuring a 3-layer architecture (PL → BL → DAL). It connects merchants, affiliates, and customers with AI-powered content generation and intelligent product matching.

## Architecture
- **AffalitePL**: ASP.NET Core Web API (Controllers, Middleware, Configuration)
- **AffaliteBL**: Business Logic (Services, DTOs, Mappers, Background Jobs)
- **AffaliteDAL**: Data Access Layer (EF Core, Repositories, Entities, Migrations)

## Prerequisites
- .NET 8 SDK
- SQL Server (local or cloud)
- Optional: Redis for caching

## Getting Started

### 1. Clone & Restore
```bash
git clone <repo-url>
cd Affalite
dotnet restore
```

### 2. Configure Secrets
**Do NOT commit secrets to source control.** Use User Secrets for development:

```bash
cd AffalitePL
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=..."
dotnet user-secrets set "JWT:Key" "YOUR_32+_CHAR_STRONG_KEY"
dotnet user-secrets set "Claude:ApiKey" "YOUR_CLAUDE_KEY"
dotnet user-secrets set "AiSettings:OpenAiApiKey" "YOUR_OPENROUTER_KEY"
dotnet user-secrets set "AiSettings:CohereApiKey" "YOUR_COHERE_KEY"
dotnet user-secrets set "EmailSettings:SmtpPassword" "YOUR_SMTP_PASSWORD"
dotnet user-secrets set "DefaultAdmin:Password" "YOUR_ADMIN_PASSWORD"
```

### 3. Database Setup
```bash
cd AffaliteDAL
dotnet ef database update --startup-project ../AffalitePL
```

### 4. Run
```bash
cd AffalitePL
dotnet run
```

The API will be available at `https://localhost:7001` (or as configured).

## Key Features
- **Authentication**: JWT + Refresh Tokens with ASP.NET Identity
- **Roles**: Admin, Merchant, Affiliate, Customer
- **Orders & Commissions**: Automated commission calculation and order lifecycle
- **AI Content**: Generate social media content via OpenRouter/Claude APIs
- **Matching Engine**: AI-powered affiliate-to-product recommendations using embeddings
- **File Uploads**: Secure image uploads with validation

## Security Checklist
- [ ] Rotate all placeholder secrets in `appsettings.json`
- [ ] Move production secrets to Azure Key Vault or AWS Secrets Manager
- [ ] Enable HTTPS in production
- [ ] Review CORS origins in `ServiceCollectionExtensions.cs`
- [ ] Restrict Swagger to development environment only

## API Documentation
When running in Development mode, Swagger UI is available at `/swagger`.

## Background Jobs
- **MatchingBackgroundJob**: Runs every 6 hours (configurable) to generate affiliate-product match recommendations

## Testing
To run tests (when added):
```bash
dotnet test
```

## Notes
- The `AffaliteBLL` namespace is being phased out in favor of `AffaliteBL`. New code should use `AffaliteBL`.
- Nullable reference types are enabled; warnings indicate areas that may need null-safety improvements.
