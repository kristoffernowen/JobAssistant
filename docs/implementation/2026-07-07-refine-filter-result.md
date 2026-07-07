# 2026-07-07 Refine Filter Result

## Genomfört

1. Uppdaterat refine-request med:
   - `MustContain`
   - `MustNotContain`
2. Lagt till separat handler för refine-filtrering i `SearchAds`.
3. Implementerat regler:
   - `MustContain` = AND
   - `MustNotContain` = excludes on any match
   - case-insensitive substring
   - trim + ignorera tomma termer
4. Sökfält inkluderar:
   - `headline`
   - `description.text`
   - `occupation` (inkl underobjekt)
   - `must_have` (inkl underobjekt)
   - `nice_to_have` (inkl underobjekt)
5. Endpointen `/ads/sessions/{sessionId}/refine` kopplad till nya handlern.
6. DTO utökad med `occupation`, `must_have`, `nice_to_have` som `JsonElement` för sökning i underobjekt.
7. Tester uppdaterade/utökade för refine-regler.

## Verifiering

- `dotnet test tests/JobAssistant.Api.Tests/JobAssistant.Api.Tests.csproj` passerar.
- `dotnet test` för hela lösningen passerar.
- Totalt i steget: 17 tester gröna, 0 fel.
