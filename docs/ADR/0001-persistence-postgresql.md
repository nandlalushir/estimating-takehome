# ADR-0001 — PostgreSQL

## Status
Accepted

## Context
The domain contains projects, assignments, estimates, lines, catalogue items, effective-dated rates and audit events. Referential integrity and transactional lifecycle changes matter.

## Decision
Use PostgreSQL with EF Core.

## Alternatives rejected
- SQL Server: valid but unnecessary for this small cross-platform exercise.
- NoSQL: weaker fit for relational constraints and effective-dated relational queries.

## Reconsider if
The workload becomes dominated by analytical/time-series queries or an existing enterprise platform mandates another database.
