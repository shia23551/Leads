using Volo.Abp.Modularity;

namespace Leads;

[DependsOn(
    typeof(LeadsApplicationModule),
    typeof(LeadsDomainTestModule)
)]
public class LeadsApplicationTestModule : AbpModule
{

}
