# JobAssistant

JobAssistant är ett .NET Web API för jobbsökning.
API:t hämtar annonser från Arbetsförmedlingens API:er, filtrerar resultat och stödjer både stateless sökning och sessionsbaserad refine.

## Syfte

Målet är att bygga en praktisk assistent för jobbsökning med tydliga API-kontrakt, robust validering och spårbar utveckling.

## Funktioner

- Sök annonser via extern källa i realtid
- Filtrering med validerade parametrar
- Sessionsflöde för refine utan nytt upstream-anrop
- Enhetlig felhantering via ProblemDetails (RFC 7807)
- Dokumenterad beslutshistorik och implementation

## Arkitektur och teknik

- ASP.NET Core 10 Web API
- Entity Framework Core
- SQL Server (lokalt)
- Vertical Slice Architecture
- FluentValidation
- xUnit

## Snabbstart lokalt

Förutsättningar:

- .NET SDK 10
- SQL Server lokal utvecklingsmiljö

Kommandoöversikt:

```bash
dotnet restore
dotnet build
dotnet test
```

Köra API lokalt:

```bash
dotnet run --project src/JobAssistant.Api
```

## API-översikt

Fullständiga endpoint-kontrakt finns i docs/instructions/01-endpoints-contract.md.

## Tester och kvalitet

Projektet innehåller API-, applikations- och integrationstester.
Fokus är att verifiera validering, felmappning, filtreringslogik och integrationsflöden.

## AI-samarbete

Detta projekt är byggt som AI-assisted development.
Arbetssättet har varit att styra implementationen via tydliga instruktioner till agent, följt av granskning, iteration och verifiering med tester.

Metoder som använts i praktiken:

- Prompt-driven kravspecifikation
- Stegvis leveransstyrning och uppföljning
- Beslutsspårning med ADR
- Dokumentation av instruktioner, beslut och resultat

## Dokumentation

Dokumentationsindex finns i docs/README.md.

## Roadmap

- Förbättrad ranking och fler filterstrategier
- Förbereda cache/session för flera instanser
- Vidareutveckla användarprofilflöden
