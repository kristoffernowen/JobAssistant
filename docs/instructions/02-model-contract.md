## UserProfile

Guid id
Username
Skills: String List, where each item is a word, phrase or sentence to be used in LINQ expressions to filter the jobads.

## JobAd

Title string
Description string
SourceId string (jag tog bort Guid id, för det bör väl räcka med source id som unikt id. säg bar till om du bedömer att det antagandet är fel.)
SourceType string (default JobStream, förbereder för fler källor senare)
Location string
Category string
Loaded DateTime
Inactive bool

Kommentar:
SourceId som unikt id fungerar bra i första versionen om du bara använder JobStream.
Om du i framtiden lägger till fler källor, använd unik nyckel på SourceType + SourceId.
