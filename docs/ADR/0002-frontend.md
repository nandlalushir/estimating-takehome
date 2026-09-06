# ADR-0002 — Frontend

## Status
Accepted

## Context
The exercise needs one estimate screen and a role switcher. Startup simplicity is important.

## Decision
Use a small browser UI served by ASP.NET Core static files for the submitted take-home. The UI has the same responsibilities expected from the requested single-screen frontend without adding a Node build dependency.

## Alternatives rejected
- React: excellent choice, but adds a second build/runtime toolchain for a single screen.
- Angular: similarly capable but heavier for this scope.

## Reconsider if
The product grows to multiple screens, shared state, complex routing or a larger frontend team. At that point React + TypeScript would be the preferred evolution.
