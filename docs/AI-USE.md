# AI Usage

## Tools used

ChatGPT:
- architecture brainstorming
- domain invariant review
- test case identification
- documentation drafting

GitHub Copilot/Cursor:
- optional boilerplate completion during implementation

## Concrete AI output rejected

A tempting implementation is:

```csharp
if (request.Role == "Reviewer")
{
    allowApproval = true;
}
```

I rejected this because the client must not be trusted to assert its own role. The implementation instead reads `X-User-Email` and resolves the user's role and project assignment from server-side data.

## Before / after

Before:
```text
Client -> role header -> API -> authorize
```

After:
```text
Client -> X-User-Email -> server user lookup -> role + assignment -> authorize
```

## Code I would review line-by-line before production

The persistence/concurrency implementation is intentionally compact for the take-home. In a production system I would add PostgreSQL integration tests for concurrent approval, stronger idempotency semantics if required by clients, and a dedicated query layer for very large estimates.
