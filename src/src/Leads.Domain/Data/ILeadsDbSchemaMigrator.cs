using System.Threading.Tasks;

namespace Leads.Data;

public interface ILeadsDbSchemaMigrator
{
    Task MigrateAsync();
}
