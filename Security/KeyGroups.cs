using System;

namespace DigitalniProdukty.Security;

public static class KeyGroups
{
    // Konstanta pro „admin pool“ (speciální skupina používaná pro systémové scénáře).
    public static readonly Guid AdminGroupId = new("00000000-0000-0000-0000-000000000001");

    // Zobrazovaný název admin skupiny (UI, seedování).
    public const string AdminGroupName = "Admin pool";
}

/*
Podrobnosti (vazby a použité části)

- Účel: společné konstanty pro KeyGroup doménu (zejména „Admin pool“).
- Použití v aplikaci:
    - `KeyGroupProvisioningService`: zajišťuje existenci admin group.
    - `AuthController` / `LicensingAdminController`: defaultní groupId pro admin flow.
    - `AdminController`: bezpečnostní pravidlo zakazující přiřazení ne-admin rolí do admin group.
- Poznámka:
    - `AdminGroupId` je pevný GUID, aby byl stabilní napříč prostředími/migracemi.
*/
