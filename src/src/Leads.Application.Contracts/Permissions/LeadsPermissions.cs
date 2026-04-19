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
}
