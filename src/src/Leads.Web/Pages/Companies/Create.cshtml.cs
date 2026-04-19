using System.Threading.Tasks;
using Leads.Companies;
using Leads.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leads.Web.Pages.Companies;

[Authorize(LeadsPermissions.Companies.Create)]
public class CreateModel : LeadsPageModel
{
    private readonly ICompanyAppService _companyAppService;

    [BindProperty]
    public CreateUpdateCompanyDto Company { get; set; } = new();

    public CreateModel(ICompanyAppService companyAppService)
    {
        _companyAppService = companyAppService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _companyAppService.CreateAsync(Company);
        return RedirectToPage("/Companies/Index");
    }
}
