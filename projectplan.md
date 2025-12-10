# Projectplan – Lovion Integration Client

Context: het skelet staat al klaar met `LovionIntegrationClient.Api`, `LovionIntegrationClient.Core`, `LovionIntegrationClient.Infrastructure`, `LovionIntegrationClient.Tests`. Controllers, services, `IntegrationDbContext`, `appsettings.json` en README bestaan al als lege basis. Onderstaande stappen vullen het skelet stapsgewijs, zonder direct veel logica te implementeren.

## Begrippen (kort en simpel)
- Dependency Injection (DI): het framework maakt en geeft je objecten (services, DbContext, repos) zodat je zelf geen `new` hoeft te doen. Registratie gebeurt in `LovionIntegrationClient.Api/Program.cs` met regels als `builder.Services.AddScoped<IMyService, MyService>();`.
- DbContext: EF Core-poort naar de database. Staat in `LovionIntegrationClient.Infrastructure/Persistence/IntegrationDbContext.cs`. DbSets zijn tabellen.
- Migrations: EF Core-commando’s die een migratiebestand maken (`dotnet ef migrations add ...`) en de database bijwerken (`dotnet ef database update ...`).
- DTO: Data Transfer Object; lichte versies van je entiteiten die je via REST teruggeeft.
- Mapping: het omzetten van entiteiten ↔ DTO’s, bijv. in een aparte mappingklasse of in de service.

## Snelstart (commando’s)
- Restore/build/run:
  - `dotnet restore`
  - `dotnet build`
  - `dotnet run -p LovionIntegrationClient.Api`
- EF Core na provider/connectionstring-keuze:
  - `dotnet ef migrations add InitialCreate -p LovionIntegrationClient.Infrastructure -s LovionIntegrationClient.Api`
  - `dotnet ef database update -p LovionIntegrationClient.Infrastructure -s LovionIntegrationClient.Api`

## Belangrijke plekken in de codebase
- API bootstrap, pipeline, DI-registraties: `LovionIntegrationClient.Api/Program.cs`
- Configuratie (connectionstrings, SOAP): `LovionIntegrationClient.Api/appsettings.json`
- Controllers: `LovionIntegrationClient.Api/Controllers/AssetController.cs`, `WorkOrderController.cs`, `SystemController.cs`
- Domeinmodellen: `LovionIntegrationClient.Core/Domain/`
- Services/interfaces: `LovionIntegrationClient.Core/Services/` (+ implementaties in `Services/Implementations/`)
- DbContext: `LovionIntegrationClient.Infrastructure/Persistence/IntegrationDbContext.cs`
- Repositories: `LovionIntegrationClient.Infrastructure/Repositories/`
- SOAP placeholder: `LovionIntegrationClient.Infrastructure/Soap/SoapWorkOrderClient.cs`
- README (globaal overzicht): `README.md`

## Fase 0 – Kennismaken met het skelet
Doel: Begrijp de structuur voordat je code schrijft. Bekijk namespaces, controllers, services, DbContext en configuratie. Open `Program.cs` om te zien wat er in DI zit en welke middleware (Swagger, controllers) draait.

## Fase 1 – Database & EF Core werkend maken
- Provider kiezen (nu SQLite voorbereid in `Program.cs`): pas zo nodig `DefaultConnection` in `appsettings.json` aan. Voor SQLite staat er standaard `Data Source=integration.db`.
- Check `IntegrationDbContext`: DbSets staan klaar. Laat ze zo; later kun je Fluent API configuratie toevoegen.
- Migraties uitvoeren:
  - `dotnet restore`
  - `dotnet ef migrations add InitialCreate -p LovionIntegrationClient.Infrastructure -s LovionIntegrationClient.Api`
  - `dotnet ef database update -p LovionIntegrationClient.Infrastructure -s LovionIntegrationClient.Api`
- Optioneel: eenvoudige seeder (bijv. in `Program.cs` of aparte seederklasse) die een paar Assets/WorkOrders toevoegt bij start, zodat je endpoints iets kunnen teruggeven.

## Fase 2 – Basis-REST API voor Assets en WorkOrders
- Endpoints toevoegen in controllers:
  - `AssetController`: `GET /api/assets`
  - `WorkOrderController`: `GET /api/workorders`
- DTO’s definiëren (bijv. `AssetDto`, `WorkOrderDto`) in een map `Dtos` of `Contracts`.
- Mapping toevoegen (kan in services of een helper in `Infrastructure/Mappings/`):
  - Entity → DTO: alleen velden die je wilt exposen.
- Services (`IAssetService`, `IWorkOrderService`) uitbreiden met leesmethodes zoals `GetAllAssetsAsync()`, `GetAllWorkOrdersAsync()`. Controllers roepen alleen services aan, geen DbContext direct.
- DI-registratie controleren in `Program.cs`: `AddScoped<IAssetService, AssetService>();` is al aanwezig. Zo weet je waar DI voor staat: het framework injecteert deze service in de controllers.

## Fase 3 – SOAP-client naar Spring Boot dummy backend
- Gebruik `Infrastructure/Soap/SoapWorkOrderClient.cs` en genereer een client uit de WSDL (bijv. met `dotnet-svcutil` of een andere tooling).
- Voeg een methode toe zoals `Task<IReadOnlyList<SoapWorkOrderDto>> GetWorkOrdersAsync();` (alleen skeleton, nog geen zware logica).
- In `IWorkOrderService` en implementatie: `ImportWorkOrdersFromSoapAsync` die de SOAP-client aanroept en de resultaten teruggeeft (nog niet opslaan).
- Bewaar zo mogelijk de ruwe XML voor validatie en logging (handig voor Fase 4).

## Fase 4 – XML-afhandeling en XSD-validatie
- Maak of verkrijg een XSD voor de werkorder-XML.
- Voeg een validatiemethode toe (bijv. `ValidateWorkOrderXml(string xml)`) in een aparte helper/validator.
- In de importflow: valideer vóór opslag. Bij validatiefout: sla niet op, registreer later een `ImportError` (zie Fase 5).

## Fase 5 – Importlogica (Imports & ImportErrors)
- Gebruik `ImportRun` en `ImportError` uit `Core/Domain/`.
- Proces:
  - Start run → `ImportRun.Status = RUNNING`, `StartedAtUtc = now`.
  - Per werkorder: valideren; bij fout → `ImportError` vastleggen met referentie naar de bron/ExternalId.
  - Einde run: status zetten op SUCCESS / PARTIAL_SUCCESS / FAILED; `CompletedAtUtc` invullen.
- Eventueel API-endpoint toevoegen, bijv. `GET /api/imports` om importgeschiedenis in te zien.

## Fase 6 – Workflowstatus en businessregels
- Breid `WorkOrder` uit met statusveld (IMPORTED, VALIDATION_FAILED, READY, PROCESSED).
- Definieer simpele regels: na succesvolle import → IMPORTED; bij validatiefout → VALIDATION_FAILED; na handmatige/automatische verwerking → PROCESSED.
- Endpoints laten filteren op status (`/api/workorders?status=IMPORTED`).

## Fase 7 – Logging en foutafhandeling uitbreiden
- Voeg echte logging toe waar TODO’s staan:
  - Start/einde import, aantal opgehaalde werkorders.
  - Validatiefouten met ExternalId.
  - Databasefouten met ERROR-niveau en exceptiondetails.
- Overweeg middleware of filter voor ongehandelde exceptions zodat responses netjes blijven en errors gelogd worden.
- Zorg voor logniveaus (INFO/WARN/ERROR) en eventueel correlation ids in logregels.

## Fase 8 – (Optioneel) Kleine UI of monitoring
- Maak een simpele frontend (React/Razor/HTML) die:
  - Aantallen werkorders per status toont.
  - Laat zien wanneer de laatste import-run was en met welk resultaat.
  - ImportErrors toont met details.
- De UI praat tegen je eigen REST API.

## Checklists per fase (praktisch)
- Algemene basis: `dotnet restore`, `dotnet build`, `dotnet run -p LovionIntegrationClient.Api`
- Fase 1: provider kiezen, `appsettings.json` invullen, migratie + update draaien.
- Fase 2: DTO’s + mapping, services vullen, controllers laten lezen via services.
- Fase 3: SOAP client genereren uit WSDL, service-methode voor import.
- Fase 4: XSD-validatie toevoegen vóór opslag.
- Fase 5: ImportRun/ImportError bijhouden, status en timestamps vullen.
- Fase 6: Statusmodel en filters in API.
- Fase 7: Logging op alle kritieke paden, exception-handling middleware.
- Fase 8: (optioneel) UI/dashboard koppelen op de bestaande API.

## Tips voor beginners zonder .NET-ervaring
- Start altijd met `dotnet restore` na het clonen.
- Kijk in `Program.cs` om te zien hoe DI werkt: elke `AddScoped<Interface, Implementatie>()` betekent “het framework maakt dit object en geeft het mee in de constructor van je controller of service”.
- Gebruik Swagger (automatisch aan in dev) via `/swagger` om endpoints te testen.
- Houd mapping/DTO’s buiten controllers; services en mappings houden de code schoon.
- Zet EF Core migrations in source control zodat iedereen dezelfde databasebasis heeft.

