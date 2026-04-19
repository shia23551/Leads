using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace Leads.Data;

/* This is used if database provider does't define
 * ILeadsDbSchemaMigrator implementation.
 */
public class NullLeadsDbSchemaMigrator : ILeadsDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
