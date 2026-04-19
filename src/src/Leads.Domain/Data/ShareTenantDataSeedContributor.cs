using System.Threading.Tasks;
using Leads.MultiTenancy;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.TenantManagement;

namespace Leads.Data;

public class ShareTenantDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ITenantRepository _tenantRepository;
    private readonly TenantManager _tenantManager;

    public ShareTenantDataSeedContributor(
        ITenantRepository tenantRepository,
        TenantManager tenantManager)
    {
        _tenantRepository = tenantRepository;
        _tenantManager = tenantManager;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId.HasValue)
        {
            return;
        }

        var existingTenant = await _tenantRepository.FindByNameAsync(LeadsTenantNames.Share);
        if (existingTenant != null)
        {
            return;
        }

        var tenant = await _tenantManager.CreateAsync(LeadsTenantNames.Share);
        await _tenantRepository.InsertAsync(tenant, autoSave: true);
    }
}
