# TaskFlow — Task Management Platform

![CI](https://github.com/OezalA/TaskManagement/actions/workflows/ci.yml/badge.svg)

A full-stack task & time-management platform: projects, a Kanban board, teams,
time tracking and a natural-language query panel — secured with Microsoft Entra ID
and shipped as a fully containerized stack. The user interface is in **German**.

> **Note on authentication:** Sign-in uses Microsoft Entra ID (Azure AD) tied to a
> specific tenant, so the live app can only be run against that tenant. This repo is
> primarily intended for **code review**; the sections below document the architecture
> and how the stack is built and run.

## Tech stack

| Layer | Technology |
|-------|-----------|
| Backend | .NET 9, ASP.NET Core Web API, Clean Architecture, EF Core |
| Database | PostgreSQL 16 |
| Auth | Microsoft Entra ID (Azure AD) — JWT bearer on the API, MSAL redirect flow on the SPA |
| Frontend | Angular 17 (standalone components), RxJS |
| AI / MCP | Natural-language query parser + MCP tool handler |
| Tests | xUnit (backend), Jasmine + Karma (frontend) |
| DevOps | Multi-stage Docker images, Docker Compose, GitHub Actions CI |

## Architecture

The backend follows Clean Architecture with clear layer boundaries:

```
TaskManagement.Domain          → entities, enums (no dependencies)
TaskManagement.Application      → DTOs, interfaces, use-case services, NLP parser
TaskManagement.Infrastructure   → EF Core, PostgreSQL, service implementations
TaskManagement.Api              → controllers, Entra ID auth, middleware
TaskManagement.MCP              → MCP tool handler exposing API capabilities
TaskManagement.Tests            → xUnit unit tests
TaskManagement.Frontend         → Angular SPA (served by nginx in production)
```

In the container setup, nginx serves the Angular build and reverse-proxies `/api`
to the backend, so the whole app is same-origin (no CORS) behind a single entry point.

## Features

- **Projects** with descriptions and a per-project **Kanban board** (Zu erledigen / In Bearbeitung / Erledigt)
- **Tasks**: assignment, due dates, status transitions
- **Teams & members** management
- **Time tracking**: start/stop work, live timer, weekly reports per user
- **AI query panel**: ask in natural German (e.g. *"Was hat Hans diese Woche gemacht?"*) and it maps to MCP tools
- **Microsoft Entra ID** login (full-page MSAL redirect flow) with enforced JWT validation on every endpoint

## Run the whole stack with Docker

```bash
docker compose up -d --build
```

| Service | URL |
|---------|-----|
| Frontend (nginx) | http://localhost:4200 |
| API | http://localhost:8080 |
| PostgreSQL | localhost:5432 |

Stop it with `docker compose down` (data is kept in the `pgdata` volume).

## Run in development

```bash
# Backend (http://localhost:5253)
dotnet run --project TaskManagement.Api --launch-profile http

# Frontend (http://localhost:4200, proxies /api to the backend)
cd TaskManagement.Frontend
npm install
npm start
```

## Tests

```bash
# Backend
dotnet test

# Frontend (headless)
cd TaskManagement.Frontend
npm test -- --watch=false --browsers=ChromeHeadless
```

Both suites run automatically on every push via **GitHub Actions** (see the CI badge above):
backend build + tests, frontend build + tests, and a Docker image build.
