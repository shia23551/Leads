using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Leads.SharedAccounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

using AppIdentityUser = Volo.Abp.Identity.IdentityUser;

namespace Leads.Web.Pages.SharedAccounts;

[AllowAnonymous]
public class AcceptInvitationModel : LeadsPageModel
{
    private readonly IRepository<TenantUserInvitation, Guid> _invitationRepository;
    private readonly IRepository<TenantUserMembership, Guid> _membershipRepository;
    private readonly UserManager<AppIdentityUser> _userManager;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IDataFilter<IMultiTenant> _multiTenantFilter;

    [BindProperty(SupportsGet = true)]
    public string Token { get; set; } = string.Empty;

    public InvitationInfoViewModel? InvitationInfo { get; private set; }

    public bool CanAccept { get; private set; }

    public AcceptInvitationModel(
        IRepository<TenantUserInvitation, Guid> invitationRepository,
        IRepository<TenantUserMembership, Guid> membershipRepository,
        UserManager<AppIdentityUser> userManager,
        IdentityUserManager identityUserManager,
        IDataFilter<IMultiTenant> multiTenantFilter)
    {
        _invitationRepository = invitationRepository;
        _membershipRepository = membershipRepository;
        _userManager = userManager;
        _identityUserManager = identityUserManager;
        _multiTenantFilter = multiTenantFilter;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var invitation = await FindInvitationAsync(Token);
        if (invitation == null)
        {
            return Page();
        }

        InvitationInfo = ToInfo(invitation);
        CanAccept = await CanCurrentUserAcceptAsync(invitation);

        if (!CurrentUser.IsAuthenticated)
        {
            var returnUrl = Url.Page("/SharedAccounts/AcceptInvitation", values: new { token = Token });
            return Redirect($"~/Account/Login?returnUrl={Uri.EscapeDataString(returnUrl ?? "/")}");
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAcceptAsync()
    {
        if (!CurrentUser.IsAuthenticated)
        {
            return Forbid();
        }

        var invitation = await FindInvitationAsync(Token);
        if (invitation == null)
        {
            throw new UserFriendlyException(L["InvitationNotFound"]);
        }

        if (!await CanCurrentUserAcceptAsync(invitation))
        {
            throw new UserFriendlyException(L["InvitationEmailMismatch"]);
        }

        var currentUser = await _userManager.FindByIdAsync(CurrentUser.GetId().ToString());
        if (currentUser == null)
        {
            throw new UserFriendlyException(L["UserNotFound"]);
        }

        TenantUserMembership? existing;
        using (_multiTenantFilter.Disable())
        {
            var membershipQueryable = await _membershipRepository.GetQueryableAsync();
            existing = membershipQueryable.FirstOrDefault(x => x.TenantId == invitation.TenantId && x.UserId == currentUser.Id);
        }
        if (existing == null)
        {
            var membership = new TenantUserMembership(Guid.NewGuid(), invitation.TenantId!.Value, currentUser.Id, true);
            await _membershipRepository.InsertAsync(membership, autoSave: true);
        }
        else
        {
            existing.Activate();
            await _membershipRepository.UpdateAsync(existing, autoSave: true);
        }

        invitation.Accept(DateTime.UtcNow);
        await _invitationRepository.UpdateAsync(invitation, autoSave: true);

        var roleNames = ParseRoleNames(invitation.RoleNames);
        if (roleNames.Count > 0)
        {
            using (CurrentTenant.Change(invitation.TenantId))
            {
                foreach (var roleName in roleNames)
                {
                    await _identityUserManager.AddToRoleAsync(currentUser, roleName);
                }
            }
        }

        return RedirectToPage("/SharedAccounts/SelectTenant", new { tenantId = invitation.TenantId });
    }

    private async Task<TenantUserInvitation?> FindInvitationAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        List<TenantUserInvitation> list;
        using (_multiTenantFilter.Disable())
        {
            list = await _invitationRepository.GetListAsync(x => x.Token == token);
        }
        var invitation = list.FirstOrDefault();
        if (invitation == null || !invitation.IsAvailable(DateTime.UtcNow))
        {
            return null;
        }

        return invitation;
    }

    private async Task<bool> CanCurrentUserAcceptAsync(TenantUserInvitation invitation)
    {
        if (!CurrentUser.IsAuthenticated)
        {
            return false;
        }

        var user = await _userManager.FindByIdAsync(CurrentUser.GetId().ToString());
        if (user == null)
        {
            return false;
        }

        return string.Equals(invitation.Email, user.Email, StringComparison.OrdinalIgnoreCase);
    }

    private static InvitationInfoViewModel ToInfo(TenantUserInvitation invitation)
    {
        return new InvitationInfoViewModel
        {
            Email = invitation.Email,
            RoleNames = invitation.RoleNames,
            ExpireTime = invitation.ExpireTime
        };
    }

    private static List<string> ParseRoleNames(string? roleNames)
    {
        return (roleNames ?? string.Empty)
            .Split([','], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !x.IsNullOrWhiteSpace())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public class InvitationInfoViewModel
    {
        public string Email { get; set; } = string.Empty;

        public string? RoleNames { get; set; }

        public DateTime ExpireTime { get; set; }
    }
}
