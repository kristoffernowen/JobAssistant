# Teknisk Implementation-Brief

## 1. Syfte

Bygg ett .NET 10 ASP.NET Core Web API som:

- Hanterar användare och användarprofiler.
- Importerar jobbannonser från JobStream till lokal databas.
- Filtrerar lagrade annonser.

Systemet ska följa vertical slice-arkitektur och vara förberett för framtida auth via Auth0.

## 2. Scope v1

Ingår i v1:

- CreateUser
- AddSkillsOnUser
- LoadJobStreamAds
- GetAdsByFilter
- GetAds
- ProblemDetails enligt RFC 7807 för alla non-2xx svar
- Geografisk mappning Location -> JobStream concept id för Västmanland

Ingår inte i v1:

- Inloggning och tokenvalidering
- Scheduler för automatisk import
- Multi-source import (men modell förbereds)

## 3. Tekniska beslut

- Plattform: .NET 10, ASP.NET Core Web API, EF Core, SQL Server (lokalt)
- Arkitektur: Vertical Slice (en feature per slice med request, handler, validator, endpoint)
- Felhantering: central middleware för ProblemDetails
- Databasnyckel för annons: unik på SourceType + SourceId
- Username: case-insensitive unik

## 4. Förslag på projektstruktur

- src/JobAssistant.Api
- src/JobAssistant.Application
- src/JobAssistant.Infrastructure
- tests/JobAssistant.Api.Tests
- tests/JobAssistant.Application.Tests
- tests/JobAssistant.IntegrationTests

Slice-exempel i Application:

- Users/CreateUser
- Users/AddSkills
- Ads/LoadJobStreamAds
- Ads/GetAdsByFilter
- Ads/GetAds

## 5. API-kontrakt och beteende

### CreateUser

- Input: UserName
- Regler: minst 2 tecken, giltiga tecken
- Utfall: 201, 400, 409, 500

### AddSkillsOnUser

- Input: UserName, Skills[]
- Regler: user måste finnas, varje skill minst 2 tecken
- Utfall: 200, 400, 404, 500

### LoadJobStreamAds

- Input: FromDateTime, ToDateTime, Location
- Regler:
  - FromDateTime < ToDateTime
  - Location måste mappas till concept id
  - ingen overlap med tidigare importintervall
- Utfall: 200, 400, 409, 429, 502, 500

### GetAdsByFilter

- Input: Location, Category, NumberOfAds
- Utfall: 200 eller 404 om databasen saknar annonser, plus 400 och 500

### GetAds

- Input: inget
- Utfall: 200 eller 404 om databasen saknar annonser, plus 500

## 6. Datamodell och persistens

### UserProfile

- Id (Guid)
- UserName (unik, case-insensitive)
- Skills (normaliserad tabell UserSkill rekommenderas)

### JobAd

- SourceType (string, default JobStream)
- SourceId (string)
- Title (string)
- Description (string)
- Location (string)
- Category (string)
- Loaded (DateTime UTC)
- Inactive (bool)

Index och constraints:

- Unique: SourceType + SourceId
- Index: Loaded
- Index: Location, Category

## 7. JobStream-integration

Endpoint som anropas:

- GET https://jobstream.api.jobtechdev.se/v2/stream

Query-parametrar:

- updated-after (required)
- updated-before (optional)
- location-concept-id (repeatable)

Headers:

- Accept: application/json

Timeout och resiliency:

- HttpClientFactory
- Timeout 30 sekunder
- Enkel retry med exponential backoff för transient fel
- Särskild hantering för 429 (retry-after om header finns)

## 8. Mappning Location till concept id (v1)

Region:

- Västmanland, Västmanlands län -> G6DV_fKE_Viz

Kommuner:

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

Regel:

- Case-insensitive match
- Okänd location -> 400 validation error

## 9. Mappning från JobStream-ad till JobAd

- SourceId <- id
- SourceType <- JobStream
- Title <- headline
- Description <- description.text
- Location <- workplace_address.municipality, fallback workplace_address.region
- Category <- occupation_field.label
- Loaded <- UtcNow vid persist
- Inactive <- removed

Upsert-regel:

- removed true: sätt Inactive true för befintlig annons
- removed false: skapa eller uppdatera annons med samma SourceType + SourceId

## 10. ProblemDetails-standard

Alla fel returnerar samma form:

- type
- title
- status
- detail
- instance
- errors (endast validering)

Type-värden:

- https://jobassistant/errors/validation
- https://jobassistant/errors/not-found
- https://jobassistant/errors/conflict
- https://jobassistant/errors/external-service
- https://jobassistant/errors/unexpected

## 11. Logging och observability

Logga minst:

- Correlation id per request
- Start/slut för import
- Antal hämtade ads
- Antal skapade, uppdaterade, inaktiverade
- Upstream-statuskod från JobStream

Mätetal:

- Import duration
- Imported ads count
- Failed imports count

## 12. Teststrategi

Enhetstester:

- Validering av inputregler
- Location-mappning
- Mappning JobStream -> JobAd

Integrationstester:

- EF Core mot testdatabas
- Upsert-flöde
- Overlap-kontroll

API-tester:

- Statuskoder per endpoint
- ProblemDetails-format

## 13. Leveransordning

1. Skapa grundprojekt, EF Core context, entities, migration.
2. Implementera ProblemDetails middleware.
3. Implementera Users-slices.
4. Implementera LocationMappingService.
5. Implementera JobStreamClient.
6. Implementera LoadJobStreamAds-slice med overlap-kontroll och upsert.
7. Implementera AdsByFilter och Ads.
8. Lägg tester och justera felhantering.

## 14. Definition of Done för v1

- Samtliga endpoints fungerar enligt dokumenterade kontrakt.
- Location-mappning för Västmanland fungerar.
- Import med tidsintervall fungerar och blockeras vid overlap.
- ProblemDetails returneras konsekvent för alla non-2xx.
- Tester täcker kritisk logik och passerar.
