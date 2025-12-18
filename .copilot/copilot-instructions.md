
# GitHub Copilot Instructions for DigitalniProdukty

## Kontext projektu

Tento projekt je **ASP.NET Core MVC (net10.0)** s:
- **Razor Views + ASP.NET Core Identity (bez Identity UI Razor Pages)**
- **HTMX-first UI** (fragmenty/partialy)
- **Tailwind CSS v4 (CLI)** bez dalších UI knihoven
- Minimalistický frontend: **bez jQuery**, bez Alpine, bez DaisyUI

## Knowledge Base (povinné)

- Při generování nebo úpravách HTMX (atributy `hx-*`, fragmenty, HX-Redirect/HX-Trigger, HTMX event hooky) se vždy řiď a případně se výslovně poraď s dokumentem: `.copilot/HTMX_Knowledge_Base.md`.
- Při generování nebo úpravách Tailwind CSS (utility třídy, `@source`, `@layer`, komponentní třídy `app-*`) se vždy řiď a případně se výslovně poraď s dokumenty ve složce: `.copilot/Tailwind_Knowledge_Base/*`.

Primární cíl: jednoduché, konzistentní UI ve stylu „Admin“ pomocí Tailwind utility tříd + pár vlastních komponentních tříd `app-*`.

## Tailwind CSS v4 pravidla

- Nepřidávej `tailwind.config.js` (v tomto projektu se konfigurace drží přímo v CSS).
- Vstup je `wwwroot/css/site.css`, výstup je `wwwroot/css/output.css`.
- Skenování obsahuje jen Razor soubory přes `@source` direktivy.

### Detekce tříd (v4)

- Tailwind skenuje soubory jako **plain text**. Nekonstruuj názvy tříd dynamicky (řetězení/interpolace typu `text-@color-600`).
- Pokud potřebuješ podmíněné třídy, vždy používej **celé, staticky čitelné** tokeny (např. ternární výběr z hotových stringů).
- Když je potřeba „safelisting“, použij `@source inline("...")` ve `wwwroot/css/site.css`.

Build skripty:
- `npm run build:css`
- `npm run watch:css`

## UI & styly

- Používej **Tailwind barvy** (např. `gray-*`, `slate-*`, `indigo-*`) – žádná semantika DaisyUI.
- Preferuj existující komponentní třídy z `wwwroot/css/site.css`:
  - `app-card`, `app-card-header`, `app-card-body`
  - `app-btn-primary`, `app-btn-secondary`
  - `app-form`, `app-form-section*`, `app-input`, `app-check*`
  - `app-nav-link`, `app-nav-link-active`
- Nezaváděj nové design systémy, barvy, fonty nebo „extra“ UI knihovny.

### Preflight (Tailwind base)

- Seznamy jsou defaultně bez odrážek; pro skutečné seznamy použij `role="list"` nebo přidej `list-disc/list-decimal`.

## HTMX pravidla (HTMX-first)

- Preferuj server-rendered HTML (partialy) a HTMX navigaci.
- Layout je „shell“ s `hx-boost`, `hx-push-url`, `hx-target="#main"`.
- Pro rozlišení HTMX vs full load používej `Request.IsHtmx()` (Htmx.Net NuGet balíček), tj. podle `HX-Request`.
- Antiforgery pro HTMX: token posílat v headeru `RequestVerificationToken` (už je řešeno v layout skriptu).
- Aktivní položku sidebaru udržuj kompatibilní s HTMX history (už je řešeno v `wwwroot/js/site.js`).

### Server-side helpery (Htmx.Net)

- Preferuj `Request.IsHtmx()` místo vlastní implementace.
- Pro HTMX response hlavičky preferuj `Response.Htmx(...)` (např. `h.Redirect(url)`, `h.Refresh()`, `h.ReplaceUrl(...)`, `h.PushUrl(...)`, `h.WithTrigger(...)`) – používej podle potřeby.

### UX pro requesty (indikátory)

- Pro „loading“ stav používej `class="htmx-indicator"` + případně `hx-indicator="#id"`.
- Pro disable během requestu používej `hx-disabled-elt` (typicky na formulář nebo tlačítko).

### Konvence pro MVC controllery

- Pro stránky, které fungují jak na full load, tak přes HTMX:
  - Pokud `Request.IsHtmx()` → vracej `PartialView(...)` (jen obsah pro `#main`).
  - Jinak → vracej `View(...)` (plná stránka včetně layoutu).
- Drž stejný ViewModel pro obě varianty (nezdvojuj logiku).
- Nepoužívej JSON API pro běžné „page“ flow; preferuj HTML.

### Konvence pro fragment endpoints

- Fragmenty drž v samostatném controlleru (např. `FragmentsController`) pod prefixem `/_fragments/*`.
- Fragment musí vracet jen relevantní HTML úsek (typicky partial z `Views/Shared/*`).
- Fragmenty používej pro části layoutu, které se mají aktualizovat bez full reloadu (např. login stav v headeru).

### Redirect & navigace

- Pro HTMX post-backy preferuj redirect přes HTMX mechanismus (např. `HX-Redirect`) místo vracení celé stránky.
- U běžných (non-HTMX) POST zachovej standardní `RedirectToAction` / `RedirectToPage`.

### OOB swapy (pokud je potřeba)

- Pro aktualizaci části layoutu „mimo target“ můžeš použít `hx-swap-oob` / `hx-select-oob`.
- Používej jen když je to jednodušší než explicitní fragment request (např. login/header refresh).

### Cache/headers

- Reakce, které se liší mezi HTMX a non-HTMX, musí respektovat `Vary: HX-Request` (už je řešeno middlewarem).

## Identity

- Identity používáme jako backend (UserManager/SignInManager), UI je řešené přes MVC (`/auth/*`, `/account/*`).
- Nevracej JSON/SPA logiku; vše drž v HTML/partial přístupu.

## JavaScript

- Preferuj vanilla JS v `wwwroot/js/site.js`.
- Nezaváděj jQuery, validační pluginy ani velké frameworky.
- Validace je záměrně minimalistická a HTMX-kompatibilní.

## Co nedělat

- Nepřidávej DaisyUI ani jiné komponentní frameworky.
- Nepřepisuj existující styling na úplně nový systém bez explicitního požadavku.
- Nevymýšlej nové stránky/UX navíc mimo zadání.

### Vyhni se

- Inline stylům (`style="..."`) – preferuj Tailwind utility nebo existující `app-*` třídy.
- `!important` (v Tailwind použij spíš `!` suffix jen když je to nutné).
- Přidávání další UI knihovny (DaisyUI/Bootstrap/Alpine/jQuery).

## Kde co je (rychlá mapa)

- Layout / shell: `Views/Shared/_Layout.cshtml`
  - HTMX shell atributy (`hx-boost`, `hx-push-url`, `hx-target="#main"`)
  - Antiforgery header injekce pro HTMX (`RequestVerificationToken`)
  - Sidebar + topbar

- Tailwind vstup: `wwwroot/css/site.css`
  - `@source` pro Razor soubory
  - Komponentní třídy: `app-card`, `app-btn-*`, `app-form-section*`, `app-nav-link*`

- Tailwind výstup: `wwwroot/css/output.css`
  - Generovaný soubor (needitovat ručně)

- JS: `wwwroot/js/site.js`
  - Sidebar toggle
  - HTMX event hooky (history / aktivní link)
  - Minimal validace bez jQuery
  - `auth-changed` trigger pro refresh login headeru

- HTMX detekce requestu: Htmx.Net (`Request.IsHtmx()` podle headeru `HX-Request`)

- Fragmenty: `Controllers/FragmentsController.cs`
  - Např. `/_fragments/login` vrací `Views/Shared/_LoginPartial.cshtml`

- Identity UI (Razor Pages): removed (auth/account implemented via MVC)
  - Stránky jsou stylované přes `app-card` + `app-form-section*`

## Build & Windows poznámky

- Když běží aplikace, může být zamčený `bin/Debug/.../*.dll` a Debug build pak padá.
- Pro ověření použij:
  - `dotnet build -c Release /p:UseAppHost=false`
  - nebo zastav běžící proces aplikace před Debug buildem.

## Externí odkazy

- Tailwind CSS: https://tailwindcss.com
- HTMX: https://htmx.org

