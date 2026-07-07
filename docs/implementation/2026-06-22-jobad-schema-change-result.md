# 2026-06-22 JobAd Schema Change Result

## Sammanfattning

Allt genomfört enligt plan.

## Genomfört

1. Uppdaterat `JobAd` med nya fält: `OccupationGroup`, `OccupationField`, `PublicationDate`, `Removed`, `FullData` (JSON column).
2. Tagit bort `Category` från entiteten.
3. Uppdaterat `JobStreamAdDto` med `text_formatted`, `occupation_group`, `publication_date`.
4. Uppdaterat EF Core-konfiguration för JSON-kolumn och nya index.
5. Uppdaterat `LoadJobStreamAdsEndpoint` mapping till nya fält.
6. Uppdaterat `GetAdsContracts` med nya response-fält.
7. Uppdaterat `GetAdsEndpoint` projektion.
8. Uppdaterat `GetAdsByFilter` från category till occupationField.
9. Uppdaterat tester efter nya schema.
10. Skapat migration `UpdateJobAdSchema`.
11. Uppdaterat databasen med nya kolumner och index.

## Verifiering

- Build succeeded.
- Samtliga tester gröna i det aktuella steget.
- Databasen uppdaterad.
