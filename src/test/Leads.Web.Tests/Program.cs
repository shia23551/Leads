using Microsoft.AspNetCore.Builder;
using Leads;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("Leads.Web.csproj"); 
await builder.RunAbpModuleAsync<LeadsWebTestModule>(applicationName: "Leads.Web");

public partial class Program
{
}
