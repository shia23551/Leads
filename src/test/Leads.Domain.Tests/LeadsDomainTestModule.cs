using Volo.Abp.Modularity;

namespace Leads;

[DependsOn(
    typeof(LeadsDomainModule),
    typeof(LeadsTestBaseModule)
)]
public class LeadsDomainTestModule : AbpModule
{

}
