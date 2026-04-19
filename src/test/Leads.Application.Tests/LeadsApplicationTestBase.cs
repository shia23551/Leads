using Volo.Abp.Modularity;

namespace Leads;

public abstract class LeadsApplicationTestBase<TStartupModule> : LeadsTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
