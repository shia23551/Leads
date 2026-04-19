using System.Collections.Generic;
using System.Threading.Tasks;
using Leads.Companies;
using Leads.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leads.Web.Pages.Companies;

[Authorize(LeadsPermissions.Companies.Default)]
public class IndexModel : LeadsPageModel
{
    private readonly ICompanyAppService _companyAppService;
    private readonly IAuthorizationService _authorizationService;

    [BindProperty(SupportsGet = true)]
    public CompanyListInput Input { get; set; } = new();

    public IReadOnlyList<CompanyDto> Companies { get; private set; } = [];

    public bool CanCreate { get; private set; }

    public bool CanEdit { get; private set; }

    public bool CanDelete { get; private set; }

    public IndexModel(
        ICompanyAppService companyAppService,
        IAuthorizationService authorizationService)
    {
        _companyAppService = companyAppService;
        _authorizationService = authorizationService;
    }

    public async Task OnGetAsync()
    {
        await LoadPermissionsAsync();
        await LoadCompaniesAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(System.Guid id)
    {
        await _companyAppService.DeleteAsync(id);
        return RedirectToPage();
    }

    private async Task LoadPermissionsAsync()
    {
        CanCreate = await _authorizationService.IsGrantedAsync(LeadsPermissions.Companies.Create);
        CanEdit = await _authorizationService.IsGrantedAsync(LeadsPermissions.Companies.Edit);
        CanDelete = await _authorizationService.IsGrantedAsync(LeadsPermissions.Companies.Delete);
    }

    private async Task LoadCompaniesAsync()
    {
        var result = await _companyAppService.GetListAsync(Input);
        Companies = result.Items;
    }
}
