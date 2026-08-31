# SupportPilot

An AI-powered customer support platform demonstrating production-grade .NET engineering combined with modern AI integration patterns — RAG-based knowledge retrieval, agentic tool-use, real-time streaming chat, and sentiment-driven escalation.

Built as a portfolio project to demonstrate the intersection of senior full-stack .NET development and AI-augmented systems design.

> **Note on positioning:** This project demonstrates AI integration patterns (RAG, agentic tool-use, embeddings) applied to a support-ticketing domain. It is not a healthcare or industry-specific product — the customer-support domain was chosen because it's a well-understood, general-purpose use case for demonstrating these patterns clearly.

---

## What it does

SupportPilot is a chat-based customer support assistant that:

- Answers customer questions by retrieving relevant knowledge base articles (RAG) rather than relying purely on the LLM's training data
- Streams responses in real time over SignalR, so answers appear token-by-token instead of after a long wait
- Detects customer frustration through sentiment analysis and automatically escalates conversations to a human-reviewable ticket queue when a configurable threshold is crossed
- Uses agentic tool-use (via Semantic Kernel) so the AI model can decide when to search the knowledge base or create a ticket, rather than following a fixed script
- Gives admins a dashboard to review conversations, manage the knowledge base, and monitor escalation metrics

---

## Tech stack

**Backend**
- ASP.NET Core 10, C#, Clean Architecture (Domain / Application / Infrastructure / Api)
- Entity Framework Core + PostgreSQL (conversations, messages, tickets, users)
- Qdrant (vector search for the knowledge base — no Postgres mirror; single source of truth)
- Semantic Kernel (AI orchestration, function/tool calling)
- SignalR (real-time streaming chat)
- JWT authentication with role-based authorization (Customer / Agent / Admin)
- Serilog (structured logging)
- xUnit + Moq (targeted unit tests on business-critical logic)

**Frontend**
- React 19, TypeScript, Vite
- Tailwind CSS v4, shadcn/ui (Base UI primitives)
- react-router-dom, @microsoft/signalr
- Vitest + Testing Library

**AI Providers**
- OpenAI (`gpt-4o-mini`) and Anthropic (`claude-haiku-4-5`), routed by task — OpenAI for high-volume simple classification, Anthropic for compliance-sensitive decisions requiring a structured reasoning trail

---

## Architecture highlights

A few decisions worth calling out, since they reflect engineering judgment rather than default choices:

- **Escalation is deterministic, not LLM-judged.** Whether a conversation escalates is decided by a pure, testable threshold function (`EscalationPolicy`) based on a sentiment score — not left to the model's tool-choice judgment. This trades a little flexibility for reliability and auditability, which matters more in a support context.
- **Dependency direction is strictly inward.** Domain has zero dependencies. Application defines interfaces; Infrastructure implements them. This keeps business rules testable without spinning up a database or an AI provider.
- **SignalR + JWT.** WebSocket handshakes can't carry custom headers, so the token is passed via query string and manually attached to the request pipeline via `OnMessageReceived` — the standard pattern for securing SignalR with JWT.
- **Pure function extraction for testability.** Business-critical logic (`EscalationPolicy` on the backend, a message-append helper on the frontend) is extracted into pure functions specifically so it can be unit-tested without mocking half the system.
- **Rate limiting is partitioned by identity, not just IP.** Authenticated endpoints partition by user ID (from the JWT `sub` claim); unauthenticated auth endpoints partition by IP, since no identity exists yet at that point.

**Known simplification, documented honestly:** this project uses a single long-lived JWT rather than a short-lived access token + refresh token pair. A production system would rotate short-lived tokens to limit the exposure window if a token is ever compromised — that's the natural next step, intentionally out of scope here to keep the project focused.

---

## Enterprise-readiness signals

Two additions demonstrate patterns a growing SaaS product would need, without over-building features the demo doesn't require:

- **Integrations tab (UI stub).** The admin dashboard includes a "Connect" flow for Slack (functional toggle, no backend call) alongside Teams/Zendesk placeholders. This is intentionally UI-only — it demonstrates the intended integration surface without claiming a real webhook implementation. A production build would introduce an `IWebhookService` interface, following the same provider-abstraction pattern already used for `IChatProvider`, with real OAuth flows and signed webhook delivery.

- **Path to multi-tenancy.** The current data model is single-tenant — one shared set of conversations, tickets, and knowledge base articles. This was a deliberate scope decision, not an oversight: a demo project serving one imagined organization doesn't need tenant isolation to prove the underlying patterns work. Getting to multi-tenant would mean:
  - Adding a `TenantId` column to `Conversation`, `Ticket`, `Message`, and `User`, sourced from a claim on the JWT rather than a request parameter (so a compromised client can't spoof another tenant's ID).
  - Using EF Core's **global query filters** (`modelBuilder.Entity<T>().HasQueryFilter(e => e.TenantId == currentTenantId)`) so every query is automatically scoped — the alternative, manually adding a `.Where(TenantId == ...)` to every repository method, is exactly the kind of thing that gets forgotten once and causes a data leak.
  - Partitioning the Qdrant knowledge base per tenant (either separate collections or a `tenant_id` payload filter on search), since knowledge articles are tenant-specific content, not shared reference data.
  
  None of this is implemented — it's documented here because knowing the shape of the next step matters as much as building the current one.

---

## Getting started

### Prerequisites

- .NET 10 SDK
- Node.js 20+
- Docker Desktop (for PostgreSQL)
- An OpenAI API key, an Anthropic API key, and a Qdrant Cloud cluster (or self-hosted Qdrant)

### Backend setup

```bash
# 1. Start PostgreSQL
docker-compose up -d

# 2. Configure secrets (development uses .NET User Secrets — never commit these)
cd src/SupportPilot.Api
dotnet user-secrets set "ConnectionStrings:Postgres" "Host=localhost;Port=5432;Database=supportpilot;Username=supportpilot;Password=localdevpassword"
dotnet user-secrets set "Jwt:Key" "<a long random string — see note below>"
dotnet user-secrets set "OpenAI:ApiKey" "<your key>"
dotnet user-secrets set "Anthropic:ApiKey" "<your key>"
dotnet user-secrets set "Qdrant:ApiKey" "<your key>"

# 3. Apply migrations
dotnet ef database update --project ../SupportPilot.Infrastructure --startup-project .

# 4. Run
dotnet run
```

Generating a JWT signing key (PowerShell):
```powershell
$bytes = New-Object byte[] 64
[System.Security.Cryptography.RandomNumberGenerator]::Create().GetBytes($bytes)
[Convert]::ToBase64String($bytes)
```

On first run with an empty database, the app automatically seeds demo data — two users, a handful of knowledge base articles, and a few sample conversations — so there's no manual setup needed to explore the app.

### Frontend setup

```bash
cd frontend
npm install
npm run dev
```

---

## Demo credentials

The app seeds these accounts automatically on first run:

| Role | Email | Password |
|---|---|---|
| Admin | `admin@supportpilot.demo` | `Demo1234!` |
| Customer | `customer@supportpilot.demo` | `Demo1234!` |

Log in as Admin to see the dashboard (conversation history, escalation metrics, knowledge base management). Log in as Customer to try the chat experience.

---

## Project status

Actively in development. Completed so far: JWT auth with role-based access, demo data seeding, rate limiting, graceful AI-failure handling, sentiment visualization, analytics charts, PDF transcript export, and an integrations UI stub. Remaining before launch: production deployment (Render + Vercel), CORS lockdown, and a demo video.

See commit history for progress — this project is built incrementally, session by session, with an emphasis on getting each piece right before moving to the next rather than rushing to a demo-only MVP.
