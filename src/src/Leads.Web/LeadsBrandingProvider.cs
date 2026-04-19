using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using Leads.Localization;

namespace Leads.Web;

[Dependency(ReplaceServices = true)]
public class LeadsBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<LeadsResource> _localizer;

    public LeadsBrandingProvider(IStringLocalizer<LeadsResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
