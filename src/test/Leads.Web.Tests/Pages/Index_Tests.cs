using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace Leads.Pages;

[Collection(LeadsTestConsts.CollectionDefinitionName)]
public class Index_Tests : LeadsWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
