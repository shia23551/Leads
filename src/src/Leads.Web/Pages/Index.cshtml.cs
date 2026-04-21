using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Leads.SharedAccounts;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace Leads.Web.Pages;

public class IndexModel : LeadsPageModel
{
    private readonly IRepository<TenantUserMembership, Guid> _membershipRepository;
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;

    public IndexModel(
        IRepository<TenantUserMembership, Guid> membershipRepository,
        IDataFilter<IMultiTenant> multiTenantFilter)
    {
        _membershipRepository = membershipRepository;
        _multiTenantFilter = multiTenantFilter;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!CurrentUser.IsAuthenticated)
        {
            return Page();
        }

        List<TenantUserMembership> memberships;
        using (_multiTenantFilter.Disable())
        {
            memberships = await _membershipRepository.GetListAsync(x => x.UserId == CurrentUser.GetId() && x.IsActive);
        }
        var count = memberships.Count(x => x.TenantId.HasValue);
        var hasTenantCookie = Request.Cookies.ContainsKey(TenantResolverConsts.DefaultTenantKey);

        if (count > 1 && !hasTenantCookie)
        {
            return RedirectToPage("/SharedAccounts/SelectTenant", new { returnUrl = Url.Page("/Index") });
        }

        return Page();
    }
}
