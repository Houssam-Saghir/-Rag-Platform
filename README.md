# Enterprise RAG Platform

Production-ready Retrieval-Augmented Generation platform using **ASP.NET Core 8 + Angular 17 + SQL Server + OpenAI**.

## Architecture

```text
[Angular UI] -> [Rag.API]
                    |
       [Application Services/Validation]
                    |
      [Infrastructure: EF Core, OpenAI, Storage]
                    |
              [SQL Server]
```

## Prerequisites
- .NET 8 SDK
- Node.js 20+
- SQL Server
- OpenAI API key

## Setup
1. Configure `backend/src/Rag.API/appsettings.json` (or env vars).
2. Backend:
   - `cd backend`
   - `dotnet restore Rag.slnx`
   - `dotnet run --project src/Rag.API`
3. Frontend:
   - `cd frontend/rag-platform`
   - `npm install`
   - `npm start`

## Default Credentials
- Email: `admin@ragplatform.com`
- Password: `Admin@123`

## API Endpoints
- `/api/auth/register`, `/api/auth/login`, `/api/auth/refresh`, `/api/auth/revoke`
- `/api/documents/upload`, `/api/documents`, `/api/documents/{id}`
- `/api/chat/sessions`, `/api/chat/sessions/{id}/messages`, `/api/chat/sessions/{id}/ask`
- `/api/admin/users` and role assignment endpoints

## Docker
```bash
docker compose up --build
```
