namespace DigitalniProdukty.Models.Licensing
{
    // Typ události instalace/aktivace pro audit a reporting.
    public enum InstallationEventType
    {
        Activate = 1,
        Deactivate = 2,
        Run = 3
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: jednotný seznam událostí, které se ukládají k `InstallationModel`.
- Vazby na zbytek aplikace:
  - `Models/SerialNum/InstallationModel.EventType` používá tento enum.
  - Hodnoty jsou explicitně číslované kvůli stabilitě při ukládání do DB.
*/
