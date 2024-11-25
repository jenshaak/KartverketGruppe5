# Kartverket Gruppe 5

## Om Prosjektet
Dette er en webapplikasjon for registrering og behandling av endringer i kartet, utviklet for Kartverket. Applikasjonen lar brukere melde inn nye endringer i kartet, mens saksbehandlere kan behandle disse innmeldingene.

## Systemarkitektur

### Teknisk Stack
- **Frontend**: ASP.NET Core MVC, Tailwind CSS, JavaScript
- **Backend**: C# (.NET 8.0)
- **Database**: MariaDB
- **Containerization**: Docker
- **Autentisering**: Custom Session-basert autentisering
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