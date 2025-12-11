# SOAP Client Genereren met dotnet-svcutil

## Installatie

Het `dotnet-svcutil` tool is al geïnstalleerd als lokaal tool voor dit project. Je hoeft niets meer te installeren.

## .NET SDK

Dit project gebruikt .NET 8. Zorg dat je de .NET 8 SDK (bijv. 8.0.300) geïnstalleerd hebt:

- Download van: https://dotnet.microsoft.com/download/dotnet/8.0
- Controleer met:
  ```bash
  dotnet --version
  ```
  Dit zou iets als `8.0.300` moeten tonen.

## Gebruik

### Stap 1: WSDL URL bepalen

Zorg dat je de WSDL URL hebt. Deze moet in `appsettings.json` staan onder `SoapBackend:WsdlUrl`.

### Stap 2: SOAP client genereren

Navigeer naar de Infrastructure project directory en voer uit:

**Optie 1 (aanbevolen):**
```bash
cd LovionIntegrationClient.Infrastructure
dotnet dotnet-svcutil <WSDL_URL> --outputDir Soap/Generated
```

**Optie 2:**
```bash
cd LovionIntegrationClient.Infrastructure
dotnet tool run dotnet-svcutil <WSDL_URL> --outputDir Soap/Generated
```

**Voorbeeld:**
```bash
cd LovionIntegrationClient.Infrastructure
dotnet dotnet-svcutil http://localhost:8080/ws/workorders.wsdl --outputDir Soap/Generated
```

**Belangrijk:** Gebruik **NIET** `dotnet tool run dotnet-svcutil -- --version` - dat is onjuist!

### Stap 3: Vereiste NuGet packages toevoegen

Na het genereren moet je de volgende packages toevoegen aan `LovionIntegrationClient.Infrastructure.csproj` (versies worden automatisch gekozen):

```bash
cd LovionIntegrationClient.Infrastructure
dotnet add package System.ServiceModel.Primitives
dotnet add package System.ServiceModel.Http
```

### Stap 4: Gebruik de gegenereerde client

De gegenereerde code staat in `Infrastructure/Soap/Generated/`. Je kunt deze gebruiken in `SoapWorkOrderClient.cs`.

## Belangrijke opmerkingen

- **Gebruik `dotnet tool run dotnet-svcutil`** (niet `dotnet tool run dotnet-svcutil -- --version`)
- De `--outputDir` parameter bepaalt waar de gegenereerde code komt
- De gegenereerde code wordt automatisch toegevoegd aan je project
- Controleer altijd de gegenereerde code voordat je deze gebruikt

## Troubleshooting

Als je foutmeldingen krijgt:
1. Controleer of de WSDL URL bereikbaar is
2. Zorg dat je in de juiste directory bent (Infrastructure project)
3. Controleer of alle vereiste packages geïnstalleerd zijn

