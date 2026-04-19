using Leads.Samples;
using Xunit;

namespace Leads.EntityFrameworkCore.Applications;

[Collection(LeadsTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<LeadsEntityFrameworkCoreTestModule>
{

}
