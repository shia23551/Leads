using System.Threading.Tasks;
using Leads.Localization;
using Leads.Permissions;
using Leads.MultiTenancy;
using Volo.Abp.SettingManagement.Web.Navigation;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Identity;
using Volo.Abp.Identity.Web.Navigation;
using Volo.Abp.UI.Navigation;
using Volo.Abp.TenantManagement.Web.Navigation;

namespace Leads.Web.Menus;

public class LeadsMenuContributor : IMenuContributor
{
    public async Task ConfigureMenuAsync(MenuConfigurationContext context)
    {
        if (context.Menu.Name == StandardMenus.Main)
        {
            await ConfigureMainMenuAsync(context);
        }
    }

    private static Task ConfigureMainMenuAsync(MenuConfigurationContext context)
    {
        var l = context.GetLocalizer<LeadsResource>();

        //Home
        context.Menu.AddItem(
            new ApplicationMenuItem(
                LeadsMenus.Home,
                l["Menu:Home"],
                "~/",
                icon: "fa fa-home",
                order: 1
            )
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                LeadsMenus.Companies,
                l["Menu:Companies"],
                "~/Companies",
                icon: "fa fa-building",
                order: 2,
                requiredPermissionName: LeadsPermissions.Companies.Default
            )
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                LeadsMenus.Organizations,
                l["Menu:Organizations"],
                "~/Organizations",
                icon: "fa fa-sitemap",
                order: 3
            )
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                LeadsMenus.Users,
                l["Menu:Users"],
                "~/Users",
                icon: "fa fa-users",
                order: 4,
                requiredPermissionName: IdentityPermissions.Users.Default
            )
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                LeadsMenus.OpenIddictApplications,
                l["Menu:OpenIddictApplications"],
                "~/OpenIddict/Applications",
                icon: "fa fa-key",
                order: 5,
                requiredPermissionName: LeadsPermissions.OpenIddictApplications.Default
            )
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                LeadsMenus.OpenIddictScopes,
                l["Menu:OpenIddictScopes"],
                "~/OpenIddict/Scopes",
                icon: "fa fa-shield-alt",
                order: 6,
                requiredPermissionName: LeadsPermissions.OpenIddictScopes.Default
            )
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                LeadsMenus.Passkeys,
                l["Menu:Passkeys"],
                "~/Passkeys/Manage",
                icon: "fa fa-fingerprint",
                order: 7
            )
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                LeadsMenus.SharedAccountMembers,
                l["Menu:SharedAccountMembers"],
                "~/SharedAccounts/Members",
                icon: "fa fa-user-plus",
                order: 8,
                requiredPermissionName: LeadsPermissions.SharedAccounts.ManageMembers
            )
        );

        context.Menu.AddItem(
            new ApplicationMenuItem(
                LeadsMenus.SharedAccountSelectTenant,
                l["Menu:SelectTenant"],
                "~/SharedAccounts/SelectTenant",
                icon: "fa fa-exchange-alt",
                order: 9
            )
        );


        //Administration
        var administration = context.Menu.GetAdministration();
        administration.Order = 6;

        //Administration->Identity
        administration.SetSubItemOrder(IdentityMenuNames.GroupName, 1);
    
        administration.SetSubItemOrder(TenantManagementMenuNames.GroupName, 1);
        
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 3);

        //Administration->Settings
        administration.SetSubItemOrder(SettingManagementMenuNames.GroupName, 8);
        
        return Task.CompletedTask;
    }
}
