using Leads.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Leads.Web.Pages;

public abstract class LeadsPageModel : AbpPageModel
{
    protected LeadsPageModel()
    {
        LocalizationResourceType = typeof(LeadsResource);
    }
}
