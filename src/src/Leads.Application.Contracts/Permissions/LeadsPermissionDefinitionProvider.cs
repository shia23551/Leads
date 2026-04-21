using Leads.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Leads.Permissions;

public class LeadsPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        var myGroup = context.AddGroup(LeadsPermissions.GroupName);
        var companiesPermission = myGroup.AddPermission(LeadsPermissions.Companies.Default, L("Permission:Companies"));

        companiesPermission.AddChild(LeadsPermissions.Companies.Create, L("Permission:Companies.Create"));
        companiesPermission.AddChild(LeadsPermissions.Companies.Edit, L("Permission:Companies.Edit"));
        companiesPermission.AddChild(LeadsPermissions.Companies.Delete, L("Permission:Companies.Delete"));

        var openIddictApplicationsPermission = myGroup.AddPermission(
            LeadsPermissions.OpenIddictApplications.Default,
            L("Permission:OpenIddictApplications"));

        openIddictApplicationsPermission.AddChild(
            LeadsPermissions.OpenIddictApplications.Create,
            L("Permission:OpenIddictApplications.Create"));
        openIddictApplicationsPermission.AddChild(
            LeadsPermissions.OpenIddictApplications.Edit,
            L("Permission:OpenIddictApplications.Edit"));
        openIddictApplicationsPermission.AddChild(
            LeadsPermissions.OpenIddictApplications.Delete,
            L("Permission:OpenIddictApplications.Delete"));

        var openIddictScopesPermission = myGroup.AddPermission(
            LeadsPermissions.OpenIddictScopes.Default,
            L("Permission:OpenIddictScopes"));

        openIddictScopesPermission.AddChild(
            LeadsPermissions.OpenIddictScopes.Create,
            L("Permission:OpenIddictScopes.Create"));
        openIddictScopesPermission.AddChild(
            LeadsPermissions.OpenIddictScopes.Edit,
            L("Permission:OpenIddictScopes.Edit"));
        openIddictScopesPermission.AddChild(
            LeadsPermissions.OpenIddictScopes.Delete,
            L("Permission:OpenIddictScopes.Delete"));

        var sharedAccountsPermission = myGroup.AddPermission(
            LeadsPermissions.SharedAccounts.Default,
            L("Permission:SharedAccounts"));

        sharedAccountsPermission.AddChild(
            LeadsPermissions.SharedAccounts.ManageMembers,
            L("Permission:SharedAccounts.ManageMembers"));

        sharedAccountsPermission.AddChild(
            LeadsPermissions.SharedAccounts.AcceptInvitation,
            L("Permission:SharedAccounts.AcceptInvitation"));
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<LeadsResource>(name);
    }
}
