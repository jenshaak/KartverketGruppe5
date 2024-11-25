# Kartverket Gruppe 5

### NB!
- Punkt 5 i "Oppsett og Installasjon" er essensielt for at alt skal fungere.

## Om Prosjektet

### Oversikt
Dette er en webapplikasjon utviklet for Kartverket som muliggjør effektiv registrering og behandling av kartendringer. Systemet tilbyr en brukervennlig plattform hvor publikum kan melde inn endringer i kartet, mens Kartverkets saksbehandlere kan behandle disse innmeldingene på en strukturert måte.

### Hovedfunksjoner
- **For brukere:**
  - Registrering av nye kartendringer med nøyaktig lokasjon
  - Opplasting av dokumenterende bilder
  - Sporing av egne innmeldinger
  - Mottak av statusoppdateringer på innmeldte saker

- **For saksbehandlere:**
  - Oversikt over innmeldte kartendringer
  - Effektiv saksbehandling med statusoppdateringer
  - Videresending av saker til andre saksbehandlere
  - Kommunikasjon med innmelder

- **For administratorer:**
  - Saksbehandleradministrasjon
  - Oppdatering av kommune- og fylkesdata

### Tekniske Høydepunkter
- **Sikkerhet:** Autentisering og autorisering med rollebasert tilgangskontroll
- **Skalerbarhet:** Containerbasert arkitektur med Docker
- **Vedlikeholdbarhet:** Clean Architecture prinsipper med "separation of concerns"
- **Kvalitet:** Omfattende testing med 81 unit tester
- **Brukervennlighet:** Responsivt design med Tailwind CSS og vanlig CSS

### Målgruppe
- **Brukere:** Brukere som ønsker å melde inn kartendringer
- **Saksbehandlere:** Kartverkets saksbehandlere
- **Administratorer:** Kartverkets systemansvarlige

### Prosjektmål
1. Forenkle prosessen for innmelding av kartendringer
2. Effektivisere saksbehandlingen
3. Sikre kvalitet i kartdata
4. Forbedre kommunikasjon mellom mannen i gata og Kartverket


## Oppsett og Installasjon

### Forutsetninger
- Docker Desktop
- .NET 8.0 SDK
- Git

### Installasjon
1. Klon repositoriet:
```bash
git clone https://github.com/jenshaak/KartverketGruppe5.git
```

2. Naviger til prosjektmappen:
```bash
cd KartverketGruppe5
```

3. Start Docker containers:
```bash
docker-compose up -d
```

4. Applikasjonen er nå tilgjengelig på `http://localhost:8080`

5. (VIKTIG) Populer databasen med kommuner og fylker:
   - Logg inn som saksbehandler med admin-tilgang (se punkt 6)
   - Gå til Administrasjon og klikk på "Oppdater Fylker og Kommuner"

6. Her er innlogging for bruker og saksbehandlere:
   - Saksbehandler med admin-tilgang: `rune@kartverket.no` med passord `123`
   - Saksbehandler: `lars@kartverket.no` med passord `123`
   - Bruker: `ole@gmail.com` med passord `123`


## Systemarkitektur

### Teknisk Stack
- **Frontend**: ASP.NET Core MVC, Tailwind CSS, JavaScript
- **Backend**: C# (.NET 8.0)
- **Database**: MariaDB
- **Containerization**: Docker
- **Autentisering**: Session-basert autentisering
- **ORM**: Dapper

### Arkitekturdiagram
```
[Web Browser] → [ASP.NET Core MVC App] → [Services] → [Repositories] → [MariaDB]
                         ↓
                 [Docker Container]
```

### Hovedkomponenter
1. **Views** (MVC Views)
   - Home
   - Login
   - MineInnmeldinger
   - MapChange
   - MinProfil
   - MineSaker
   - Saksbehandling
   - Admin

2. **Controllers**
   - LoginController
   - HomeController
   - MineInnmeldingerController
   - MapChangeController
   - MinProfilController
   - MineSakerController
   - SaksbehandlingController
   - AdminController

3. **Services & Interfaces**
   - IBrukerService / BrukerService
   - IInnmeldingService / InnmeldingService
   - ISaksbehandlerService / SaksbehandlerService
   - IKommuneService / KommuneService
   - INotificationService / NotificationService
   - IKommunePopulateService / KommunePopulateService
   - IBildeService / BildeService
   - ILokasjonService / LokasjonService
   - IFylkeService / FylkeService
   - IPasswordService / PasswordService

4. **Repositories & Interfaces**
   - IBrukerRepository / BrukerRepository
   - IInnmeldingRepository / InnmeldingRepository
   - ISaksbehandlerRepository / SaksbehandlerRepository
   - IKommuneRepository / KommuneRepository
   - IKommunePopulateRepository / KommunePopulateRepository
   - IBildeRepository / BildeRepository
   - ILokasjonRepository / LokasjonRepository
   - IFylkeRepository / FylkeRepository

5. **Database-behandling**
   - MariaDB database
   - Dapper ORM med prepared statements
   - Repository pattern for datahåndtering
   - Migrations for databaseoppsett

## Database

### ERD (Entity Relationship Diagram)
- ER-diagrammet til prosjektet finnes her: https://drive.google.com/file/d/1RTW6p7qJRdnA4xg8uCIE0aoXyQ7_iA_k/view?usp=sharing

### Tabeller
- Bruker
- Saksbehandler
- Innmelding
- Kommune
- Fylke
- Lokasjon

## Testing
Prosjektet har omfattende unit testing av alle controllers og services:

- **Antall tester:** 81
- **Test resultat:** Alle tester bestått ✅
- **Test dekning:** Controllers og Services

### Kjøre testene selv
```bash
cd KartverketGruppe5.Tests
dotnet test
```

### Se testresultater
Detaljerte testresultater finnes i `/KartverketGruppe5.Tests/TestResults/TestResults.trx`

## Sikkerhet

### Autentisering og Autorisering
- Custom session-basert autentisering
- CSRF-beskyttelse med AntiForgeryToken
- XSS-beskyttelse
- Rollebasert tilgangskontroll
- Input validering og sanitering

### Feilhåndtering og Logging
- Strukturert logging med ILogger
- Exception handling på controller-, repository- og service-nivå
- Transaksjonshåndtering for databaseoperasjoner

### Arkitekturmønster
- Repository pattern for dataisolasjon
- Service pattern for forretningslogikk
- Dependency Injection for løs kobling
- Interface-basert programmering

## Drift og Vedlikehold

### Logging
- Applikasjonslogger i Docker container
- Feilhåndtering og logging
- Database-sporing

### Backup
- Database backup prosedyrer
- Gjenopprettingsrutiner

### Overvåking
- Docker container status
- Database tilkobling
- Applikasjonsytelse