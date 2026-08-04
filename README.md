# Asset Manager

WPF-desktopapplikation til styring af IT-udstyrs livscyklus, udviklet som
2. semesters eksamensprojekt på datamatikeruddannelsen, UCL Erhvervsakademi
og Professionshøjskole.

Systemet er udviklet for en dansk softwarevirksomhed, som havde behov for et
samlet overblik over virksomhedens devices. Eksisterende værktøjer dækkede
sikkerhed og enhedsstyring, men gav ikke overblik over ejerskab, indkøbsdata
eller devicets vej gennem organisationen.

Projektet blev gennemført over tre sprints med løbende sprint reviews med
virksomhedens Product Owner.

## Funktionalitet i version 1.0

**Anmodningsflow**
- Medarbejdere opretter anmodninger om nyt udstyr med begrundelse og ønsket
  leveringsdato
- Asset Manager behandler anmodninger fra et dashboard, hvor de vises
  kronologisk
- Godkendelse eller afvisning registreres som en beslutning med kommentar og
  dato, så processen kan spores bagud

**Device-livscyklus**
- Registrering af devices med type, operativsystem, lokation og købsdato
- Forventet udskiftningsdato beregnes automatisk tre år efter registrering
- Statusforløb: `PLANNED` → `ORDERED` → `RECEIVED` → `INUSE` → `INSTOCK`,
  samt `REGISTERED` og `CANCELLED`
- Tildeling af devices direkte til en medarbejder, uden forudgående anmodning
- Returnering til lager med markering af, om devicet er wiped

**Medarbejdere**
- Oprettelse med tilknytning til afdeling og rolle, inkl. validering af
  emailformat og kontrol for dubletter
- Registrering af fratrædelse, hvorefter dashboardet viser de devices, der
  skal wipes

**Overblik**
- Søgning, sortering og filtrering i både medarbejder- og deviceoversigten
- Dashboard med statuskort og genveje til de hyppigste handlinger
- Rollevalg ved opstart (medarbejder eller Asset Manager) styrer, hvilke
  funktioner og data der er tilgængelige

## Teknologi

| | |
|---|---|
| Sprog og platform | C#, .NET 8 |
| Brugerflade | WPF (XAML), MVVM |
| Database | Microsoft SQL Server, tilgået med ADO.NET og stored procedures |
| Test | MSTest, Moq |
| Proces og værktøjer | Scrum, Git, GitHub Projects, pull requests med peer review |

## Arkitektur

Løsningen er opdelt i tre lag, hvor afhængighederne kun peger indad mod
forretningslogikken:
- **Application** – domænemodeller, services med forretningslogik, display
  models og interfaces. Kender hverken til brugerflade eller database.
- **Data** – repositories, der implementerer dataadgang via stored procedures.
- **Presentation.Wpf** – views i XAML og tilhørende viewmodels.
- **AssetManagerTest** – test af repository-laget.

Applikations- og datalaget er bygget som class libraries, hvilket gjorde det
muligt at tilføje nye use cases sprint for sprint uden at ændre den
overordnede struktur.

**Anvendte mønstre**

- **MVVM** – views binder til viewmodels via databinding og `RelayCommand`.
  Præsentationslogik og validering ligger i viewmodels, forretningslogik i
  services.
- **Repository pattern** – al databaseadgang indkapsles bag et generisk
  `IRepository<T>`, så services ikke kender den underliggende database.
- **Service layer** – services koordinerer på tværs af flere repositories.
  `RequestService` fungerer fx som proceskoordinator, når en godkendelse
  samtidig skal opdatere loan, request, device og oprette en beslutning.
- **Observer pattern** – viewmodels implementerer `INotifyPropertyChanged`,
  og `ObservableCollection<T>` bruges til lister, så brugerfladen opdateres
  automatisk. Overlay-paneler kommunikerer med deres host via events frem
  for direkte referencer.
- **Singleton** – `DatabaseConnection` sikrer, at connection string
  initialiseres ét sted og deles af alle repositories.
- **Constructor injection** – konkrete implementeringer tilføres via
  constructoren, hvilket holder lagene løst koblet og gør dem testbare.

## Database

Databasen er normaliseret til tredje normalform. `Loan` er den centrale
tabel, som binder medarbejder, device, anmodning og godkender sammen.
En `Decision` refererer til et `Loan`, da en beslutning altid træffes på
baggrund af et eksisterende lån, mens et lån kan eksistere uden beslutning.

Al dataadgang sker gennem stored procedures med eksplicit parameterbinding
som beskyttelse mod SQL-injection. Statusværdier gemmes som heltal og er
defineret som enums i applikationslaget, da værdierne er faste.

Enkelte forretningsregler håndhæves i databasen frem for i koden – fx sletter
`uspDeleteRequest` kun en anmodning, hvis der ikke findes et tilknyttet lån.

## Kom i gang

**Forudsætninger**
- Windows (WPF)
- .NET 8 SDK
- Microsoft SQL Server

**Opsætning**

1. Klon repoet
2. Kør scripts i `database/` i rækkefølge:
   - `01_schema_and_seed.sql` – database, tabeller, constraints og
     eksempeldata
   - `02_stored_procedures.sql` – stored procedures
3. Opret filen `Presentation.Wpf/appsettings.json` med din connection
   string. Filen er bevidst udeladt fra versionsstyring:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AssetManagement;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

4. Byg og kør `Presentation.Wpf`

Eksempeldata er fiktivt og indeholder fire afdelinger, ti medarbejdere, tyve
devicebeskrivelser samt et udsnit af anmodninger, lån og beslutninger, så
systemet kan afprøves uden først at oprette data.

## Test

Repository-laget testes med MSTest mod en dedikeret testdatabase, der er en
kopi af produktionsskemaet. Testene dækker CRUD-operationer samt
fejlsituationer – fx at et opslag på et ukendt id returnerer null, og at
ugyldige input kaster de forventede exceptions.

Testene kører mod en rigtig database frem for mocks, fordi de skal verificere
samspillet mellem repository-kode, stored procedures og databaseskema. Det er
her, fejl i datahåndteringen hurtigst får konsekvenser for resten af systemet.

Opret testdatabasen ved at køre `database/99_test_database.sql`, som
indeholder skema, eksempeldata og stored procedures i én fil. Kør derefter:

```bash
dotnet test
```

## Afgrænsning

Version 1.0 fokuserer på kernefunktionaliteten: registrering, anmodninger,
beslutninger og livscyklusstyring. Følgende blev bevidst fravalgt og
prioriteret til en senere version:

- Login med brugernavn og adgangskode. I version 1.0 vælges rolle ved
  opstart, hvilket styrer adgangen til funktioner, men ikke er reel
  adgangskontrol.
- Audit-logning af oprettelser, ændringer og sletninger
- Understøttelse af flere devicetyper end laptops og mobiler
- Integration til Microsoft Intune og HR-systemer
- Rapporter og budgetprognoser
