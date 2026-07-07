# JobSearch Option B Instructions (Session Flow)

## Mål

- Behåll `GET /ads/search` för stateless sökning.
- Lägg till sessionsflöde med explicit `sessionId`.

## Endpoint 1

`POST /ads/sessions/search`

- Tar sökparametrar:
  - `publishedAfter` obligatorisk
  - `publishedBefore` optional
  - `municipality`/`occupationGroup`: minst en krävs
  - `keyword` optional
  - `maxLimit` optional
- Hämtar data från AF JobSearch.
- Sparar resultat i in-memory session.
- Returnerar `sessionId`, `expiresAtUtc`, `ads`, `messages`.

## Endpoint 2

`POST /ads/sessions/{sessionId}/refine`

- Tar filterobjekt.
- Filtrerar sparad session-data utan ny AF-call.
- Returnerar `sessionId`, `expiresAtUtc`, `ads`, `messages`.

## Regler

- Session identifieras alltid med servergenererat `sessionId`.
- Sliding expiration på session.
- Hård cap på 50 annonser i sessionsflödet.
- Om fler än 50 träffar finns klipps listan och `messages` informerar.
- `maxLimit` är optional men returnerar aldrig mer än 50.
- `404` vid saknad/utgången session.
