# JobSearch Step 1 Instructions

## Mål

Lägg till en endpoint som anropar Job Search API på `https://jobsearch.api.jobtechdev.se/`.

## Krav

- Endpoint: `GET /ads/search`
- Parametrar till upstream:
  - `published-after` (obligatorisk, format `YYYY-MM-DDTHH:MM:SS`)
  - `published-before` (optional)
  - `municipality` (array/repeatable)
  - `occupation-group` (array/repeatable)
- Minst en av `municipality` eller `occupationGroup` krävs.
- Internt filter: optional `keyword` matchar `description.text` case-insensitive substring.
- Sortering: `pubdate-desc`.
- Optional `maxLimit` i request.
- Returnera `200` med tom lista vid no-result.
- Validera concept ids mot statiska filer i `JobAssistant.Api/Data/Taxonomy`.

## Response shape

`AdItem` ska inkludera:

- `title`
- `location`
- `occupation-group`
- `id`
- `webpage_url`

## Felmappning

- Upstream `429` -> ProblemDetails `429`
- Övriga upstreamfel -> ProblemDetails `502`
