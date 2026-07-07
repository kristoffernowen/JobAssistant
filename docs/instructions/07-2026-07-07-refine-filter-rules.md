# 2026-07-07 Refine Filter Rules

## Instruktion

Uppdatera `RefineSearchAdsSessionRequest` till:

- `List<string> MustContain`
- `List<string> MustNotContain`

Bygg en handler i `SearchAds`-mappen som filtrerar sessionsannonser utifrån refine-request.

## Regler

- `MustContain`: alla termer måste finnas, annars exkluderas annonsen.
- `MustNotContain`: inga termer får finnas, annars exkluderas annonsen.
- Trimma whitespace på varje sökterm.
- Ignorera tomma söktermer.
- Matchning är case-insensitive substring.
- Null/tomma annonsfält räknas som ingen text i det fältet.
- En term är träff om den finns i minst ett av de genomsökta fälten.

## Sökfält

- `headline`
- `description`
- `occupation`
- `must_have`
- `nice_to_have`

Sök underliggande objekt också.

## Också

- Lägg till antal annonsträffar i `messages` i response.
- Flytta `messages` före `ads` i response.
