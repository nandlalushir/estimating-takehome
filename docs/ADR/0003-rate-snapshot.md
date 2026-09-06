# ADR-0003 — Snapshot catalogue pricing on estimate lines

## Status
Accepted

## Context
Catalogue rates are effective-dated and approved estimates are commercial records.

## Decision
Copy the selected rate and labour cost onto an estimate line when the line is added or updated.

## Alternatives rejected
Re-resolving the catalogue on every read could change historical estimates when the catalogue changes.

## Reconsider if
The business explicitly defines estimates as live-price documents rather than historical commercial records.
