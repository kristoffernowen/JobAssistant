# Endpoints with request/response

## Standard for errors: ProblemDetails

Use RFC 7807 ProblemDetails for all non-2xx responses.

### Response (JSON)

Record ApiProblemDetails(
string Type,
string Title,
int Status,
string Detail,
string Instance,
Dictionary<string, string[]>? Errors
);

Notes:

- Type is a stable identifier for the error category.
- Title is a short summary.
- Status is HTTP status code.
- Detail is a human readable explanation.
- Instance is endpoint path, for example /users.
- Errors is only used for validation errors (field -> list of errors).

### Recommended Type values

- https://jobassistant/errors/validation
- https://jobassistant/errors/not-found
- https://jobassistant/errors/conflict
- https://jobassistant/errors/external-service
- https://jobassistant/errors/unexpected

### Example JSON: 400 Validation failed

```json
{
  "type": "https://jobassistant/errors/validation",
  "title": "Validation failed",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/users",
  "errors": {
    "userName": [
      "Username must be at least 2 characters.",
      "Username contains invalid characters."
    ]
  }
}
```

### Example JSON: 409 Conflict

```json
{
  "type": "https://jobassistant/errors/conflict",
  "title": "Conflict",
  "status": 409,
  "detail": "A user with the same username already exists.",
  "instance": "/users"
}
```

### Example JSON: 404 Not found

```json
{
  "type": "https://jobassistant/errors/not-found",
  "title": "Resource not found",
  "status": 404,
  "detail": "The requested user was not found.",
  "instance": "/user-skills"
}
```

### Example JSON: 502 External service

```json
{
  "type": "https://jobassistant/errors/external-service",
  "title": "External service error",
  "status": 502,
  "detail": "JobStream could not be reached or returned invalid data.",
  "instance": "/jobstream-ads"
}
```

## User - ge honom ett namn

POST Return CreatedAt or 400(length < 2), 409(already exist).

### Request

Record CreateUserRequest(string UserName);

#### Example JSON request

```json
{
  "userName": "anna-andersson"
}
```

### Response

Record CreateUserResponse(string UserName, Guid Id);

#### Example JSON response (201)

```json
{
  "userName": "anna-andersson",
  "id": "7e8a4f5f-6039-4d5f-9e4c-e4871a4e36f4"
}
```

### Errors (ProblemDetails)

- 400 Validation failed (invalid username format or too short)
- 409 Username already exists
- 500 Unexpected server error

## UserSkills - Lägga till skills på användaren

PUT Return 200 and list of added skills on user UserName or 500, or 404(user does not exist)

### Request

Record AddSkillsOnUserRequest(List<string> Skills, string UserName);

#### Example JSON request

```json
{
  "skills": ["c#", ".net", "sql"],
  "userName": "anna-andersson"
}
```

### Response

Record AddSkillsOnUserResponse(List<string> Skills, string UserName);

#### Example JSON response (200)

```json
{
  "skills": ["c#", ".net", "sql"],
  "userName": "anna-andersson"
}
```

### Errors (ProblemDetails)

- 400 Validation failed (invalid username or invalid skills)
- 404 User not found
- 500 Unexpected server error

## JobStreamAds - Läsa nya annonser från JobStream

POST (is POST right here?) This will become something scheduled in the future, but for now,
it should trigger a function in the API that Calls the JobStream API and get the ads from there and then persists them in this APIs database. But before that there should be a check if ads have already been pulled for that timespan (let us check only for time right now, but expand options later). For now consider this to mean no overlap in timespan. If ads have already been pulled, refuse the operation and return 409.
Create rules for mapping Location strings to concept id. For now for Västmanland and the cities there.
Ads are assumed to come in json format per ad. As a preliminary solution, parse them and get a title and the rest of props in JobAd, but the rest as description, only format it according to normal language rules. Use JobStream id as SourceId.

POST is correct here because this endpoint starts an import operation (side effects in your own database).

### Outbound request to JobStream

The API should call:

GET https://jobstream.api.jobtechdev.se/v2/stream

with query params:

- updated-after (required, format YYYY-MM-DDTHH:MM:SS)
- updated-before (optional, same format)
- location-concept-id (optional, repeatable query key)

Header:

- Accept: application/json

Example outbound URL:

https://jobstream.api.jobtechdev.se/v2/stream?updated-after=2026-05-28T10:00:00&updated-before=2026-05-28T10:05:00&location-concept-id=G6DV_fKE_Viz

### Location mapping rules (initial scope: Västmanland)

Use case-insensitive matching for input Location.

- Västmanland or Västmanlands län -> G6DV_fKE_Viz (region concept id)
- Västerås -> 8deT_FRF_2SP
- Arboga -> Jkyb_5MQ_7pB
- Fagersta -> 7D9G_yrX_AGJ
- Hallstahammar -> oXYf_HmD_ddE
- Kungsör -> Fac5_h7a_UoM
- Köping -> 4Taz_AuG_tSm
- Norberg -> jbVe_Cps_vtd
- Sala -> dAen_yTK_tqz
- Skinnskatteberg -> Nufj_vmt_VrH
- Surahammar -> jfD3_Hdg_UhT

If Location is unknown in this initial mapping, return 400 Validation failed.

### Request

Record LoadJobStreamAdsRequest(DateTime FromDateTime, DateTime ToDateTime, string Location);

#### Example JSON request

```json
{
  "fromDateTime": "2026-05-01T00:00:00Z",
  "toDateTime": "2026-05-07T23:59:59Z",
  "location": "Västerås"
}
```

### Response

Record LoadJobStreamAdsResponse(bool AdsAlreadyLoaded, bool OperationAttempted, bool Success);

#### Example JSON response (200)

```json
{
  "adsAlreadyLoaded": false,
  "operationAttempted": true,
  "success": true
}
```

### Errors (ProblemDetails)

- 400 Validation failed (invalid time range or missing location)
- 409 Ads already loaded for overlapping timespan
- 429 Rate limit exceeded from JobStream (mapped from upstream)
- 502 JobStream call failed or returned invalid data
- 500 Unexpected server error

## AdsByFilter - Göra en sökning i laddade jobannonser.

GET AdsByFilter(string Location, string Category, int NumberOfAds)
Make a query from the database and retrieve the ads that match Location and Category. Pack each ad into AdsItem with title and description like above. Limit to numberOfAds. If database have no JobAds, return 404. Otherwise return 200 with list, which will be empty if no records match or otherwise contain the JobAds.

### Response

Record GetAdsResponse(List<AdItem> Ads);

### Errors (ProblemDetails)

- 400 Validation failed (missing filters or invalid NumberOfAds)
- 500 Unexpected server error
- 404 No records exist(if no ads were in the database to be filtered)

## Ads

GET Get the first 10 ads or less.

### Response

Record GetAdsResponse(List<AdItem> Ads);

### Errors (ProblemDetails)

- 500 Unexpected server error
- 404 No records exist
