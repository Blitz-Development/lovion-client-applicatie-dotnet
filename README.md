# Lovion Integration Client (skelet)

## Over dit project
- Dit is een startproject / skelet voor een integratieclient.
- Er is nog geen logica geïmplementeerd, alleen de structuur en mapindeling staan klaar.

## Gebruikte technieken en waarom ze nodig zijn
- **ASP.NET Core Web API** – nodig om later REST-endpoints te bouwen waarmee je data zichtbaar kunt maken voor dashboards of andere systemen.
- **Entity Framework Core** – nodig om later werkorders, assets en import-runs in een SQL-database op te slaan. EF Core biedt makkelijke mapping, migraties en testbaarheid.
- **Configuratie via appsettings.json** – nodig omdat integraties verschillende SOAP-endpoints, tokens of databases gebruiken in DEV/TEST/PROD.
- **Microsoft Logging Extensions** – later cruciaal voor debugging van SOAP-calls, validatiefouten en importfouten.
- **Services & interfaces** – nodig om de architectuur schoon te houden. Businesslogica hoort niet in controllers.
- **DbContext** – nodig als basis voor alle databasebewerkingen. Nu nog leeg, maar klaar om later te vullen.
- **Tests** – nodig om later integratietests te bouwen, bijvoorbeeld voor XML-validatie en SOAP-calls.

## Hoe run je dit project?
1. `dotnet restore`
2. `dotnet build`
3. `dotnet run -p LovionIntegrationClient.Api`

## Hoe ga je dit project uitbreiden?
- SOAP-client genereren uit WSDL.
- XML-validatie met XSD implementeren.
- Mappinglogica bouwen.
- Database vullen en query's bouwen.
- Logging toevoegen.
- Workflow/statuslogica toevoegen.
- REST endpoints uitbreiden.



