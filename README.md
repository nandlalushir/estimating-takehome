# Cost Estimating Take-Home

A .NET 8 + PostgreSQL implementation of the construction cost estimating slice.

## Requirements

- .NET 8 SDK
- Docker Desktop
- Node.js 20+ and npm (for the React UI)

## Run locally

### 1. Start PostgreSQL

```bash
docker compose up -d
```

### 2. Run the API

```bash
dotnet restore
dotnet run --project src/CostEstimating.Api
```

API/Swagger:
- `http://localhost:5080`
- `http://localhost:5080/swagger`

The API creates the development schema and seed data on startup.

### 3. Run the React UI

In another terminal:

```bash
cd web
npm install
npm run dev
```

Open `http://localhost:5173`.

The Vite proxy sends `/api` calls to `http://localhost:5080`.

## Demo users

The UI role switcher changes the `X-User-Email` header:

| User | Effective role | Project capability |
|---|---|---|
| estimator@example.com | Estimator | Edit/submit |
| reviewer@example.com | Reviewer | Approve/reject, see labour |
| viewer@example.com | Viewer | Read-only |
| manager@example.com | Estimator | Project manager; can correct approved description and see labour |

There is deliberately no authentication in this exercise.

## API

```text
GET  /api/projects
POST /api/projects/{projectId}/estimates
GET  /api/estimates/{estimateId}
PUT  /api/estimates/{estimateId}/lines/{catalogueItemId}
POST /api/estimates/{estimateId}/submit
POST /api/estimates/{estimateId}/approve
POST /api/estimates/{estimateId}/reject
POST /api/estimates/{estimateId}/return-to-draft
PATCH /api/estimates/{estimateId}/description
```

Every request should include:

```http
X-User-Email: estimator@example.com
```

## Example curl

```bash
curl -H "X-User-Email: estimator@example.com" \
  http://localhost:5080/api/projects
```

```bash
curl -H "X-User-Email: estimator@example.com" \
  http://localhost:5080/api/estimates/40000000-0000-0000-0000-000000000001
```

## Tests

```bash
dotnet test
```

The test suite focuses on high-risk domain invariants and architecture enforcement.

## Design

See:

- `docs/DESIGN.md`
- `docs/REQUIREMENTS.md`
- `docs/AI-USE.md`
- `docs/ADR/`

## Important design choices

1. PostgreSQL + EF Core.
2. Estimate is the aggregate root.
3. Effective-dated rates are selected by pricing date.
4. Selected rate/labour cost is snapshotted on the line.
5. Duplicate catalogue items update the existing line.
6. Approved estimates are immutable except for project-manager description correction.
7. Roles are resolved from server-side user data; the client supplies identity only.
8. Project assignment is checked on every project/estimate operation.
9. Labour cost is excluded from DTOs for unauthorized roles.
10. Audit events are written for lifecycle transitions.

## SDLC / commit history

The assessment asks for design-first commits. Recommended sequence is documented in `docs/DESIGN.md`. Do not squash these into one commit if submitting as a take-home.

## Time

Record the actual implementation time here before submission. Example:

`Implementation time: 4h 15m`

The brief explicitly prefers an honest time record over pretending the work took three hours.

## Production follow-ups

- PostgreSQL integration tests for concurrent approval.
- Strong idempotency key support if clients can retry across network failures.
- Keyset pagination/virtualization for very large estimates.
- Real authentication/identity integration.
- Structured logging sink and OpenTelemetry.
- Browser E2E tests.
