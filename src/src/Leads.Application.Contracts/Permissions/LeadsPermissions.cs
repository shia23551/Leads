namespace Leads.Permissions;

public static class LeadsPermissions
{
    public const string GroupName = "Leads";

    public static class Companies
    {
        public const string Default = GroupName + ".Companies";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class OpenIddictApplications
    {
        public const string Default = GroupName + ".OpenIddictApplications";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class OpenIddictScopes
    {
        public const string Default = GroupName + ".OpenIddictScopes";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class SharedAccounts
    {
        public const string Default = GroupName + ".SharedAccounts";
        public const string ManageMembers = Default + ".ManageMembers";
        public const string AcceptInvitation = Default + ".AcceptInvitation";
    }
}
