# ADR-001: Runtime JobSearch Pivot

## Status

Accepted

## Date

2026-06-29

## Context

- Import-first flödet med overlap-kontroll och lagring i databas gjorde exploration långsammare.
- Projektet behövde snabbare iteration på filtrering och payload-förståelse.

## Decision

- Pivotera till runtime search-first via AF JobSearch API.
- Behåll stateless sökning som standard.
- Tillåt optional session-cache för experimentell refine utan ny upstream-call.
- Behåll befintlig ProblemDetails-strategi för felhantering.

## Consequences

Positiva:

- Snabbare iteration av filterlogik.
- Färskare annonser direkt från upstream.
- Mindre schema-låsning i tidigt skede.

Negativa:

- Mindre nytta av lagrade annonser i databas i detta steg.
- Sessionlösning måste hanteras extra noga vid eventuell skalning till flera noder.
