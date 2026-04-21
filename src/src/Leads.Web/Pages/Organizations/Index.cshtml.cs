using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Data;
using Volo.Abp.Guids;
using Volo.Abp.Identity;

namespace Leads.Web.Pages.Organizations;

[Authorize]
public class IndexModel : LeadsPageModel
{
    private readonly IOrganizationUnitRepository _organizationUnitRepository;
    private readonly OrganizationUnitManager _organizationUnitManager;
    private readonly IIdentityUserRepository _identityUserRepository;
    private readonly IdentityUserManager _identityUserManager;
    private readonly IGuidGenerator _guidGenerator;

    [BindProperty(SupportsGet = true)]
    public Guid? SelectedOrganizationUnitId { get; set; }

    public IReadOnlyList<OrganizationUnitItemViewModel> OrganizationUnits { get; private set; } = [];

    public IReadOnlyList<MemberItemViewModel> Members { get; private set; } = [];

    public OrganizationUnitItemViewModel? SelectedOrganizationUnit { get; private set; }

    public bool IsAdmin { get; private set; }

    public bool CanManageSelected { get; private set; }

    public IndexModel(
        IOrganizationUnitRepository organizationUnitRepository,
        OrganizationUnitManager organizationUnitManager,
        IIdentityUserRepository identityUserRepository,
        IdentityUserManager identityUserManager,
        IGuidGenerator guidGenerator)
    {
        _organizationUnitRepository = organizationUnitRepository;
        _organizationUnitManager = organizationUnitManager;
        _identityUserRepository = identityUserRepository;
        _identityUserManager = identityUserManager;
        _guidGenerator = guidGenerator;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        await LoadDataAsync();

        if (!IsAdmin && OrganizationUnits.Count == 0)
        {
            return Forbid();
        }

        return Page();
    }

    public async Task<IActionResult> OnPostCreateOrganizationUnitAsync(string displayName, Guid? parentId, Guid? supervisorUserId)
    {
        if (displayName.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException(L["OrganizationNameRequired"]);
        }

        var allUnits = await GetAllOrganizationUnitsAsync();
        var accessContext = BuildAccessContext(allUnits);

        if (!parentId.HasValue && !accessContext.IsAdmin)
        {
            throw new AbpAuthorizationException();
        }

        if (parentId.HasValue)
        {
            var parent = allUnits.FirstOrDefault(x => x.Id == parentId.Value);
            if (parent == null || !CanManageOrganizationUnit(parent, accessContext))
            {
                throw new AbpAuthorizationException();
            }
        }

        var organizationUnit = new OrganizationUnit(_guidGenerator.Create(), displayName.Trim(), parentId, CurrentTenant.Id);
        SetSupervisor(organizationUnit, supervisorUserId);
        await _organizationUnitManager.CreateAsync(organizationUnit);

        return RedirectToPage("/Organizations/Index", new { selectedOrganizationUnitId = organizationUnit.Id });
    }

    public async Task<IActionResult> OnPostUpdateOrganizationUnitAsync(Guid id, string displayName, Guid? supervisorUserId)
    {
        if (displayName.IsNullOrWhiteSpace())
        {
            throw new UserFriendlyException(L["OrganizationNameRequired"]);
        }

        var allUnits = await GetAllOrganizationUnitsAsync();
        var accessContext = BuildAccessContext(allUnits);
        var organizationUnit = allUnits.FirstOrDefault(x => x.Id == id);

        if (organizationUnit == null || !CanManageOrganizationUnit(organizationUnit, accessContext))
        {
            throw new AbpAuthorizationException();
        }

        organizationUnit.DisplayName = displayName.Trim();
        SetSupervisor(organizationUnit, supervisorUserId);
        await _organizationUnitManager.UpdateAsync(organizationUnit);

        return RedirectToPage("/Organizations/Index", new { selectedOrganizationUnitId = id });
    }

    public async Task<IActionResult> OnPostDeleteOrganizationUnitAsync(Guid id)
    {
        var allUnits = await GetAllOrganizationUnitsAsync();
        var accessContext = BuildAccessContext(allUnits);
        var organizationUnit = allUnits.FirstOrDefault(x => x.Id == id);

        if (organizationUnit == null || !CanManageOrganizationUnit(organizationUnit, accessContext))
        {
            throw new AbpAuthorizationException();
        }

        await _organizationUnitManager.DeleteAsync(id);

        return RedirectToPage("/Organizations/Index");
    }

    public async Task<IActionResult> OnPostAddMemberAsync(Guid organizationUnitId, Guid userId)
    {
        var allUnits = await GetAllOrganizationUnitsAsync();
        var accessContext = BuildAccessContext(allUnits);
        var organizationUnit = allUnits.FirstOrDefault(x => x.Id == organizationUnitId);

        if (organizationUnit == null || !CanManageOrganizationUnit(organizationUnit, accessContext))
        {
            throw new AbpAuthorizationException();
        }

        await _identityUserManager.AddToOrganizationUnitAsync(userId, organizationUnitId);

        return RedirectToPage("/Organizations/Index", new { selectedOrganizationUnitId = organizationUnitId });
    }

    public async Task<IActionResult> OnPostRemoveMemberAsync(Guid organizationUnitId, Guid userId)
    {
        var allUnits = await GetAllOrganizationUnitsAsync();
        var accessContext = BuildAccessContext(allUnits);
        var organizationUnit = allUnits.FirstOrDefault(x => x.Id == organizationUnitId);

        if (organizationUnit == null || !CanManageOrganizationUnit(organizationUnit, accessContext))
        {
            throw new AbpAuthorizationException();
        }

        await _identityUserManager.RemoveFromOrganizationUnitAsync(userId, organizationUnitId);

        return RedirectToPage("/Organizations/Index", new { selectedOrganizationUnitId = organizationUnitId });
    }

    public async Task<IActionResult> OnGetSearchUsersAsync(Guid? organizationUnitId, string? filter)
    {
        var allUnits = await GetAllOrganizationUnitsAsync();
        var accessContext = BuildAccessContext(allUnits);

        if (organizationUnitId.HasValue)
        {
            var organizationUnit = allUnits.FirstOrDefault(x => x.Id == organizationUnitId.Value);
            if (organizationUnit == null || !CanManageOrganizationUnit(organizationUnit, accessContext))
            {
                return Forbid();
            }
        }
        else if (!accessContext.IsAdmin)
        {
            return Forbid();
        }

        var users = await _identityUserRepository.GetListAsync(
            sorting: nameof(IdentityUser.UserName),
            maxResultCount: 20,
            skipCount: 0,
            filter: filter?.Trim(),
            includeDetails: true,
            roleId: null,
            organizationUnitId: null,
            id: null,
            userName: null,
            phoneNumber: null,
            emailAddress: null,
            name: null,
            surname: null,
            isLockedOut: null,
            notActive: null,
            emailConfirmed: null,
            isExternal: null,
            maxCreationTime: null,
            minCreationTime: null,
            maxModifitionTime: null,
            minModifitionTime: null);

        var result = users
            .Select(x => new UserLookupItemViewModel
            {
                Id = x.Id,
                UserName = x.UserName,
                DisplayName = GetUserDisplayName(x),
                Email = x.Email
            })
            .ToList();

        return new JsonResult(result);
    }

    private async Task LoadDataAsync()
    {
        var allUnits = await GetAllOrganizationUnitsAsync();
        var accessContext = BuildAccessContext(allUnits);

        IsAdmin = accessContext.IsAdmin;

        var visibleUnits = allUnits
            .Where(x => CanManageOrganizationUnit(x, accessContext))
            .OrderBy(x => x.Code)
            .ToList();

        var supervisorMap = await BuildSupervisorMapAsync(visibleUnits);

        OrganizationUnits = visibleUnits
            .Select(x => new OrganizationUnitItemViewModel
            {
                Id = x.Id,
                ParentId = x.ParentId,
                DisplayName = x.DisplayName,
                Code = x.Code,
                Level = GetLevel(x.Code),
                SupervisorUserId = GetSupervisorUserId(x),
                SupervisorDisplayName = GetSupervisorDisplayName(x, supervisorMap),
                CanManage = CanManageOrganizationUnit(x, accessContext)
            })
            .ToList();

        if (SelectedOrganizationUnitId.HasValue && OrganizationUnits.All(x => x.Id != SelectedOrganizationUnitId.Value))
        {
            SelectedOrganizationUnitId = null;
        }

        if (!SelectedOrganizationUnitId.HasValue && OrganizationUnits.Count > 0)
        {
            SelectedOrganizationUnitId = OrganizationUnits[0].Id;
        }

        SelectedOrganizationUnit = SelectedOrganizationUnitId.HasValue
            ? OrganizationUnits.FirstOrDefault(x => x.Id == SelectedOrganizationUnitId.Value)
            : null;

        CanManageSelected = SelectedOrganizationUnit?.CanManage == true;

        if (!SelectedOrganizationUnitId.HasValue)
        {
            Members = [];
            return;
        }

        var selectedOu = allUnits.First(x => x.Id == SelectedOrganizationUnitId.Value);
        var members = await _organizationUnitRepository.GetMembersAsync(
            selectedOu,
            sorting: nameof(IdentityUser.UserName),
            maxResultCount: 200,
            skipCount: 0,
            filter: null,
            includeChildren: false,
            includeDetails: true);

        Members = members
            .Select(x => new MemberItemViewModel
            {
                Id = x.Id,
                UserName = x.UserName,
                DisplayName = GetUserDisplayName(x),
                Email = x.Email
            })
            .OrderBy(x => x.UserName)
            .ToList();
    }

    private async Task<List<OrganizationUnit>> GetAllOrganizationUnitsAsync()
    {
        return (await _organizationUnitRepository.GetListAsync(
            sorting: nameof(OrganizationUnit.Code),
            maxResultCount: 1000,
            skipCount: 0,
            includeDetails: true)).ToList();
    }

    private AccessContext BuildAccessContext(IReadOnlyCollection<OrganizationUnit> allUnits)
    {
        var isAdmin = CurrentUser.Roles.Any(x => string.Equals(x, "admin", StringComparison.OrdinalIgnoreCase));

        if (isAdmin)
        {
            return new AccessContext(true, []);
        }

        var supervisorRoots = allUnits
            .Where(x => GetSupervisorUserId(x) == CurrentUser.Id)
            .ToList();

        return new AccessContext(false, supervisorRoots);
    }

    private static bool CanManageOrganizationUnit(OrganizationUnit unit, AccessContext accessContext)
    {
        if (accessContext.IsAdmin)
        {
            return true;
        }

        return accessContext.SupervisorRoots.Any(root => IsDescendantOrSame(unit.Code, root.Code));
    }

    private async Task<Dictionary<Guid, string>> BuildSupervisorMapAsync(IReadOnlyCollection<OrganizationUnit> organizationUnits)
    {
        var userIds = organizationUnits
            .Select(GetSupervisorUserId)
            .Where(x => x.HasValue)
            .Select(x => x!.Value)
            .Distinct()
            .ToList();

        if (userIds.Count == 0)
        {
            return new Dictionary<Guid, string>();
        }

        var users = await _identityUserRepository.GetListByIdsAsync(userIds, includeDetails: true);
        return users.ToDictionary(x => x.Id, GetUserDisplayName);
    }

    private static bool IsDescendantOrSame(string code, string rootCode)
    {
        return string.Equals(code, rootCode, StringComparison.Ordinal) ||
               code.StartsWith(rootCode + ".", StringComparison.Ordinal);
    }

    private static int GetLevel(string code)
    {
        return string.IsNullOrWhiteSpace(code) ? 0 : code.Split('.').Length - 1;
    }

    private static Guid? GetSupervisorUserId(OrganizationUnit organizationUnit)
    {
        var supervisorUserId = organizationUnit.GetProperty<Guid?>("SupervisorUserId");
        if (supervisorUserId.HasValue)
        {
            return supervisorUserId;
        }

        var legacySupervisor = organizationUnit.GetProperty<string>("Supervisor");
        return Guid.TryParse(legacySupervisor, out var parsed) ? parsed : null;
    }

    private static string? GetSupervisorDisplayName(OrganizationUnit organizationUnit, IReadOnlyDictionary<Guid, string> supervisorMap)
    {
        var supervisorUserId = GetSupervisorUserId(organizationUnit);
        if (supervisorUserId.HasValue && supervisorMap.TryGetValue(supervisorUserId.Value, out var displayName))
        {
            return displayName;
        }

        return null;
    }

    private static void SetSupervisor(OrganizationUnit organizationUnit, Guid? supervisorUserId)
    {
        organizationUnit.SetProperty("SupervisorUserId", supervisorUserId);
        organizationUnit.SetProperty("Supervisor", supervisorUserId?.ToString());
    }

    private static string GetUserDisplayName(IdentityUser user)
    {
        var fullName = string.Join(" ", new[] { user.Name, user.Surname }.Where(x => !x.IsNullOrWhiteSpace())).Trim();
        return fullName.IsNullOrWhiteSpace() ? user.UserName : fullName;
    }

    private sealed record AccessContext(bool IsAdmin, IReadOnlyList<OrganizationUnit> SupervisorRoots);

    public class OrganizationUnitItemViewModel
    {
        public Guid Id { get; set; }

        public Guid? ParentId { get; set; }

        public string DisplayName { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public int Level { get; set; }

        public Guid? SupervisorUserId { get; set; }

        public string? SupervisorDisplayName { get; set; }

        public bool CanManage { get; set; }
    }

    public class MemberItemViewModel
    {
        public Guid Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? Email { get; set; }
    }

    public class UserLookupItemViewModel
    {
        public Guid Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        public string DisplayName { get; set; } = string.Empty;

        public string? Email { get; set; }
    }
}
