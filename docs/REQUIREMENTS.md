# Requirements Traceability

| Rule | Defined | Implemented | Enforcement |
|---|---|---|---|
| R1 | CatalogueItem/CatalogueRate | Domain + EF | Unit/domain validation + DB schema |
| R2 | RateResolver | Application | Automated domain/API path + effective-date query |
| R3 | EstimateLine | Domain | Unit tests |
| R4 | EstimateLine.Recalculate | Domain | Unit tests |
| R5 | Estimate.Total | Domain | Unit tests |
| R6 | Quantity validation | Domain | Unit tests |
| R7 | AddOrUpdateLine + DB unique index | Domain/Infrastructure | Unit test + unique DB constraint |
| R8 | Estimate state machine | Domain | Unit tests |
| R9 | Submit | Domain/Application | Unit tests |
| R10 | EnsureEditable | Domain | Unit test |
| R11 | CorrectApprovedDescription + authorization | Domain/Application | Code path + authorization |
| R12 | Transaction + lifecycle + concurrency token | Application/Infrastructure | Integration strategy; should be exercised against PostgreSQL before submission |
| R13 | EstimateAuditEvent | Application/Infrastructure | Audit write path |
| R14 | UserRole | Domain | Authorization service |
| R15 | AuthorizationService | Application | Server-side checks |
| R16 | EnsureCanApproveAsync | Application | Server-side check |
| R17 | Estimate DTO projection | Application | Labour field null for unauthorized roles |
| R18 | ProjectAssignment | Infrastructure/Application | Server-side project access |
| R19 | CurrentUser reads identity only | API/Application | Role is resolved from DB |
| R20 | Indexes + projection | Infrastructure/Application | Query design; load test recommended |

## Deliberately not implemented

No real authentication/token issuing was implemented because the brief explicitly removes authentication from the exercise. The supplied development identity header is used only as identity, not as a source of authorization claims.

## Submission hardening checklist

Before submitting to an evaluator, run:

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
cd web
npm install
npm run build
```

Then perform the PostgreSQL approval retry scenario twice and verify only one audit event exists.
