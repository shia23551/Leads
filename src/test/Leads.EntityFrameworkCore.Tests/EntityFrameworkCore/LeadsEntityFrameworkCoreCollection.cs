using Xunit;

namespace Leads.EntityFrameworkCore;

[CollectionDefinition(LeadsTestConsts.CollectionDefinitionName)]
public class LeadsEntityFrameworkCoreCollection : ICollectionFixture<LeadsEntityFrameworkCoreFixture>
{

}
