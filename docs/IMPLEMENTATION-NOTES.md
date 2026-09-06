# Implementation Notes

This repository is intentionally a compact, explainable take-home implementation.

## Frontend

The repository includes a React + TypeScript + Vite single-screen frontend under `web/`. It uses the API contract without putting authorization decisions in the browser.

## Important note about migrations

The API calls `Database.EnsureCreatedAsync()` on startup. For a production application, schema changes would normally be applied by a deployment step rather than by every API instance at startup.

## Concurrency

The current code has a transaction and a concurrency token, but the take-home should add a PostgreSQL integration test that executes two approval attempts concurrently. This is the first hardening step before calling the implementation production-grade.
