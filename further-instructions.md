# further-instructions

## important details

Issue, commit, affected areas, time?

2026-06-22

Jag skulle vilja se hur de råa jsonobjekten från jobstream ser ut, vilka datafält som finns. Lägg till loggnings funktionalitet som skriver till en textfil i en mapp i projktet. Det räcker att den loggar jsonobjektet från jobstream och de tre första objekten räcker. I JobStreamClient laddas JobStream och deserialiseras till ett jobstreamdto, men jag skulle vilja ha ett loggat jsonobjekt som visar det råa objektet som kommer in.

### copilot eget förslag

Givet på frågan om att uppdatera db entity.
Smart approach:

1. Spara hela JSON-objektet i ett string eller JsonDocument-fält
2. Ha enkla, indexerade fält för sökning/filtrering
3. Lazy-load full data när användaren öppnar en annons

schema: public sealed class JobAd
{
// Indexerade sökfält (snabba queries)
public string SourceId { get; set; }
public string Title { get; set; }
public string Location { get; set; }
public DateTime PublicationDate { get; set; }
public bool Removed { get; set; }

    // EF Core 5+ med JSON columns:
    // public JobStreamAdDto FullData { get; set; }

}

Uppdatera jobad entity och db enligt förslaget precis ovan och med följande tillägg. Jsonobjektet har även "occupation_group": {
"concept_id": "BStc_SJh_DKG",
"label": "Kockar och kallsk\u00E4nkor",
"legacy_ams_taxonomy_id": "5120"
},
"occupation_field": {
"concept_id": "ScKy_FHB_7wT",
"label": "Hotell, restaurang, storhush\u00E5ll",
"legacy_ams_taxonomy_id": "7"
}. Både dessa label behöver också bli props i jobad med index:
public string OccupationGroup (använder alltså label som värde)
public string OccuptaionField (använder alltså label som värde)

Location ska använda workplace_address:municaplity
Category ersätts alltså av OccuptaionGroup och OccuptaionField

Då återstår dessa i den gamla modellen. Jag tror att de behöver vara kvar för att övrig logik ska fungera. Låt dem vara kvar så länge.

public string SourceType { get; set; } = "JobStream";

public DateTime Loaded { get; set; }

public bool Inactive { get; set; }

Uppdatera så att GetAdsEndpoint fungerar med den nya modellen.
Uppdatera denna i GetAdsContracts: public sealed record AdItem(string Title, string Description); lägg till string Location, string OccupationGroup, string OccupationField. Ändra logiken så att description innehåller description:text från det deserialiserade jsonobjektet.
