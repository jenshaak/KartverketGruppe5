# Kartverket Gruppe 5

## Om Prosjektet
Dette er en webapplikasjon for registrering og behandling av endringer i kartet, utviklet for Kartverket. Applikasjonen lar brukere melde inn nye endringer i kartet, mens saksbehandlere kan behandle disse innmeldingene.

## Systemarkitektur

### Teknisk Stack
- **Frontend**: ASP.NET Core MVC, Tailwind CSS, JavaScript
- **Backend**: C# (.NET 7.0)
- **Database**: MariaDB
- **Containerization**: Docker
- **Autentisering**: ASP.NET Core Identity

### Arkitekturdiagram
```
[Web Browser] → [ASP.NET Core MVC App] → [MariaDB]
                        ↓
                [Docker Container]
```

### Hovedkomponenter
1. **Views** (MVC Views)
   - Brukergrensesnitt for innmelding
   - Administrasjonspanel
   - Saksbehandlervisning

2. **Services**
   - BrukerService
   - InnmeldingService
   - SaksbehandlerService
   - KommuneService

3. **Database-behandling**
   - MariaDB database
   - Dapper ORM

## Oppsett og Installasjon

### Forutsetninger
- Docker Desktop
- .NET 7.0 SDK
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

5. Her er innlogging for bruker og saksbehandlere:
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

### Testscenarioer

#### 1. Brukerregistrering og Innlogging
- Registrere ny bruker
- Logge inn med eksisterende bruker
- Feilhåndtering ved ugyldig innlogging

#### 2. Innmelding av endringer i kartet
- Opprette ny innmelding
- Redigere eksisterende innmelding
- Slette innmelding

#### 3. Saksbehandling
- Vise liste over innmeldinger
- Endre status på innmelding
- Legge til kommentar

### Testresultater
[Tenker å legge testresultater her når vi har gjennomført dem]

## Sikkerhet

### Autentisering
- Bruker sessions for innlogging
- Passordhashing med SHA256
- CSRF-beskyttelse

### Autorisering
- Rollebasert tilgangskontroll for saksbehandlere (admin eller bare saksbehandler)
- Admin-rettigheter
- Saksbehandler-rettigheter
- Bruker-rettigheter

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