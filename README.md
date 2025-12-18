# DigitalniProdukty

ASP.NET Core MVC aplikace pro správu digitálních produktů a licencí (Identity + EF Core, HTMX, Tailwind).

## Účel projektu

Cílem projektu je poskytnout jednoduché, bezpečné a více-tenantní prostředí pro práci s licencemi digitálních produktů:

- **Administrace licencí** (generování, přehled, detaily, práce se stavem).
- **Oddělení distributorů/resellerů** tak, aby každý obchodní partner viděl a spravoval pouze “své” licenční klíče.
- **Auditovatelné zásahy administrátora**, zejména přesuny licencí mezi skupinami (tenants).

Projekt je postaven tak, aby šel postupně rozšiřovat (produkty, objednávky, fakturace, integrace), ale už teď pokrývá kompletní workflow kolem licencí.

## Jak to funguje (high-level)

- Aplikace používá **ASP.NET Identity** pro přihlášení, role a autorizaci.
- Data se ukládají přes **EF Core** (SQL Server / LocalDB).
- Licencování je navrženo jako více-tenantní: klíče jsou přiřazené do **skupin (KeyGroups)**.
  - Skupina reprezentuje obchodního vlastníka klíčů (admin pool, distributor, apod.).
  - Uživatel má členství v právě jedné skupině (membership), což se používá pro scoping dat.
- Admin může (v rámci licencování) přepínat přehled přes skupiny a umí klíče **přesouvat** mezi skupinami; přesun se zapisuje do auditní tabulky.

## Hlavní funkce

### Role a přístup

Typicky se pracuje s rolemi:

- **Admin**: vidí vše (případně filtruje dle skupiny), může přesouvat klíče mezi skupinami.
- **Distributor**: může generovat klíče a spravovat licencování pouze ve své skupině.
- **Reseller / End-user**: podle nastavených politik (v projektu jsou připravené role a policy).

Přístup k licencování je chráněn přes autorizace (policy), aby se nedalo “dostat” na cizí data jen úpravou URL.

### Licencování (UI)

V levém menu jsou licencování rozdělené do samostatných částí:

- **Přehled licencí**: poslední klíče, rychlý přehled stavu, detail licence.
- **Generování licencí**: formulář pro vygenerování klíčů. Stránka po vygenerování zobrazuje pouze naposledy vytvořenou várku.
- **Instalace**: přehled posledních instalačních událostí (historií).

UI používá HTMX pro částečné aktualizace bez nutnosti plného reloadu stránky.

### Skupiny (multi-tenant)

- Každý licenční klíč je přiřazen do konkrétní skupiny.
- Ne-admin uživatelé jsou automaticky “scoped” na svou skupinu.
- Admin může vidět všechny skupiny a cíleně filtrovat.
- Přesun klíče mezi skupinami je auditovaný.

## Možná budoucí rozšíření

Nápady na smysluplná rozšíření, která na současnou architekturu přirozeně navazují:

- **Produkty a plány**: navázat klíče na konkrétní produkt/edici, délku podpory, feature flags.
- **Objednávky a fakturace**: import/export objednávek, napojení na fakturační systém, DPH, párování plateb.
- **Self-service portál pro end-user**: správa zařízení, odpojení zařízení, zobrazení aktivací, změny profilu.
- **Integrace**: webhooky (vygenerováno/přiřazeno/revokováno), API pro e‑shop, SSO (Entra ID / OAuth2).
- **Reporting**: dashboardy pro admin/distributory (aktivace v čase, nejčastější zařízení, top produkty).
- **Jemnější multi-tenant model**: podskupiny (reseller pod distributorem), více členství na uživatele (pokud bude potřeba).
- **Bezpečnost a provoz**: rotace klíčů, detailnější audit log, hardening emailových flows, rate-limiting per IP/user, alerting.
- **DevOps**: docker-compose (SQL + app), CI pro build/test, automatické migrace na staging/prod.

## Architektura / komponenty

Projekt je klasická ASP.NET Core MVC aplikace s oddělením zodpovědností do vrstev:

- **Controllers/**: MVC controllery a endpointy (včetně HTMX partialů) pro auth, účet a licencování.
- **Views/**: Razor stránky + partial views, kde se skládá UI a HTMX interakce.
- **Data/**: `ApplicationDbContext` (EF Core) + migrace v `Data/Migrations/`.
- **Models/**: view-modely a doménové modely (licence, skupiny, audit, instalace, auth input modely).
- **Services/**: aplikační logika (generování licencí, scoping/skupiny, 2FA, email, tokeny, QR).
- **Resources/**: lokalizace `.resx` (CZ/EN) pro UI texty a validační hlášky.
- **wwwroot/**: statické assety, Tailwind output, vendor knihovny.

Klíčové principy:

- **Multi-tenant scoping** je řešen přes KeyGroup membership a `GroupId` na licenčních klíčích.
- **Autorizace** je vynucena policy/rolemi v `Program.cs` a kontrolami v controllerech.
- **Audit** se používá pro administrátorské zásahy (např. přesun licence mezi skupinami).

## Roadmapa

Praktická roadmapa po menších krocích (orientačně):

- **MVP hardening**
  - doplnit `.env` / user-secrets pro lokální secrets,
  - zkontrolovat logování citlivých údajů,
  - sjednotit copy textů a validace na licencování.
- **v1 (produktově použitelná verze)**
  - produkty/plány + navázání licencí na produkt,
  - export/import (CSV) pro distributory,
  - přehledy/reporting (aktivace v čase, top zařízení).
- **v2 (integrace a automatizace)**
  - veřejné API + tokeny, webhooky,
  - napojení na e‑shop/fakturaci,
  - pokročilé multi-tenant scénáře (hierarchie distributor → reseller).

## Větve v GitHub repozitáři

- `develop`: hlavní vývojová větev (aktuální kód)
- `main`: záměrně prázdná (jen inicializační commit bez souborů)

Doporučení: na GitHubu si nastav jako **Default branch** větev `develop`.

## Požadavky

- .NET SDK (projekt cílí na `net10.0`)
- SQL Server / LocalDB (výchozí connection string je pro LocalDB)
- (Volitelné) Node.js + npm pro Tailwind build

## Spuštění (Development)

1) Obnov závislosti a spusť aplikaci:

```bash
dotnet restore
dotnet run
```

V Development režimu se databáze automaticky migruje při startu (`db.Database.Migrate()`).

2) Frontend (volitelné)

Projekt používá Tailwind CLI a HTMX.

```bash
npm install
npm run build:css
# nebo během vývoje
npm run watch:css
```

## Konfigurace (appsettings)

- `appsettings.json` je commitovaný a neobsahuje citlivé údaje.
- `appsettings.Development.json` je **ignorovaný v gitu** (viz `.gitignore`) a je určen pro lokální nastavení.

### Bootstrap admin

Aplikace umí v Development vytvořit admin účet podle konfigurace `BootstrapAdmin`.

Příklad lokální konfigurace (do `appsettings.Development.json`):

```json
{
  "BootstrapAdmin": {
    "Enabled": true,
    "Email": "admin@local.test",
    "Password": "ChangeMe!12345",
    "RequireChangeOnFirstLogin": true
  }
}
```

## DB / migrace

Pokud budeš chtít migrovat ručně:

```bash
dotnet ef database update
```

(Pokud nemáš nainstalovaný EF CLI: `dotnet tool install --global dotnet-ef`)
