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
cd KartverketGruppe5/KartverketGruppe5
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

## Test Scenarioer

### Kontroll av scenario
Kontroll av stegene kan utføres ved å kontrollere i nettlesere, eller gjennom databasen med ```Spørringer```. For å sjekke databasen, ```docker exec -it mariadb bash``` i en annen terminal mens prosjektet kjører i docker. ```mariadb -u root -pgruppe5```for å logge inn i databasen, ```USE kartverketdb;```for å gå inn i databasen.

### Admin test scenario
1. **Logg inn som administrator** ved å trykke **Meld inn endring**, **Mine innmeldinger** eller drop down menyen, og bruk en @kartverket.no (bruk rune@kartverket.no, 123).

2. **Gå til Administrasjon side** ved trykke på **Administrasjon** på hjemsiden.

3. **Oppdater Fylker og Kommuner** ved å trykke på **Oppdater Fylker og Kommuner** knappen.
*(Kontrolleres ved å trykke på knappen igjen, og sjekke om 0 fylker blir oppdatert eller ```SELECT * FROM Kommune; SELECT * FROM Fylke;```)*

4. **Gjør en saksbehandler til admin** ved å trykke **Rediger** på en saksbehandler som er merket "Nei" i admin feltet. Deretter huk av Adminstrator punktet og lagre endringene. 
*(Kontrolleres ved å sjekke at sakabehandleren er makert med grønn "Ja" i admin feltet eller ```SELECT * FROM Saksbehandler;```, og se om "Admin" verdien er satt til 1)*

5. **Slett "Sandra Bakken"** ved å gå til side 2 i tabellen, og trykke på **Slett**. 
*(Kontrolleres ved å sjekke etter Sandra på side 2 eller ```SELECT * FROM Saksbehandler WHERE fornavn="Sandra";``` hvor resultatet blir "empty set")*

6. **Registrer en ny saksbehandler** ved å trykke på **Registrer ny saksbehandler** og fylle ut feltene. 
*(Kontrolleres ved å sortere saksbehandlere etter "opprettet" eller ```SELECT * FROM Saksbehandler ORDER BY OpprettetDato DESC;``` hvor den opprettede sakbehandler vil vises)*

7. **Logg ut** ved å bruke drop-down menyen i høyre hjørne, og trykke **logg ut**


### Opprettelse av sak
1. **Registrer en bruker** ved å trykke **Meld inn endring**, **Mine innmeldinger** eller drop down menyen. Trykk **Registrer ny bruker**, fyll ut feltene og trykk **Registrer** (Husk email og passord). 
*(Kontrolleres ved å se på Min Profile i drop down menyen eller ```SELECT * FROM Bruker ORDER BY OpprettetDato DESC;``` og se etter den nye brukeren)*

2. **Meld inn en endring** ved å trykke på **Meld inn endring**, marker i kartet, skriv en beskrivelse og legg inn et bilde (Ikke nødvendig). Trykk **Send inn**. 

3. **Sjekk innmelding i Mine innmeldinger** ved å trykke på **Mine innmeldinger** og sjekke at innmeldingen er registrert. 
*(Kontrolleres ved ```SELECT * FROM Innmelding ORDER BY OpprettetDato DESC;```og se etter nyeste innmelding)*

4. **Endre innmelding** ved å trykke på innmeldingen, rediger Beskrivelsen og trykk **Endre innmelding**. 
*(Kontrolleres ved å trykke inn på innmeldingene å se etter de nye endringene, eller ```SELECT * FROM Innmelding ORDER BY OpprettetDato DESC;```og se etter nyeste innmelding).*

5. **Logg ut** ved å bruke drop-down menyen i høyre hjørne, og trykke **logg ut**

### Behandle en innmelding
1. **Logg inn som saksbehandler** ved å trykke **Meld inn endring**, **Mine innmeldinger** eller drop down menyen, og bruk en @kartverket.no (bruk ivar@kartverket.no, 123).

2. **Gå til saksbehandling** ved å trykke på **Saksbehandling**. 

3. **Ta ansvar for en sak** ved å trykke på den nyeste saken og trykk **Ta over saken**.
*(Kontrolleres ved ```SELECT * FROM Innmelding ORDER BY OpprettetDato DESC;```og se at "SaksbehandlerId" ikke er "null")*

4. **Godkjenn saken** ved å trykke på saken som er makert "under behandling", skriv en kommentar og trykk **Godkjenn**

5. **Sjekk at saken er godkjent** ved å trykkke på saken som er markert godkjent, og sjekk at kommentaren er sendt inn.
*(Kontrolleres ved ```SELECT * FROM Innmelding ORDER BY OpprettetDato DESC;```og se at "kommentar" ikke er "null" og "status" er "godkjent")*

6. **Logg ut** ved å bruke drop-down menyen i høyre hjørne, og trykke **logg ut**

### Sjekk at sak er behandlet for bruker
1. **Logg inn som bruker** ved å trykke **Meld inn endring**, **Mine innmeldinger** eller drop down menyen, og bruk samme Email og passord som tidligere.

2. **Finn innmeldingen** ved å trykke på **Mine innmeldinger** og se etter nyeste sak.

3. **Se kommentar og status** ved å trykke på nyeste innmelding, og se om status er "Godkjent", og om rett kommentar er skrevet.
*(Kontrolleres ved ```SELECT * FROM Innmelding ORDER BY OpprettetDato DESC;```og se at "kommentar" ikke er "null" og "status" er "godkjent")*

4. **Se oversikt over profil** ved å gå på **Min Profil** i drop-down menyen.

5. **Slett bruker** ved å trykk på **Slett Bruker**. (Kontrolleres ved å prøve å logge inn med samme Email og passord)
*(Kontrolleres ved ```SELECT * FROM Bruker ORDER BY OpprettetDato DESC;```og se at "Slettet" er "1")*


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
- ER-diagrammet til prosjektet finnes her: https://drive.google.com/file/d/1-eoHX_qH78j4ggozp6DFervL_HS4-HhB/view?usp=share_link

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
