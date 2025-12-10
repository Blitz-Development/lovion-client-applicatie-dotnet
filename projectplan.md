# Projectplan – Lovion Integration Client

## Doel van het project

Je hebt een Lovion Integration Client in .NET die koppelt met een Lovion dummy backend in Spring Boot.

Samen vormen ze een oefenlandschap waarin je alles traint wat bij integraties rond Lovion-achtige systemen speelt: SOAP, XML/XSD, SQL, REST, logging, status/workflow en configuratie.

### Rol van de Spring Boot dummy backend

De Spring Boot backend speelt het externe systeem na (bijvoorbeeld GIS/ERP) dat werkorders aanbiedt.

Deze backend levert:

- Een SOAP-service (`GetWorkOrders`, eventueel `GetWorkOrderDetails`)
- Een eigen database met `Asset` en `WorkOrder`
- Een REST-API om data te bekijken (handig voor testen en vergelijken)

De backend is dus de "bron" van werkorders waar jouw .NET-client later tegenaan praat.

### Rol van de .NET Lovion Integration Client

De .NET-client is de integratielaag: een soort mini-Lovion-koppeling.

Die client gaat op termijn:

- Werkorders ophalen via SOAP bij de Spring Boot backend
- De XML van die berichten valideren met XSD
- Alles opslaan in een eigen SQL-database (EF Core)
- Import-runs en fouten registreren
- Werkorders een status geven (`IMPORTED`, `FAILED`, `PROCESSED`, …)
- Een eigen REST-API aanbieden waarmee je de verwerkte data en import-historie kunt opvragen
- Uitgebreid loggen wat er gebeurt (voor debugging en inzicht)

De studenten bouwen alleen aan deze .NET-client; de Spring Boot backend is hun "buitenwereld".

### Fasen op hoofdlijnen

Je kunt het traject in een paar grote blokken zien:

**Structuur en database**

Skeletproject begrijpen, EF Core aansluiten, database en basis-entiteiten (`Asset`, `WorkOrder`, `ImportRun`, `ImportError`) actief maken, en simpele REST-endpoints die data teruggeven.

**Integratie met externe bron (SOAP)**

SOAP-client genereren tegen de Spring Boot backend, werkorders ophalen en testen via een test-endpoint (nog niet opslaan).

**Validatie en import**

XML/XSD-validatie toevoegen, import-runs en fouten opslaan, werkorders in de lokale database schrijven met een status.

**Workflow en inzicht**

Status-overgangen bouwen (bijv. `IMPORTED` → `PROCESSED`), REST-filters op status, logging en foutafhandeling verbeteren, en optioneel een klein dashboard/UI dat de integratie zichtbaar maakt.

### Wat studenten hier inhoudelijk van leren

Aan het eind hebben ze geoefend met:

- Werken in een bestaand skelet (instappen in een bestaande codebase)
- EF Core, migrations, DbContext en configuratie
- Opzetten van nette REST-API's met services en DTO's
- Koppelen met een externe SOAP-service en omgaan met WSDL-gegenereerde types
- XML en XSD-validatie als onderdeel van datakwaliteit
- Ontwerpen van importflows, errorlogging en audit-trails
- Modellen van status en eenvoudige workflowlogica
- Logging en foutafhandeling op een manier die beheer en debugging echt helpt

---

Context: het skelet staat al klaar met `LovionIntegrationClient.Api`, `LovionIntegrationClient.Core`, `LovionIntegrationClient.Infrastructure`, `LovionIntegrationClient.Tests`. Controllers, services, `IntegrationDbContext`, `appsettings.json` en README bestaan al als lege basis. Onderstaande stappen vullen het skelet stapsgewijs, zonder direct heel complexe logica te implementeren.

## Begrippen (kort en simpel)

Je gebruikt deze begrippen in bijna elke fase:

### Dependency Injection (DI)

Het framework maakt en beheert objecten (services, DbContext, repositories) en injecteert ze via constructors.

Registratie gebeurt in `LovionIntegrationClient.Api/Program.cs` met regels als:

```csharp
builder.Services.AddScoped<IMyService, MyService>();
```

Dit betekent: als ergens `IMyService` in een constructor staat, levert .NET automatisch een instantie van `MyService`.

### DbContext

EF Core-poort naar de database.

**Bestandslocatie:** `LovionIntegrationClient.Infrastructure/Persistence/IntegrationDbContext.cs`

`DbSet<Asset> Assets` gedraagt zich als een "virtuele tabel" Assets in de database.

### Migrations

EF Core-commando's om databasewijzigingen bij te houden.

- `dotnet ef migrations add InitialCreate ...` → beschrijft schema-wijziging.
- `dotnet ef database update ...` → past schema toe op de echte database.

### DTO (Data Transfer Object)

Eenvoudige klasse die je via REST naar buiten stuurt.

Meestal minder of andere velden dan de database-entiteit.

Helpt om intern model los te koppelen van API-contracten.

### Mapping

Het omzetten van entiteiten ↔ DTO's.

Kan in service, aparte mapperklasse, of met een library.

Zorgt dat je databasevorm niet 1-op-1 je API dicteert.

## Snelstart (commando's)

Algemene commando's die je door het hele traject blijft gebruiken:

### Project binnenhalen en draaien:

```bash
dotnet restore
dotnet build
dotnet run -p LovionIntegrationClient.Api
```

### EF Core na provider/connectionstring-keuze:

```bash
dotnet ef migrations add InitialCreate -p LovionIntegrationClient.Infrastructure -s LovionIntegrationClient.Api
dotnet ef database update -p LovionIntegrationClient.Infrastructure -s LovionIntegrationClient.Api
```

**Leren:** dit zijn de basishandelingen van een .NET-ontwikkelaar: dependency herstellen, compileren, migraties draaien en het webproject starten.

## Belangrijke plekken in de codebase

- **Bootstrap / pipeline / DI:**
  - `LovionIntegrationClient.Api/Program.cs`
- **Configuratie:**
  - `LovionIntegrationClient.Api/appsettings.json`
- **Controllers (in- en uitgangen van je API):**
  - `LovionIntegrationClient.Api/Controllers/AssetController.cs`
  - `LovionIntegrationClient.Api/Controllers/WorkOrderController.cs`
  - `LovionIntegrationClient.Api/Controllers/SystemController.cs`
- **Domeinmodellen (kern-data):**
  - `LovionIntegrationClient.Core/Domain/`
- **Services/interfaces:**
  - `LovionIntegrationClient.Core/Services/` en implementaties (bijv. `Services/Implementations/`)
- **DbContext:**
  - `LovionIntegrationClient.Infrastructure/Persistence/IntegrationDbContext.cs`
- **Repositories (optionele abstractielaag boven DbContext):**
  - `LovionIntegrationClient.Infrastructure/Repositories/`
- **SOAP placeholder:**
  - `LovionIntegrationClient.Infrastructure/Soap/SoapWorkOrderClient.cs`
- **README:**
  - `README.md`

**Leren:** deze structuur lijkt op veel echte enterprise-opzetten. Door dit pad in je hoofd te hebben, kun je later ook in andere projecten sneller navigeren.

## Fase 0 – Kennismaken met het skelet

**Doel:** de structuur begrijpen vóór je logica toevoegt.

### Concrete stappen

1. Open de solution in je IDE (Rider, VS, VS Code).
2. Open `Program.cs` in `LovionIntegrationClient.Api`:
   - Bekijk:
     - `builder.Services....` (DI-registraties)
     - `app.UseRouting()`, `app.MapControllers()`, eventueel Swagger-config.
3. Loop langs de controllers:
   - `SystemController` met `GET /health` (of vergelijkbaar).
   - Kijk welke routes er al zijn, al is het alleen skeleton.
4. Open `IntegrationDbContext`:
   - Controleer welke DbSets zijn voorbereid.
5. Lees 'snel' de README:
   - Begrijp: dit is een basis, niet het eindproduct.

### Wat leer je hier?

- Waar je wat moet aanpassen voor de rest van het project.
- Hoe DI en de pipeline in .NET ongeveer werken.
- Dat je niet "in het wilde weg" gaat programmeren, maar bewust in lagen.

## Fase 1 – Database & EF Core werkend maken

**Doel:** een werkende databaseverbinding en schema opzetten.

### Concrete taken

#### Provider kiezen en configureren

1. Open `appsettings.json` in `LovionIntegrationClient.Api`.
2. Controleer of er iets staat als:

```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=integration.db"
}
```

Dit betekent: SQLite-bestand `integration.db` in de output-folder.

#### IntegrationDbContext controleren

In `Infrastructure/Persistence/IntegrationDbContext.cs`:

- Zorg dat `DbSet<Asset>`, `DbSet<WorkOrder>`, `DbSet<ImportRun>`, `DbSet<ImportError>` aanwezig zijn.
- Eventueel `[Key]`- en relatie-attributen toevoegen als die nog niet bestaan.
- Let op naamgeving (tabellen krijgen default de naam van de DbSet).

#### DbContext registreren in DI

In `Program.cs`:

Controleer dat er iets is als:

```csharp
builder.Services.AddDbContext<IntegrationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

Zo niet: voeg dit toe (met de juiste provider).

#### Migratie en database aanmaken

In de rootmap van de solution:

```bash
dotnet restore
dotnet ef migrations add InitialCreate -p LovionIntegrationClient.Infrastructure -s LovionIntegrationClient.Api
dotnet ef database update -p LovionIntegrationClient.Infrastructure -s LovionIntegrationClient.Api
```

Controleer of het `integration.db` bestand is aangemaakt.

#### Eenvoudige seeder (optioneel, maar handig)

1. Maak een `DataSeeder` class in `Infrastructure` of `Api`.
2. In `Program.cs` na `app.Run()`-aanmaak (of net ervoor) roep je iets aan als:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IntegrationDbContext>();
    // TODO: seed hier bv. 2 assets en 3 workorders
}
```

### Wat leer je hier?

- Hoe EF Core, DbContext en de echte database aan elkaar gekoppeld zijn.
- Hoe migrations de structuur van je database beschrijven.
- Dat een domeinmodel pas "echt" is als het in een database leeft.

## Fase 2 – Basis-REST API voor Assets en WorkOrders

**Doel:** data via API kunnen lezen met een nette laagstructuur.

### Concrete taken

#### DTO's maken

1. Maak in `LovionIntegrationClient.Core` een map `Dtos` of `Contracts`.
2. Voeg toe:
   - `AssetDto` met velden als: `Id`, `ExternalAssetRef`, `Type`, `Description`, `Location`.
   - `WorkOrderDto` met velden als: `Id`, `ExternalWorkOrderId`, `WorkType`, `Priority`, `ScheduledDate`, `Status`, eventueel `AssetId`.

#### Services uitbreiden

In `Core/Services/IAssetService.cs`:

Voeg methodesignatures toe:

```csharp
Task<IReadOnlyList<AssetDto>> GetAllAssetsAsync();
Task<AssetDto?> GetAssetByIdAsync(int id);
```

In `IWorkOrderService.cs`:

```csharp
Task<IReadOnlyList<WorkOrderDto>> GetAllWorkOrdersAsync();
Task<WorkOrderDto?> GetWorkOrderByIdAsync(int id);
```

#### Service-implementaties invullen

In `Core/Services/Implementations/AssetService.cs` (of in `Infrastructure`, afhankelijk van je structuur):

1. Injecteer `IntegrationDbContext`.
2. Implementeer `GetAllAssetsAsync`:

```csharp
var entities = await _db.Assets.ToListAsync();
Map entities naar List<AssetDto>.
```

Idem voor `WorkOrderService`.

#### Controllers koppelen aan services

In `AssetController`:

- Constructor met `IAssetService assetService`.
- `GET /api/assets` → `return Ok(await _assetService.GetAllAssetsAsync());`
- `GET /api/assets/{id}` → `Ok/NotFound` op basis van `GetAssetByIdAsync`.

In `WorkOrderController`:

- Vergelijkbare endpoints voor workorders.

#### DI controleren

In `Program.cs`:

Zorg dat er registraties zijn als:

```csharp
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IWorkOrderService, WorkOrderService>();
```

#### Testen via Swagger/Postman

1. Start de API.
2. Ga naar `/swagger`.
3. Roep `GET /api/assets` en `GET /api/workorders` aan.
4. Verwacht: lege lijst of seed-data.

### Wat leer je hier?

- De basis van een nette layered architecture: Controller → Service → DbContext.
- Het nut van DTO's en waarom je entiteiten niet direct blootstelt.
- Het gebruik van `async/await` in een web-API.

## Fase 3 – SOAP-client naar Spring Boot dummy backend

**Doel:** werkorders ophalen via SOAP i.p.v. alleen uit je eigen database.

### Concrete taken

#### WSDL-bron bepalen

Leg vast welke URL de Spring Boot dummy backend exposeert, bijvoorbeeld:

```
http://localhost:8080/ws/workorders.wsdl
```

Vul deze in `appsettings.json` onder `SoapBackend:WsdlUrl` en `SoapBackend:BaseUrl`.

#### SOAP-client genereren

1. Gebruik een tool (bijvoorbeeld `dotnet-svcutil` of Visual Studio "Add Connected Service") om uit de WSDL strongly typed C#-klassen te genereren.
2. Plaats de gegenereerde code in `Infrastructure/Soap/Generated` of vergelijkbare map.

#### SoapWorkOrderClient invullen

In `Infrastructure/Soap/SoapWorkOrderClient.cs`:

1. Injecteer configuratie (`IConfiguration` of een `SoapBackendSettings` class).
2. Maak een methode:

```csharp
public async Task<IReadOnlyList<SoapWorkOrderDto>> GetWorkOrdersAsync()
```

waarin je:

- De gegenereerde SOAP-client aanmaakt met de endpoint uit config.
- De SOAP-operatie `GetWorkOrders` aanroept.
- Het response object omzet naar een eigen `SoapWorkOrderDto` (eenvoudige klas in `Core/Dtos`).

#### Service integreren met SOAP-client

In `IWorkOrderService` voeg je toe:

```csharp
Task<IReadOnlyList<SoapWorkOrderDto>> FetchWorkOrdersFromSoapAsync();
```

Implementatie (`WorkOrderService`) gebruikt `SoapWorkOrderClient` om de data op te halen.

In eerste instantie: alleen ophalen en teruggeven, nog niet opslaan.

#### Test-endpoint voor SOAP-import

Voeg in `WorkOrderController` een endpoint toe:

- `GET /api/workorders/soap-test`
- Roept `FetchWorkOrdersFromSoapAsync` aan en geeft resultaat terug, zodat je kunt zien of SOAP werkt.

### Wat leer je hier?

- Hoe je een .NET-project koppelt aan een externe SOAP-service.
- Hoe je werkt met door WSDL gegenereerde types.
- Dat integratie begint bij "kunnen praten" met de andere kant, nog voordat je data opslaat.

## Fase 4 – XML-afhandeling en XSD-validatie

**Doel:** ontvangen SOAP-data controleren op structuur en inhoud via XML/XSD.

### Concrete taken

#### XSD-schema verkrijgen of maken

1. Zorg voor een XSD die de structuur van de werkorderberichten beschrijft (veldtypen, verplichte elementen).
2. Plaats het bestand in bijvoorbeeld `Infrastructure/XmlSchemas/workorders.xsd`.

#### Validatielaag toevoegen

1. Maak een class `XmlWorkOrderValidator` in `Infrastructure/Xml`.
2. Voeg een methode toe:

```csharp
public ValidationResult Validate(string xml);
```

`ValidationResult` kan een eigen type zijn met bijv.:

- `bool IsValid`
- `List<string> Errors`

3. Gebruik `XmlReaderSettings` + `XmlSchemaSet` om tegen de XSD te valideren.

#### XML bemachtigen

Afhankelijk van tooling:

- Als de SOAP-client alleen objecten teruggeeft, kun je eventueel XML-logica in de Spring Boot backend configureren.
- Of je serialiseert de ontvangen objecten weer terug naar XML (voor validatie-oefening).

Voor dit leerproject is serialiseren van de response-objecten naar XML acceptabel: het gaat om de techniek.

#### Validatie integreren in importflow

In `WorkOrderService.ImportWorkOrdersFromSoapAsync`:

Voor elke binnengekomen werkorder:

1. Zet deze om naar XML (of gebruik de ruwe XML als je die hebt).
2. Roep de validator aan.
3. Als `IsValid == false`: markeer deze werkorder als "invalid" (in deze fase: alleen in geheugen, opslaan komt in Fase 5).

#### Fouten inzichtelijk maken

Voeg logging toe bij validatiefouten:

- Log `ExternalWorkOrderId` + eerste regel van de fout.

### Wat leer je hier?

- Hoe je XML-berichten valideert tegen een schema.
- Hoe je structurele fouten vroeg in de keten opspoort.
- Waarom schema's belangrijk zijn bij integraties (consistentie, contracten).

## Fase 5 – Importlogica (Imports & ImportErrors)

**Doel:** elke import-run en elke fout correct administreren in de database.

### Concrete taken

#### Entities fine-tunen

- Controleer `ImportRun`:
  - Eigenschappen als `Id`, `StartedAtUtc`, `CompletedAtUtc`, `Status`, `SourceSystem`.
- Controleer `ImportError`:
  - `ImportRunId`, `ExternalWorkOrderId`, `ErrorType`, `ErrorMessage`.

#### Importflow ontwerpen

In `WorkOrderService.ImportWorkOrdersFromSoapAsync`:

1. Maak eerst een nieuwe `ImportRun` met:
   - `Status = "RUNNING"`
   - `StartedAtUtc = DateTime.UtcNow`
   - `SourceSystem = "DummyLovionSoapBackend"` (uit config)
2. Sla deze op (zodat je `ImportRun.Id` hebt).

#### Werkorders verwerken per stuk

Voor elke werkorder uit SOAP:

1. Valideer XML (Fase 4).
2. Als validatie faalt:
   - Maak een `ImportError`:
     - `ImportRunId = importRun.Id`
     - `ExternalWorkOrderId = <id uit bericht>`
     - `ErrorType = "VALIDATION"`
     - `ErrorMessage = string.Join("; ", validationErrors)`
   - Sla deze op.
3. Als validatie slaagt:
   - Bereid data voor om later op te slaan als `WorkOrder` (database-opslag kun je in deze fase al doen, of verschuiven naar Fase 6).

#### ImportRun afronden

Na de loop:

- Als er geen fouten zijn → `Status = "SUCCESS"`.
- Als er gemengde resultaten zijn → `Status = "PARTIAL_SUCCESS"`.
- Als alles fout is → `Status = "FAILED"`.
- Vul `CompletedAtUtc = DateTime.UtcNow`.
- Sla de wijziging op.

#### API voor importgeschiedenis (optioneel nu, handig later)

In `ImportController` of bijvoorbeeld `SystemController`:

- `GET /api/imports` → lijst van import-runs.
- `GET /api/imports/{id}/errors` → alle fouten bij één run.

### Wat leer je hier?

- Dat integratie meer is dan "even data inlezen"; je bouwt een herleidbare geschiedenis.
- Hoe je fouten structureel vastlegt i.p.v. alleen in logs.
- Hoe je runs kunt vergelijken (bijv. hoeveel werkorders per dag succesvol waren).

## Fase 6 – Workflowstatus en businessregels

**Doel:** van ruwe data naar processtatussen en eenvoudige businesslogica.

### Concrete taken

#### Statusveld uitbreiden

In `WorkOrder` een `Status`-property gebruiken zoals:

- `IMPORTED`
- `VALIDATION_FAILED`
- `READY`
- `PROCESSED`

#### Status setten tijdens import

In `ImportWorkOrdersFromSoapAsync`:

- Bij succesvolle validatie:
  - Nieuwe `WorkOrder`-entity aanmaken (als nog niet bestaat) met `Status = "IMPORTED"`.
- Bij validatiefout:
  - Of helemaal geen werkorder aanmaken.
  - Of een werkorder record maken met `Status = "VALIDATION_FAILED"` en bijvoorbeeld extra foutinfo.

#### Status-update acties

1. Maak in `IWorkOrderService` een methode:

```csharp
Task MarkAsProcessedAsync(int workOrderId);
```

Implementatie:

- Haal `WorkOrder` op.
- Zet `Status = "PROCESSED"`.
- Sla op.

2. Voeg in `WorkOrderController` een endpoint toe:

- `POST /api/workorders/{id}/process` dat deze methode aanroept.

#### Filteren op status in REST-API

`GET /api/workorders?status=IMPORTED`:

1. Querystring lezen in controller.
2. Filter toepassen in service:

```csharp
if (!string.IsNullOrEmpty(status))
    query = query.Where(w => w.Status == status);
```

### Wat leer je hier?

- Denken in status-overgangen in plaats van alleen data-opslag.
- Hoe businessregels zich vertalen naar code (bijv. "alleen PROCESSED als data oké is").
- Hoe je API's maakt die aansluiten bij procesvragen ("Geef me alle open werkorders").

## Fase 7 – Logging en foutafhandeling uitbreiden

**Doel:** de integratie inzichtelijk en debugbaar maken.

### Concrete taken

#### Logging in services

1. Injecteer `ILogger<WorkOrderService>` en `ILogger<SoapWorkOrderClient>` etc.
2. Log o.a.:

**INFO** bij start/einde van import:

- `"Starting import run {ImportRunId}"`
- `"Finished import run {ImportRunId} with status {Status}"`

**WARN** bij dubieuze situaties:

- Dubbele `ExternalWorkOrderId`
- Onbekend `AssetRef`

**ERROR** bij exceptions:

- Log exception + context (bijv. `ExternalWorkOrderId`, `ImportRunId`).

#### Logging in controllers

Eventueel simpele logs voor binnenkomende requests:

- `"GET /api/workorders called with status={status}"`

#### Global exception handling

1. Voeg een middleware of `UseExceptionHandler` toe in `Program.cs`.
2. Zorg dat ongehandelde exceptions:
   - Gelogd worden op ERROR-niveau.
   - Een nette HTTP 500-response geven (geen stacktrace naar buiten).

#### Configuratie van logniveau

In `appsettings.Development.json`:

- Stel bijv. `Logging:LogLevel:Default` op `Information` of `Debug`.
- In productie zou je dit lager zetten (meer op `Warning/Error`).

### Wat leer je hier?

- Loggen met context (ID's, status, type fout).
- Dat goede logs later meer waarde hebben dan "iets in de console schrijven".
- Hoe je exceptions centraal afhandelt in een web-API.

## Fase 8 – (Optioneel) Kleine UI of monitoring

**Doel:** integratie zichtbaar maken voor een gebruiker/beheerder.

### Concrete taken

#### Kiezen van UI-technologie

Bijv.:

- React + TypeScript (apart project)
- Razor Pages in dezelfde API
- Of een simpele HTML+JavaScript pagina

#### Schermen bedenken

**Dashboard:**

- Totaal aantal werkorders.
- Aantal per status.
- Datum/tijd van laatste succesvolle import.

**Detail:**

- Lijst import-runs met status.
- Lijst fouten per run.

#### API-koppeling

De UI haalt data op via:

- `GET /api/workorders?status=...`
- `GET /api/imports`
- `GET /api/imports/{id}/errors`

#### Visualisatie

- Tabellen of kleine grafieken (optioneel).
- Knop "Refresh" om importstatus te herladen.

### Wat leer je hier?

- Hoe je backendgegevens vertaalt naar iets dat voor mensen begrijpelijk is.
- Hoe REST-API's echt gebruikt worden door UIs.
- Hoe je integratie- en monitoringbehoeften herkent bij eindgebruikers.

## Checklists per fase (praktisch)

### Algemene basis:

```bash
dotnet restore
dotnet build
dotnet run -p LovionIntegrationClient.Api
```

### Fase 1:

- Connectionstring ingevuld.
- DbContext geregistreerd.
- InitialCreate migratie gedraaid.
- Databasebestand aanwezig.

### Fase 2:

- DTO's aangemaakt.
- Services vullen data vanuit DbContext.
- Controllers roepen alleen services aan.
- `GET /api/assets` en `GET /api/workorders` werken.

### Fase 3:

- WSDL-URL in config.
- SOAP-client gegenereerd.
- `SoapWorkOrderClient.GetWorkOrdersAsync` werkt.
- Testendpoint `/api/workorders/soap-test` geeft data.

### Fase 4:

- XSD-bestand aanwezig.
- `XmlWorkOrderValidator` aangelegd.
- Validatiefouten worden gedetecteerd.

### Fase 5:

- `ImportRun` en `ImportError` tabellen gevuld bij import.
- Status en timestamps van imports kloppen.

### Fase 6:

- `WorkOrder.Status` wordt gezet tijdens import.
- Filter op status in `GET /api/workorders`.

### Fase 7:

- Logging in services en SOAP-client.
- Global exception handling aanwezig.
- Logs duidelijk en bruikbaar.

### Fase 8 (optioneel):

- UI haalt data op uit je API.
- Dashboard laat kerninformatie zien.

## Tips voor beginners zonder .NET-ervaring

- Begin elke sessie met:
  - `dotnet restore` (alle dependencies binnenhalen)
  - `dotnet build` (check op compile-fouten)
- Zoek in `Program.cs` naar `AddScoped` / `AddDbContext` om te zien welke services beschikbaar zijn.
- Gebruik `/swagger` om snel te testen of endpoints nog werken na wijzigingen.
- Commit na elke fase in Git met een zinvolle boodschap:
  - `"Fase 2: basis REST API werkend"`
  - `"Fase 3: SOAP-client skeleton toegevoegd"`
- Als iets "magisch" voelt (zoals DI), probeer het even hardop uit te leggen:
  - "Wanneer ik `IWorkOrderService` in de constructor zet, maakt .NET automatisch een `WorkOrderService` omdat dat in `Program.cs` is geregistreerd."
