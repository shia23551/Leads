using Leads.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace Leads.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(LeadsEntityFrameworkCoreModule),
    typeof(LeadsApplicationContractsModule)
)]
public class LeadsDbMigratorModule : AbpModule
{
}
