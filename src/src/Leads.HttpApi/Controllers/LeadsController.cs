using Leads.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Leads.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class LeadsController : AbpControllerBase
{
    protected LeadsController()
    {
        LocalizationResource = typeof(LeadsResource);
    }
}
