namespace DigitalniProdukty.Security;

public static class Authz
{
    public static class Claims
    {
        public const string ForceCredentialsChange = "ForceCredentialsChange";
    }

    public static class Roles
    {
        // Governance role: přesně jeden uživatel by měl mít tuto roli.
        // Tato role spravuje účty Admin (a obecně je „nejvyšší“ v rámci aplikace).
        public const string Owner = "Majitel";

        public const string Admin = "Admin";
        public const string EndUser = "EndUser";
        public const string Reseller = "Reseller";
        public const string Distributor = "Distributor";

        // Všechny známé role (včetně governance role).
        public static readonly string[] All = [Owner, Admin, EndUser, Reseller, Distributor];

        // Role, které může Admin přiřazovat/spravovat přes Admin UI.
        public static readonly string[] AdminManageable = [Admin, EndUser, Reseller, Distributor];
    }

    public static class Policies
    {
        public const string LicensingAdmin = "Licensing.Admin";

        public const string Users_CreateEndUser = "Users.CreateEndUser";

        public const string SerialNumbers_ViewOwn = "SerialNumbers.ViewOwn";
        public const string SerialNumbers_Sell = "SerialNumbers.Sell";
        public const string SerialNumbers_Generate = "SerialNumbers.Generate";
    }
}

/*
Podrobnosti (vazby a použité části)

- Účel: centralizace všech autorizačních konstant (roles, policies, claims).
- Použití v aplikaci:
    - `Program`: registrace `AddAuthorization` policy mapování.
    - Controllery: atributy `[Authorize]`, `[Authorize(Roles=...)]` a policy-based přístup.
    - `AccountController`: claim `ForceCredentialsChange` pro vynucení „first-login“ změn.
- Poznámky:
    - Role `Owner` je governance (správa admin účtů) a má být unikátní.
    - Pole `Roles.All` slouží i pro seedování rolí při startu aplikace.
*/
