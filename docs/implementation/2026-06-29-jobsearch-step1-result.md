# 2026-06-29 JobSearch Step 1 Result

## Genomfört

1. Ny endpoint: `GET /ads/search`.
2. Request-validering:
   - `publishedAfter` obligatorisk
   - minst en av `municipality` eller `occupationGroup`
   - `maxLimit` validerad till 1-100 om satt
3. Concept-id-validering mot statiska taxonomy-filer i `JobAssistant.Api/Data/Taxonomy`.
4. Ny JobSearch-klient mot `https://jobsearch.api.jobtechdev.se/search`.
5. Outbound query med:
   - `published-after`
   - optional `published-before`
   - repeatable `municipality`
   - repeatable `occupation-group`
   - `sort=pubdate-desc`
   - optional `limit`
6. Internt keyword-filter mot `description.text` (case-insensitive substring).
7. Response-shape enligt beslut:
   - title, location, occupation-group, id, webpage_url
8. No-result: `200` med tom lista.
9. Felmappning:
   - upstream `429` -> `429 ProblemDetails`
   - övriga upstreamfel -> `502 ProblemDetails`
10. DI och kontrakt kopplade i API/Application/Infrastructure.
11. Tester tillagda för `/ads/search`.

## Verifiering

- `dotnet test`: alla tester gröna i steget.
