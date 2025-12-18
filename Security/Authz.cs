namespace DigitalniProdukty.Security;

public static class Authz
{
    public static class Claims
    {
        public const string ForceCredentialsChange = "ForceCredentialsChange";
    }

    public static class Roles
    {
        // Governance role: exactly one user should have this role.
        // This role manages Admin accounts.
        public const string Owner = "Majitel";

        public const string Admin = "Admin";
        public const string EndUser = "EndUser";
        public const string Reseller = "Reseller";
        public const string Distributor = "Distributor";

        // All known roles (including governance role).
        public static readonly string[] All = [Owner, Admin, EndUser, Reseller, Distributor];

        // Roles that an Admin is allowed to assign/manage via the Admin UI.
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
