using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Leads.Permissions;
using Leads.SharedAccounts;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Emailing;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

using AppIdentityUser = Volo.Abp.Identity.IdentityUser;

namespace Leads.Web.Pages.SharedAccounts;

[Authorize(LeadsPermissions.SharedAccounts.ManageMembers)]
public class MembersModel : LeadsPageModel
{
    private readonly IRepository<TenantUserMembership, Guid> _membershipRepository;
    private readonly IRepository<TenantUserInvitation, Guid> _invitationRepository;
    private readonly UserManager<AppIdentityUser> _userManager;
    private readonly IEmailSender _emailSender;
    private readonly IGuidGenerator _guidGenerator;
    private readonly ICurrentTenant _currentTenant;

    [BindProperty]
    public InviteUserInputModel InviteInput { get; set; } = new();

    public IReadOnlyList<MemberViewModel> Members { get; private set; } = [];

    public IReadOnlyList<InvitationViewModel> Invitations { get; private set; } = [];

    public string? InviteLinkPreview { get; private set; }

    [TempData]
    public string? InvitationMessage { get; set; }

    public MembersModel(
        IRepository<TenantUserMembership, Guid> membershipRepository,
        IRepository<TenantUserInvitation, Guid> invitationRepository,
        UserManager<AppIdentityUser> userManager,
        IEmailSender emailSender,
        IGuidGenerator guidGenerator,
        ICurrentTenant currentTenant)
    {
        _membershipRepository = membershipRepository;
        _invitationRepository = invitationRepository;
        _userManager = userManager;
        _emailSender = emailSender;
        _guidGenerator = guidGenerator;
        _currentTenant = currentTenant;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        if (!_currentTenant.Id.HasValue)
        {
            return Forbid();
        }

        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostInviteAsync()
    {
        if (!_currentTenant.Id.HasValue)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            await LoadAsync();
            return Page();
        }

        var tenantId = _currentTenant.Id.Value;
        var normalizedEmail = InviteInput.Email.Trim();
        var roleNames = NormalizeRoleNames(InviteInput.RoleNames);
        var token = Guid.NewGuid().ToString("N");

        var invitation = new TenantUserInvitation(
            _guidGenerator.Create(),
            tenantId,
            normalizedEmail,
            token,
            DateTime.UtcNow.AddDays(7),
            roleNames.Count == 0 ? null : string.Join(",", roleNames));

        await _invitationRepository.InsertAsync(invitation, autoSave: true);

        var path = Url.Page("/SharedAccounts/AcceptInvitation", values: new { token }) ?? string.Empty;
        InviteLinkPreview = $"{Request.Scheme}://{Request.Host}{path}";

        try
        {
            await _emailSender.SendAsync(
                normalizedEmail,
                L["InvitationEmailSubject"],
                L["InvitationEmailBody", InviteLinkPreview],
                isBodyHtml: false);

            InvitationMessage = L["InvitationEmailSent"].Value;
        }
        catch
        {
            InvitationMessage = L["InvitationEmailSendFailed"].Value;
        }

        await LoadAsync();
        return Page();
    }

    public async Task<IActionResult> OnPostRevokeInvitationAsync(Guid id)
    {
        if (!_currentTenant.Id.HasValue)
        {
            return Forbid();
        }

        var invitation = await _invitationRepository.GetAsync(id);
        invitation.Revoke();
        await _invitationRepository.UpdateAsync(invitation, autoSave: true);

        return RedirectToPage("/SharedAccounts/Members");
    }

    public async Task<IActionResult> OnPostRemoveMemberAsync(Guid memberUserId)
    {
        if (!_currentTenant.Id.HasValue)
        {
            return Forbid();
        }

        var tenantId = _currentTenant.Id.Value;
        var queryable = await _membershipRepository.GetQueryableAsync();
        var membership = queryable.FirstOrDefault(x => x.TenantId == tenantId && x.UserId == memberUserId);
        if (membership != null)
        {
            membership.Deactivate();
            await _membershipRepository.UpdateAsync(membership, autoSave: true);
        }

        return RedirectToPage("/SharedAccounts/Members");
    }

    private async Task LoadAsync()
    {
        var tenantId = _currentTenant.Id!.Value;

        var memberships = await _membershipRepository.GetListAsync(x => x.TenantId == tenantId && x.IsActive);
        var userIds = memberships.Select(x => x.UserId).Distinct().ToList();

        var users = new List<AppIdentityUser>();
        foreach (var userId in userIds)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user != null)
            {
                users.Add(user);
            }
        }

        Members = users
            .OrderBy(x => x.UserName)
            .Select(x => new MemberViewModel
            {
                UserId = x.Id,
                UserName = x.UserName,
                Name = string.Join(" ", new[] { x.Name, x.Surname }.Where(y => !y.IsNullOrWhiteSpace())).Trim(),
                Email = x.Email
            })
            .ToList();

        var invitations = await _invitationRepository.GetListAsync(x => x.TenantId == tenantId);
        Invitations = invitations
            .OrderByDescending(x => x.CreationTime)
            .Take(50)
            .Select(x => new InvitationViewModel
            {
                Id = x.Id,
                Email = x.Email,
                RoleNames = x.RoleNames,
                ExpireTime = x.ExpireTime,
                IsRevoked = x.IsRevoked,
                AcceptedTime = x.AcceptedTime,
                Token = x.Token
            })
            .ToList();
    }

    private static List<string> NormalizeRoleNames(string? roleNames)
    {
        return (roleNames ?? string.Empty)
            .Split([',', '\r', '\n'], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !x.IsNullOrWhiteSpace())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public class InviteUserInputModel
    {
        [Required]
        [EmailAddress]
        [StringLength(TenantUserInvitation.MaxEmailLength)]
        public string Email { get; set; } = string.Empty;

        [StringLength(1024)]
        public string? RoleNames { get; set; }
    }

    public class MemberViewModel
    {
        public Guid UserId { get; set; }

        public string? UserName { get; set; }

        public string? Name { get; set; }

        public string? Email { get; set; }
    }

    public class InvitationViewModel
    {
        public Guid Id { get; set; }

        public string Email { get; set; } = string.Empty;

        public string? RoleNames { get; set; }

        public DateTime ExpireTime { get; set; }

        public bool IsRevoked { get; set; }

        public DateTime? AcceptedTime { get; set; }

        public string Token { get; set; } = string.Empty;
    }
}
