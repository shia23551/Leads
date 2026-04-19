using System;
using System.Threading.Tasks;
using Leads.Companies;
using Leads.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leads.Web.Pages.Companies;

[Authorize(LeadsPermissions.Companies.Edit)]
public class EditModel : LeadsPageModel
{
    private readonly ICompanyAppService _companyAppService;

    [BindProperty]
    public CreateUpdateCompanyDto Company { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public EditModel(ICompanyAppService companyAppService)
    {
        _companyAppService = companyAppService;
    }

    public async Task OnGetAsync()
    {
        var company = await _companyAppService.GetAsync(Id);
        Company = new CreateUpdateCompanyDto
        {
            Name = company.Name,
            ShortName = company.ShortName,
            TaxId = company.TaxId,
            IndustryCategory = company.IndustryCategory,
            EmployeeCount = company.EmployeeCount,
            CapitalAmount = company.CapitalAmount,
            ContactPerson = company.ContactPerson,
            PhoneNumber = company.PhoneNumber,
            Website = company.Website
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _companyAppService.UpdateAsync(Id, Company);
        return RedirectToPage("/Companies/Index");
    }
}
