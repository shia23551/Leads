using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Leads.SharedAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Users;

namespace Leads.Web.Pages.SharedAccounts;

[Authorize]
public class SelectTenantModel : LeadsPageModel
{
    private readonly IRepository<TenantUserMembership, Guid> _membershipRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;

    [BindProperty(SupportsGet = true)]
    public Guid? TenantId { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    public IReadOnlyList<TenantOptionViewModel> Tenants { get; private set; } = [];

    public SelectTenantModel(
        IRepository<TenantUserMembership, Guid> membershipRepository,
        ITenantRepository tenantRepository,
        IDataFilter<IMultiTenant> multiTenantFilter)
    {
        _membershipRepository = membershipRepository;
        _tenantRepository = tenantRepository;
        _multiTenantFilter = multiTenantFilter;
    }

    public async Task OnGetAsync()
    {
        await LoadTenantsAsync();
    }

    public async Task<IActionResult> OnPostSwitchAsync(Guid tenantId)
    {
        await LoadTenantsAsync();
        if (Tenants.All(x => x.Id != tenantId))
        {
            throw new UserFriendlyException(L["TenantMembershipNotFound"]);
        }

        Response.Cookies.Append(
            TenantResolverConsts.DefaultTenantKey,
            tenantId.ToString(),
            new CookieOptions
            {
                HttpOnly = false,
                IsEssential = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddYears(1)
            });

        if (!string.IsNullOrWhiteSpace(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
        {
            return LocalRedirect(ReturnUrl);
        }

        return Redirect("~/");
    }

    private async Task LoadTenantsAsync()
    {
        var userId = CurrentUser.GetId();
        List<TenantUserMembership> memberships;
        using (_multiTenantFilter.Disable())
        {
            memberships = await _membershipRepository.GetListAsync(x => x.UserId == userId && x.IsActive);
        }
        var tenantIds = memberships
            .Where(x => x.TenantId.HasValue)
            .Select(x => x.TenantId!.Value)
            .Distinct()
            .ToList();

        var tenants = await _tenantRepository.GetListAsync();
        Tenants = tenants
            .Where(x => tenantIds.Contains(x.Id))
            .OrderBy(x => x.Name)
            .Select(x => new TenantOptionViewModel
            {
                Id = x.Id,
                Name = x.Name,
                IsCurrent = TenantId.HasValue ? TenantId.Value == x.Id : CurrentTenant.Id == x.Id
            })
            .ToList();
    }

    public class TenantOptionViewModel
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsCurrent { get; set; }
    }
}
