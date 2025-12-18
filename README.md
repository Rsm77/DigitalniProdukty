# DigitalniProdukty

Webová aplikace v ASP.NET Core (MVC – Model/View/Controller) pro správu digitálních produktů a licencí.

Použité technologie (stručně):

- **Identity**: správa uživatelů, rolí a přihlášení.
- **EF Core (Entity Framework Core)**: databázová vrstva/ORM (mapování objektů na tabulky).
- **HTMX**: částečné aktualizace stránky bez full reloadu.
- **Tailwind CSS**: utility-first CSS pro stylování.

## Účel projektu

Cílem projektu je poskytnout jednoduché, bezpečné a multi-tenantní (oddělené pro více obchodních partnerů) prostředí pro práci s licencemi digitálních produktů:

- **Administrace licencí** (generování, přehled, detaily, práce se stavem).
- **Oddělení distributorů / prodejců (resellerů)** tak, aby každý obchodní partner viděl a spravoval pouze “své” licenční klíče.
- **Auditovatelné zásahy administrátora**, zejména přesuny licencí mezi skupinami (tenanty).

Projekt je postaven tak, aby šel postupně rozšiřovat (produkty, objednávky, fakturace, integrace), ale už teď pokrývá kompletní workflow kolem licencí.

## Jak to funguje (stručně)

- Aplikace používá **ASP.NET Identity** pro přihlášení, role a autorizaci.
- Data se ukládají přes **EF Core** do databáze (**SQL Server** / lokální **LocalDB**).
- Licencování je navrženo jako multi-tenantní: klíče jsou přiřazené do **skupin (KeyGroups – skupiny vlastníků klíčů)**.
  - Skupina reprezentuje obchodního vlastníka klíčů (např. „admin pool“ = administrátorský fond, distributor, apod.).
  - Uživatel má členství v právě jedné skupině (membership = členství), které se používá pro omezení přístupu na data (scoping).
- Admin může (v rámci licencování) přepínat přehled přes skupiny a umí klíče **přesouvat** mezi skupinami; přesun se zapisuje do auditní tabulky.

## Slovníček pojmů

- **MVC**: architektura „Model – View – Controller“ (modely dat, šablony UI, řadiče).
- **ORM / EF Core**: mapování objektů na databázové tabulky a pohodlná práce s DB.
- **HTMX**: technika, kdy se z serveru vrací jen část HTML a stránka se aktualizuje bez reloadu.
- **Tailwind CSS**: systém CSS tříd pro rychlé skládání vzhledu.
- **Policy (autorizace)**: pravidlo přístupu (např. „smí jen admin/distributor“).
- **Scoping (omezení dat)**: zajištění, že uživatel vidí jen „svá“ data (typicky dle skupiny).
- **Tenant**: jedna „organizace/skupina“ v rámci jedné aplikace (oddělená data).
- **Audit**: záznamy o důležitých akcích (kdo, kdy, co změnil).

## Hlavní funkce

### Role a přístup

Typicky se pracuje s rolemi:

- **Majitel**: *vždy jen jeden*. Jediný, kdo může vytvářet/upravovat/mazat účty **Admin** (včetně resetu hesla admina).
- **Admin**: vidí vše (případně filtruje dle skupiny), může přesouvat klíče mezi skupinami.
- **Distributor**: může generovat klíče a spravovat licencování pouze ve své skupině.
- **Reseller (prodejce) / End-user (koncový uživatel)**: podle nastavených pravidel přístupu (policy).

Přístup k licencování je chráněn přes pravidla přístupu (policy), aby se nedalo “dostat” na cizí data jen úpravou URL.

### Licencování (UI)

V levém menu jsou licencování rozdělené do samostatných částí:

- **Přehled licencí**: poslední klíče, rychlý přehled stavu, detail licence.
- **Generování licencí**: formulář pro vygenerování klíčů. Stránka po vygenerování zobrazuje pouze naposledy vytvořenou várku.
- **Instalace**: přehled posledních instalačních událostí (historií).

UI používá HTMX pro částečné aktualizace bez nutnosti plného znovunačtení stránky.

### Skupiny (multi-tenant oddělení)

Skupiny (v kódu `KeyGroups`) jsou hlavní mechanismus multi-tenant oddělení. Jedna skupina reprezentuje „obchodního vlastníka“ licenčních klíčů (typicky distributor).

Zjednodušeně:

- **Každý licenční klíč (`SerialNumber`) má `GroupId`** – patří do jedné skupiny.
- **Každý ne-admin uživatel má členství v právě jedné skupině** – podle toho se mu „oříznou“ data (scoping).
- **Admin není scopeovaný** (může přepínat/filtruje přes skupiny), a navíc jako jediný může klíče mezi skupinami přesouvat.

#### Datový model (EF Core)

Multi-tenant oddělení není řešené přes Identity role, ale přes **samostatné tabulky**:

- `KeyGroups`
  - `Id` (GUID), `Name`, `CreatedAt`
  - `Name` je unikátní
- `KeyGroupMembers`
  - vazba uživatel → skupina (`UserId`, `GroupId`)
  - **omezení „1 členství na uživatele“** je vynucené unikátním indexem na `UserId`
- `SerialNumbers`
  - `GroupId` je foreign key na `KeyGroups` (indexované)
  - `GroupId` má default `AdminGroupId` (pokud by někdo vytvořil klíč bez explicitní skupiny)
- `SerialNumberGroupAudits`
  - audit přesunů: `FromGroupId`, `ToGroupId`, `ChangedByUserId`, `ChangedAt`

#### Provisioning (automatické zakládání skupin)

Skupiny se v aktuální verzi **nevytváří ručně přes UI**. Vznikají automaticky v těchto situacích:

- **Admin pool**: existuje pevná „admin“ skupina (`AdminGroupId`, název „Admin pool“).
- **Distributor**: při vytvoření účtu distributora se mu **založí vlastní skupina** a distributor se do ní automaticky přidá.
- **Reseller / End-user vytvořený distributorem nebo resellerem**: nový účet se automaticky přiřadí do stejné skupiny jako tvůrce.
- **Reseller / End-user vytvořený adminem**: admin vybírá cílovou skupinu (typicky skupina konkrétního distributora).

Pozn.: Aplikace má navíc „bezpečnostní“ provisioning při startu – pokud najde uživatele v roli `Reseller` bez členství ve skupině, přiřadí ho do `Admin pool` (aby nedošlo k chybovým stavům typu „uživatel nemá skupinu“).

#### Scoping (omezení dat) v aplikaci

Princip scoping je jednoduchý: u většiny licenčních dotazů se použije `GroupId`:

- Admin:
  - může pracovat „napříč skupinami“
  - v UI typicky posílá `groupId` v query stringu (nebo ponechá prázdné pro „vše“ podle obrazovky)
- Ne-admin (Distributor/Reseller/End-user):
  - aplikace si vezme skupinu z `KeyGroupMembers` pro přihlášeného uživatele
  - jakýkoliv `groupId` v URL se ignoruje (uživatel nemůže „přepnout tenant“ ručně)
  - pokud uživatel nemá členství ve skupině, licenční stránky vrací `Forbid()` a UI ukáže hlášku

#### Přesun klíče mezi skupinami + audit

Pouze Admin smí přesouvat licenční klíče mezi skupinami:

- operace změní `SerialNumbers.GroupId`
- zároveň se zapíše auditní záznam do `SerialNumberGroupAudits` (odkud/kam/kdo/kdy)

To je důležité pro dohledatelnost („proč najednou klíč patří jinému distributorovi?“).

#### Praktický příklad

1) Admin vytvoří distributora → vznikne mu skupina.
2) Distributor vygeneruje klíče → všechny mají `GroupId` distributora.
3) Distributor vidí pouze své klíče (scoping).
4) Admin může klíč přesunout do jiné skupiny (např. při změně vlastníka) → vznikne auditní stopa.

## Možná budoucí rozšíření

Nápady na smysluplná rozšíření, která na současnou architekturu přirozeně navazují:

- **Produkty a plány**: navázat klíče na konkrétní produkt/edici, délku podpory, přepínače funkcí (feature flags).
- **Objednávky a fakturace**: import/export objednávek, napojení na fakturační systém, DPH, párování plateb.
- **Samoobslužný portál pro koncové uživatele**: správa zařízení, odpojení zařízení, zobrazení aktivací, změny profilu.
- **Integrace**: notifikační volání (webhooky) pro události (vygenerováno/přiřazeno/revokováno), programové rozhraní (API) pro e‑shop, jednotné přihlášení (SSO – např. Entra ID / OAuth2).
- **Přehledy a statistiky**: přehledy pro admin/distributory (aktivace v čase, nejčastější zařízení, top produkty).
- **Jemnější model oddělení partnerů**: podskupiny (prodejce pod distributorem), více členství na uživatele (pokud bude potřeba).
- **Bezpečnost a provoz**: rotace klíčů, detailnější auditní záznamy, „zpevnění“ přihlašovacích/emailových toků (hardening), omezování pokusů (rate limiting) dle IP/uživatele, alerting.
- **Provoz a automatizace (DevOps)**: docker-compose (spuštění SQL + app v kontejnerech), CI (automatické build/test), automatické migrace na testovací/provozní prostředí (staging/prod).

## Architektura / komponenty

Projekt je klasická ASP.NET Core MVC aplikace s oddělením zodpovědností do vrstev:

- **Controllers/**: MVC controllery a endpointy (včetně HTMX partialů) pro auth, účet a licencování.
- **Views/**: Razor šablony (stránky) + dílčí šablony (partial views), kde se skládá UI a HTMX interakce.
- **Data/**: `ApplicationDbContext` (EF Core) + migrace v `Data/Migrations/`.
- **Models/**: view-modely a doménové modely (licence, skupiny, audit, instalace, auth input modely).
- **Services/**: aplikační logika (generování licencí, scoping/skupiny, 2FA, email, tokeny, QR).
- **Resources/**: lokalizace `.resx` (CZ/EN) pro UI texty a validační hlášky.
- **wwwroot/**: statické assety, Tailwind output, vendor knihovny.

Klíčové principy:

- **Oddělení partnerů (multi-tenant) a omezení dat (scoping)** je řešené přes KeyGroup členství a `GroupId` na licenčních klíčích.
- **Autorizace** je vynucena pravidly přístupu (policy) / rolemi v `Program.cs` a kontrolami v controllerech.
- **Audit** se používá pro administrátorské zásahy (např. přesun licence mezi skupinami).

## Roadmapa

Praktická roadmapa po menších krocích (orientačně):

- **MVP (zabezpečení a úklid)**
  - doplnit `.env` / User Secrets (lokální tajné údaje),
  - zkontrolovat logování citlivých údajů,
  - sjednotit texty v UI a validace na licencování.
- **v1 (produktově použitelná verze)**
  - produkty/plány + navázání licencí na produkt,
  - export/import (CSV) pro distributory,
  - přehledy/reporting (aktivace v čase, top zařízení).
- **v2 (integrace a automatizace)**
  - veřejné programové rozhraní (API) + tokeny, notifikační volání (webhooky),
  - napojení na e‑shop/fakturaci,
  - pokročilé scénáře oddělení partnerů (hierarchie distributor → prodejce).

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

### Bootstrap majitel (Majitel)

Aplikace umí v Development vytvořit také účet **Majitel** (pokud žádný Majitel neexistuje). Majitel je governance role pro správu adminů.

Příklad lokální konfigurace (do `appsettings.Development.json`):

```json
{
  "BootstrapOwner": {
    "Enabled": true,
    "Email": "owner@local.test",
    "Password": "ChangeMe!12345",
    "RequireChangeOnFirstLogin": true
  }
}
```

Správa admin účtů je dostupná v UI pod `/owner` (v menu jako „Správa adminů“).

Pozn.: Admin účty se vytváří a spravují výhradně přes Majitele (UI `/owner`).

## DB / migrace

Pokud budeš chtít migrovat ručně:

```bash
dotnet ef database update
```

(Pokud nemáš nainstalovaný EF CLI: `dotnet tool install --global dotnet-ef`)
