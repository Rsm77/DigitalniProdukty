# Digitální produkty – princip a použití (graficky)

Tento dokument je „vizuální mapa“ aplikace: kdo ji používá, jaké jsou hlavní obrazovky a jak spolu komunikují server, databáze a HTMX.

> Pozn.: Diagramy jsou v Mermaid. Ve VS Code je zobrazíte např. rozšířením pro Mermaid, na GitHubu se většinou rendrují automaticky.

---

## 1) Kontext: co aplikace dělá

- Aplikace spravuje **licenční klíče (serial numbers)** a jejich **přiřazení uživatelům**.
- Evidence zahrnuje **instalace** (události), **zařízení**, **skupiny** (key groups) a **uživatelské role**.
- UI je klasické ASP.NET MVC/Razor, ale navigace a část akcí je optimalizovaná přes **HTMX** (částečné přenačítání a fragmenty).

---

## 2) Role a oprávnění (kdo co může)

```mermaid
flowchart TB
  U[Uživatel] --> R{Role}

  R -->|Owner| OWNER[Owner]
  R -->|Admin| ADMIN[Admin]
  R -->|Distributor| DIST[Distributor]
  R -->|Reseller| RES[Reseller]
  R -->|EndUser| END[EndUser]

  OWNER --> O1[Správa adminů]

  ADMIN --> A1[Správa uživatelů – role a skupiny]
  ADMIN --> A2[Správa licencí napříč skupinami]

  DIST --> D1[Vytváření účtů – omezeně]
  DIST --> D2[Práce s licencemi ve svém kontextu]

  RES --> R1[Typicky práce s přidělenými licencemi]

  END --> E1[Vidí své serialy]
  END --> E2[Profil / Heslo / 2FA]
```

---

## 3) Hlavní obrazovky (navigace aplikace)

```mermaid
flowchart LR
  HOME[Home] --> ACC[Account]
  HOME --> LIC[Licensing Admin]
  HOME --> ADM[Admin]
  HOME --> OWN[Owner]

  ACC --> ACC_P[Profil]
  ACC --> ACC_W[Heslo]
  ACC --> ACC_2FA[2FA]
  ACC --> ACC_S[Serialy]

  LIC --> LIC_I[Index / Latest keys]
  LIC --> LIC_G[Generate]
  LIC --> LIC_INS[Installations]
  LIC_I --> LIC_D[Details – dialog]

  ADM --> ADM_I[Users/Groups management]
  OWN --> OWN_I[Admins list]
  OWN_I --> OWN_E[Edit]
  OWN_I --> OWN_RP[Reset password]
  OWN_I --> OWN_DEL[Delete confirm – modal]
```

---

## 4) Princip fungování UI s HTMX

Aplikace používá kombinaci:
- **Klasická MVC navigace** (odkazy a stránky),
- **HTMX „boost“ navigace**: klik na odkaz často nenačítá celou stránku, ale jen obsahový panel,
- **Fragmenty**: dílčí části layoutu (např. login box / sidebar) se dají přerenderovat samostatně.

### 4.1 „Boost“ navigace (výměna `#main`)

```mermaid
sequenceDiagram
  autonumber
  participant B as Browser
  participant L as Layout – hx-boost
  participant S as Server – MVC

  B->>L: Uživatel klikne na odkaz
  L->>S: HTMX request (GET)
  S-->>L: HTML odpověď (stránka/view)
  L-->>B: HTMX swap do #main (bez full reload)
  Note over L,B: URL se aktualizuje (hx-push-url)
```

### 4.2 Fragmenty (sidebar/login) po změně přihlášení

```mermaid
sequenceDiagram
  autonumber
  participant B as Browser
  participant S as Server – FragmentsController

  Note over B: Přihlášení/odhlášení vyvolá událost
  B->>B: dispatch "auth-changed" on BODY
  B->>S: hx-get /_fragments/login
  S-->>B: _LoginPartial HTML
  B->>B: swap do #login-partial

  B->>S: hx-get /_fragments/sidebar-nav
  S-->>B: _SidebarNav HTML
  B->>B: swap do #sidebar-nav
```

### 4.3 HTMX formuláře: „vrať mi znovu kartu“

Častý vzor v auth/account view:
- formulář má `hx-post`,
- cílí na `#...-card`,
- server vrátí **znovu celý card** (včetně validace/toastu),
- HTMX provede `outerHTML` swap.

```mermaid
sequenceDiagram
  autonumber
  participant B as Browser
  participant S as Server – Controller + View

  B->>S: hx-post /Account/Profile (form data)
  S-->>B: HTML (znovu celý #profile-card)
  B->>B: swap outerHTML pro #profile-card
```

### 4.4 Detail v dialogu (LicensingAdmin)

- Tabulka „Latest keys“ otevře detail přes JS (otevře `<dialog>`),
- detail se typicky dotáhne jako partial (`_Details`) do `#license-details-body`,
- akce uvnitř detailu (Assign/Revoke/Transfer…) používají `hx-post` a aktualizují jen obsah dialogu.

```mermaid
sequenceDiagram
  autonumber
  participant B as Browser
  participant S as Server – LicensingAdmin

  B->>B: click key -> open dialog
  B->>S: GET /LicensingAdmin/Details?id=...
  S-->>B: HTML partial _Details
  B->>B: swap innerHTML do #license-details-body

  B->>S: hx-post /LicensingAdmin/Assign (email, context=details)
  S-->>B: HTML partial _Details (aktualizovaný)
  B->>B: swap innerHTML do #license-details-body
```

---

## 5) Datový model (zjednodušeně)

```mermaid
erDiagram
  IdentityUser {
    string Id
    string Email
  }

  SerialNumber {
    int Id
    string Key
    int MaxDevices
    datetime CreatedAt
    bool IsRevoked
    string OwnerUserId
    guid GroupId
  }

  Device {
    int Id
    string HardwareIdentifier
    datetime FirstSeenAt
    datetime LastSeenAt
  }

  Installation {
    int Id
    datetime OccurredAt
    string EventType
    string IpAddress
  }

  KeyGroup {
    guid Id
    string Name
  }

  KeyGroupMember {
    int Id
    guid GroupId
    string UserId
    datetime AddedAt
  }

  IdentityUser ||--o{ SerialNumber : owns
  SerialNumber ||--o{ Installation : has
  Device ||--o{ Installation : has
  KeyGroup ||--o{ SerialNumber : scopes
  KeyGroup ||--o{ KeyGroupMember : members
  IdentityUser ||--o{ KeyGroupMember : joins
```

---

## 6) Typické scénáře použití

### 6.1 End-user: prohlédnutí vlastních serialů

```mermaid
flowchart TB
  A[Uživatel přihlášen] --> B[Account / Serials]
  B --> C[Server načte serialy uživatele]
  C --> D[Razor vykreslí tabulku]
  D --> E[Uživatel vidí Key/Status/Created]
```

### 6.2 Admin/Distributor: generování klíčů

```mermaid
flowchart TB
  G[LicensingAdmin / Generate] --> P[Vyplní parametry]
  P --> S[POST Generate]
  S --> K[Server vygeneruje klíče + uloží do DB]
  K --> R[Vykreslí zpět stránku s tabulkou nových klíčů]
  R -->|HTMX outerHTML| UI[UI se aktualizuje bez reloadu]
```

### 6.3 Owner: mazání admin účtu s potvrzením (modal)

```mermaid
sequenceDiagram
  autonumber
  participant B as Browser
  participant S as Server – Owner

  B->>S: hx-get /owner/admins/delete-confirm (userId)
  S-->>B: HTML fragment _DeleteConfirm
  B->>B: swap do #owner-modal (overlay)

  B->>S: POST /Owner/DeleteAdmin (anti-forgery)
  S-->>B: Redirect /Owner/Index (+ TempData toast)
  Note over B: layout/stránka pak ukáže toast
```

---

## 7) Kde hledat konkrétní implementaci

- Layout + globální HTMX nastavení: `Views/Shared/_Layout.cshtml`
- Fragment endpointy (sidebar/login): `Controllers/FragmentsController.cs`
- Licence (index/generate/details/installations): `Controllers/LicensingAdminController.cs` + `Views/LicensingAdmin/*`
- Owner správa adminů: `Controllers/OwnerController.cs` + `Views/Owner/*`
- Account (profil/heslo/2FA/serials): `Controllers/AccountController.cs` + `Views/Account/*`
- Identity auth flow: `Controllers/AuthController.cs` + `Views/Auth/*`

---

## 8) Legenda HTMX atributů (rychlá)

- `hx-boost="true"`: promění běžné odkazy/formy na HTMX requesty (XHR) – typicky bez full reload.
- `hx-target="..."`: kam se má vložit odpověď.
- `hx-swap="innerHTML"|"outerHTML"`: jak se odpověď vloží (jen obsah vs. celý element).
- `hx-trigger="..."`: kdy se request spustí (např. `load`, `change`, nebo custom event `foo from:body`).
- `hx-include="..."`: které další inputy/pole se mají poslat v requestu.
- `hx-vals='{"k":"v"}'`: doplní do requestu hodnoty (často pro jednoduché parametry).
