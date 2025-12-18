namespace DigitalniProdukty.Security;

public static class Authz
{
    public static class Claims
    {
        public const string ForceCredentialsChange = "ForceCredentialsChange";
    }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string EndUser = "EndUser";
        public const string Reseller = "Reseller";
        public const string Distributor = "Distributor";

        public static readonly string[] All = [Admin, EndUser, Reseller, Distributor];
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
