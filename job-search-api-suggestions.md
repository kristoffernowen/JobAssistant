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
