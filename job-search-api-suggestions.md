# Job Search API suggestions

## Why this is a good pivot now

1 Lower complexity now:
You avoid import windows, overlap checks, and ad persistence logic while exploring filters.

2 Faster iteration:
You can quickly test and tweak your own filtering logic on fresh AF data.

3 More up-to-date results:
Each request fetches current ads from AF instead of relying on previously loaded DB data.

4 Better for discovery:
You can inspect ad payloads and gradually design your internal filtering model without schema lock-in.

## Recommended architecture (v1)

1 Replace import-first flow with runtime search-first flow:
Call AF Job Search API first, then run your own filtering logic before returning response.

2 Keep it stateless by default:
Each request should be self-contained and not require persisted ad storage.

3 Add optional short-lived session cache:
Cache AF result sets in memory for repeated local filtering experiments.

4 Keep existing ProblemDetails strategy:
Continue using RFC 7807 with your current error type conventions.

## API design recommendation

1 Start with one endpoint for search + internal filtering:
Use either GET with query parameters or POST with request body.
POST is often cleaner when filters grow.

2 AF base filters to include first:

- published-after
- published-before
- municipality
- occupation-group
  3 Internal filters run after AF response:
  Add your stricter logic in your own service layer before returning ads.

4 Enforce paging limits:
Always cap result size and handle offset/limit safely.

## Session strategy (optional, recommended for experimentation)

1 Add session mode as opt-in:
When enabled, create a session id and cache raw AF results.

2 Use TTL:
Store cached result sets for 10-30 minutes.

3 Refine without new AF call:
Run different internal filtering options on cached data in the same session.

4 Scale note:
If API is later deployed with multiple instances, move cache to distributed store.

## Concept ID handling (municipality and occupation-group)

1 Use static mapping files first:
Keep local JSON for municipality and occupation-group concept ids.

2 Validate before calling AF:
Reject unknown ids/labels with 400 Validation failed.

3 Include startup validation:
Fail fast or warn if mapping files are malformed.

4 Add refresh plan later:
Optionally sync from taxonomy API on schedule when you move beyond v1.

## Error mapping recommendation

1 Validation errors:
Return 400 when time range, required filters, or concept ids are invalid.

2 Upstream rate limit:
Map AF 429 to your own 429 ProblemDetails.

3 Upstream failures:
Map AF non-success and parse failures to 502 External service error.

4 Unexpected internal errors:
Return 500 Unexpected server error via central exception handling.

## Important implementation details

1 Time format:
Send published-after and published-before in YYYY-MM-DDTHH:MM:SS.

2 Timezone handling:
Normalize to UTC consistently in request validation and outbound calls.

3 Request timeout and resiliency:
Use sensible timeout and bounded retry policy with backoff.

4 Logging and observability:
Log outbound query parameters (without sensitive data), response status, and latency.

## Suggested endpoint shapes

### Option A: Stateless single-step search

Request includes:

- fromDateTime
- toDateTime
- municipalityIds
- occupationGroupIds
- maxAds
- optional internal filters

Response includes:

- ads
- sourceInfo (hit count, query time if useful)
- appliedFilters

### Option B: Session-based two-step flow

Step 1:
Create/search session and store raw AF result set.

Step 2:
Apply internal filters repeatedly on session data and return refined results.

Use this when you want fast comparative filtering experiments.

## Migration plan from current design

1 Keep existing user and user-skills endpoints unchanged.

2 De-emphasize import endpoint logic:
Current DB-centered ad import flow can be retired or kept temporarily behind feature flag.

3 Introduce new search service:
Create a dedicated JobSearch client and mapping/validation service for concept ids.

4 Update tests:
Focus on:

- validation behavior
- AF query composition
- error mapping
- internal filtering behavior
- optional session cache behavior

## Final recommendation

Proceed with runtime Job Search integration now, with static concept-id validation and optional in-memory session cache.
This gives the best balance of speed, control, and low complexity while you explore filtering and ad-data usage before deciding on long-term persistence.

## Copilot questions

Det jag behöver att du bekräftar innan implementation:

Endpoint-kontrakt för steg 1:
Vill du att vi använder befintliga sökvägen /ads/filter eller inför ny, exempelvis /ads/search?
Obligatoriska vs frivilliga fält:
Ska published-after vara obligatorisk? Ska municipality och occupation-group krävas samtidigt, eller räcker minst en av dem?
Interna filter i GET:
Vilka vill du börja med konkret? Exempelvis keyword i title/description, max age på annons, krav på remote, must-have skills.
Sortering och paging:
Default sort pubdate-desc? Default limit? Max limit?
Response-shape:
Ska vi returnera ett förenklat AdItem, eller även metadata som total hits och query time från AF?
No-result-beteende:
Ska tom träfflista ge 200 med tom lista, eller 404 i vissa fall?
Validering av concept ids:
Vill du skicka in concept ids direkt, eller labels som mappas till ids via statisk fil?
Statisk taxonomy-källa:
Var vill du lägga filerna i projektet och hur ofta ska de uppdateras manuellt?
För steg 2 (Option B) är designen också genomförbar med samma GET återanvänd:

GET /ads/search fortsätter vara stateless.
Ny POST, exempelvis /ads/sessions/{sessionId}/refine, tar ett filterobjekt för mer komplex logik.
Alternativt en POST /ads/sessions för att skapa session från samma basfilter som GET använder.

## Clarifications to Copilot

1. Använd sökväg /ads/search för endpoint för steg 1.
2. Published after ska vara obligatorisk. Det räcker med municipality eller occupation-group.
3. Interna filter i GET: börja med optional keyword måste finnas i description.
4. Default sort pubdate-desc. Vi återkommer till paging. Lägg till en optional max-limit i parametrarna.
5. Returnera en enkelt AdItem, men låt det inkludera title, location, occupation-group, id (det som af använder om man vill göra en query mot enskild ad) och webpage_url som visar var annonsen finns.
6. Ge 200 och tom lista.
7. Jag lägger in concept och municipality ids filer. Vi kan jobba med id direkt tillsvidare.
8. I JobAssistant.Api/Data/Taxonomy. Ingen rutin för uppdatering av filerna nu. Jag hämtar nya manuellt vid behov.

Två små antaganden jag (copilot) annars sätter själv vid implementation:

toDateTime/published-before är optional.
keyword-match görs case-insensitive substring på description.text.

## Implemented by copilot

1. Ny endpoint byggd: GET /ads/search.
2. Request-validering byggd:

- publishedAfter är obligatorisk.
- minst en av municipality eller occupationGroup krävs.
- maxLimit valideras till 1-100 om satt.

3. Koncept-id-validering byggd mot statiska taxonomy-filer i JobAssistant.Api/Data/Taxonomy.
4. Ny JobSearch-klient byggd mot https://jobsearch.api.jobtechdev.se/search.
5. Outbound query stöder:

- published-after
- optional published-before
- municipality (repeatable)
- occupation-group (repeatable)
- sort=pubdate-desc
- optional limit

6. Internt filter byggt:

- optional keyword matchar case-insensitive mot description.text.

7. Response-shape byggd enligt beslut:

- title
- location
- occupation-group
- id
- webpage_url

8. No-result-beteende byggt:

- 200 OK med tom lista.

9. Felmappning byggd för upstream:

- 429 från AF -> 429 ProblemDetails
- övriga upstream-fel -> 502 ProblemDetails

10. DI och nya kontrakt kopplade i API/Application/Infrastructure.
11. Tester tillagda för /ads/search (validering, tom lista, keyword-filter och fältmappning).
12. Verifierat lokalt med dotnet test: alla tester gröna.
