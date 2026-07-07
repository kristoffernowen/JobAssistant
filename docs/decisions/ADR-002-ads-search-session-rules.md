# ADR-002: Ads Search Session Rules

## Status

Accepted

## Date

2026-06-29

## Context

- Sessionsflöde behövdes för att iterera refine-filter utan att göra nya AF-anrop.
- Otydlig sessionidentifiering riskerade buggar och oavsiktlig datadelning.

## Decision

- Session identifieras med servergenererat `sessionId`.
- Session lagras in-memory med sliding expiration.
- Hårt maxtak: sessionsflödet returnerar aldrig mer än 50 annonser.
- Om fler träffar finns klipps listan och messages visar att maxgränsen aktiverats.
- `maxLimit` i request är optional men får inte överskrida hard cap 50.
- Saknad eller utgången session returnerar 404.

## Consequences

Positiva:

- Snabb refine-iteration utan ny upstream-latens.
- Deterministisk hantering med explicit `sessionId`.

Negativa:

- In-memory sessions är instansbundna.
- Vid horisontell skalning krävs distribuerad cache för konsekvent beteende.
