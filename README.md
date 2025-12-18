# DigitalniProdukty

ASP.NET Core MVC aplikace pro správu digitálních produktů a licencí (Identity + EF Core, HTMX, Tailwind).

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
