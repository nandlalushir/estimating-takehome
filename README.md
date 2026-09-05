# Cost Estimating — Take-Home Starter

This scaffold exists only to save you setup minutes. It contains **no domain code** — modelling the domain is the exercise.

You are free to delete all of it and start from `dotnet new`. Nothing here is graded.

## What's in the box

```
src/
  CostEstimating.Domain/          entities, value objects, invariants — no framework references
  CostEstimating.Application/     use cases
  CostEstimating.Infrastructure/  persistence, external concerns
  CostEstimating.Api/             ASP.NET Core Web API host
  web/                            your React or Angular app
tests/
  CostEstimating.Tests/
  CostEstimating.ArchitectureTests/
seed/                             users, projects, effective-dated rate catalogue
docs/                             templates for your required documents
docker-compose.yml                PostgreSQL and SQL Server, pick one
.github/workflows/ci.yml          CI stub
```

The `src/` layout is a suggestion, not a requirement. Vertical slices, a modular monolith, or a single project are all defensible — **tell us why in your ADR** if you restructure.

## Getting started

```bash
# 1. Databases (start only the one you need)
cp .env.example .env
docker compose up -d postgres      # or: docker compose up -d sqlserver

# 2. Backend projects
dotnet new sln -n CostEstimating
dotnet new classlib -o src/CostEstimating.Domain
dotnet new classlib -o src/CostEstimating.Application
dotnet new classlib -o src/CostEstimating.Infrastructure
dotnet new webapi    -o src/CostEstimating.Api
dotnet new xunit     -o tests/CostEstimating.Tests
dotnet new xunit     -o tests/CostEstimating.ArchitectureTests
dotnet sln add $(find src tests -name '*.csproj')

# 3. Frontend — your choice
npm create vite@latest web -- --template react-ts     # in src/
ng new web --routing --style=scss                     # or Angular
```

Connection strings are in `.env.example`.

## Identity — you are not building authentication

There is no login, no token issuing and no password handling in this exercise. Identity arrives on every request in an `X-User-Email` header. Paste this in and move on:

```csharp
// DevIdentityMiddleware.cs — development only. In production this is replaced by
// a JWT bearer handler; nothing else about your authorisation code should have to change.
public sealed class DevIdentityMiddleware(RequestDelegate next)
{
    public const string HeaderName = "X-User-Email";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(HeaderName, out var value)
            || string.IsNullOrWhiteSpace(value))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        // Identity only. No role claim, deliberately: a client does not get to
        // tell you what it is allowed to do. Resolve role and project assignment
        // from your own store.
        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, value.ToString().Trim())],
            authenticationType: "Dev");

        context.User = new ClaimsPrincipal(identity);
        await next(context);
    }
}

// Program.cs
app.UseMiddleware<DevIdentityMiddleware>();
```

An email that does not match a seeded user should be rejected — that is your code, not this middleware's.

Calling the API:

```bash
curl -H "X-User-Email: estimator@example.com" http://localhost:5000/api/projects
```

In the UI, a role switcher that sets the header is enough. No login screen.

## Seed data notes

- **Rates are effective-dated.** A catalogue item has multiple rate rows; the applicable one is the latest `effectiveFrom` on or before the estimate's pricing date.
- `labourComponent` is a *part of* `rate`, not additional to it.
- `role` and `projectCodes` in `users.json` are **server-side data**. Resolve them from your store on every request; never take them from the client.
- Loading the seed data at startup is fine. So is a migration, or a script. Your call.

## Your documents

Templates are in `docs/`. All four are required — see the brief, section 4.

- `docs/DESIGN.md` — **commit this before you write code.**
- `docs/ADR/` — copy `0000-template.md` for each decision.
- `docs/REQUIREMENTS.md` — define / implement / enforce traceability.
- `docs/AI-USE.md`

## Before you submit

- [ ] `docs/DESIGN.md` committed before the first code commit
- [ ] Someone can clone and run this from these instructions in under 10 minutes
- [ ] The README says how to act as each role — the header value, or the control in the UI
- [ ] `docs/REQUIREMENTS.md` filled in, including the rules you deliberately did not implement
- [ ] At least one enforcement mechanism that is structural, not a unit test — and you have watched it fail
- [ ] `docs/AI-USE.md` filled in
- [ ] No real secrets committed
- [ ] Actual time spent noted in the README
