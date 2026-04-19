using Leads.Samples;
using Xunit;

namespace Leads.EntityFrameworkCore.Domains;

[Collection(LeadsTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<LeadsEntityFrameworkCoreTestModule>
{

}
