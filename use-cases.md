# Use Cases

## CreateUser - ge honom ett namn

Validering:
Namn endast bokstäver eller tecken associerade med namn, som -, ` etc.
Längre än två tecken.
Skapa användare med username och Guid id. Skills är en tom lista.

## AddSkillsOnUser - Lägga till skills på användaren

Validering username:
Namn endast bokstäver eller tecken associerade med namn, som -, ` etc.
Längre än två tecken.
UserExists.
Validering skills:
Varje sträng är längre än två tecken.

## LoadAds - Läsa nya annonser från JobStream

Utifrån tidsintervall och platsbegränsning.

Validering:
FromDateTime < ToDateTime.
Location ska kunna mappas till concept id enligt nuvarande regelverk (Västmanland + kommuner).

Kontroll före hämtning:
Kontrollera om tidsintervallet overlappar ett redan hämtat intervall.
Om overlap finns returnera 409 och avbryt.

Hämtningslogik:
Anropa JobStream v2 stream med updated-after och updated-before.
Skicka location-concept-id baserat på mappning av Location.

Mappning till JobAd:
SourceId <- id
Title <- headline
Description <- description.text
Location <- workplace_address.municipality, annars workplace_address.region
Category <- occupation_field.label
Loaded <- current UTC time when persisted
Inactive <- removed

Om removed är true markera annonsen som Inactive i databasen.
Om removed är false skapa eller uppdatera annons baserat på SourceId.

## GetAdsByFilter - Göra en sökning i laddade jobannonser.

Från början ska det filtreras på plats och ytterligare ett sökkriterie. Filtrera case insensitive.

## GetAds

Ladda upp till 10 första ads från databasen, så jag kan studera objekten.
