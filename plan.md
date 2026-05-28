# Syfte

En assistent till mig själv för att söka jobb. Det ska vara ett api som ska:
Hämta annonser
Filtrera annonser
Hantera min sökprofil

# Arkitektur

Vertical slice

# Teknik

Använd .NET 10 asp.net core web api.
För lokal utveckling använd MS SQL och ef core.

# Integrationer

Arbetsförmedlingen Sverige Platsbanken JobStream APIet.

# Features

Hantera olika användare som skapar en profil för att hantera jobbsökningar
Förbered för att kunna lägga till inloggning via Auth0.
Läsa jobbannonser från JobStream
Filtrera annonser utifrån valda kriterier - specificeras i use cases

# Klargöranden

Användare skapas med username. Låt det vara case insensitive. Det bör räcka för att hålla reda på just nu. Till att börja med
används ändå strings i request, där jag manuellt skriver in skills och location. Mer om det längre fram.
JobStream finns här https://jobstream.api.jobtechdev.se/ Har du möjlighet att läsa det du behöver för att konfigurera api anropet, eller behöver jag hitta den info själv?
Gör exempel hämtningar utifrån Location Västmanland alla orter. Bygg funktionalitet för att översätta ortsnamn i Västmanland till de koncept id som jobstream använder.
