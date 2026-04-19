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
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<LeadsResource>(name);
    }
}
