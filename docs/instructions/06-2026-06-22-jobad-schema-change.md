# 2026-06-22 JobAd Schema Change

## Instruktion

Uppdatera `JobAd` och databas enligt modell med indexerade sökfält + full JSON-data.

## Fält och mapping

- Behåll indexerade sökfält.
- Lägg till:
  - `OccupationGroup` (från `occupation_group.label`)
  - `OccupationField` (från `occupation_field.label`)
- `Location` ska använda `workplace_address.municipality`.
- Tidigare `Category` ersatts av `OccupationGroup` och `OccupationField`.

## Kvarvarande fält

Följande ska vara kvar tills vidare:

- `SourceType` (default `JobStream`)
- `Loaded`
- `Inactive`

## Endpoint/kontrakt

- Uppdatera `GetAdsEndpoint` till nya modellen.
- Uppdatera `GetAdsContracts` så att `AdItem` utökas med:
  - `Location`
  - `OccupationGroup`
  - `OccupationField`
- `Description` ska innehålla `description.text` från deserialiserat JSON-objekt.
