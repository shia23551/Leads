using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Leads.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Guids;
using Volo.Abp.OpenIddict.Applications;

namespace Leads.Web.Pages.OpenIddict.Applications;

[Authorize(LeadsPermissions.OpenIddictApplications.Default)]
public class IndexModel : LeadsPageModel
{
    private readonly IOpenIddictApplicationRepository _repository;
    private readonly IAuthorizationService _authorizationService;
    private readonly IGuidGenerator _guidGenerator;

    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? EditId { get; set; }

    [BindProperty]
    public EditApplicationViewModel CreateInput { get; set; } = EditApplicationViewModel.CreateDefault();

    [BindProperty]
    public EditApplicationViewModel EditInput { get; set; } = EditApplicationViewModel.CreateDefault();

    public IReadOnlyList<ApplicationItemViewModel> Applications { get; private set; } = [];

    public bool CanCreate { get; private set; }

    public bool CanEdit { get; private set; }

    public bool CanDelete { get; private set; }

    public IndexModel(
        IOpenIddictApplicationRepository repository,
        IAuthorizationService authorizationService,
        IGuidGenerator guidGenerator)
    {
        _repository = repository;
        _authorizationService = authorizationService;
        _guidGenerator = guidGenerator;
    }

    public async Task OnGetAsync()
    {
        await LoadPermissionsAsync();
        await LoadListAsync();
        await LoadEditInputAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        if (!await _authorizationService.IsGrantedAsync(LeadsPermissions.OpenIddictApplications.Create))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            await LoadPermissionsAsync();
            await LoadListAsync();
            return Page();
        }

        var clientId = CreateInput.ClientId.Trim();
        if (await _repository.FindByClientIdAsync(clientId) != null)
        {
            throw new UserFriendlyException(L["OpenIddictClientIdExists", clientId]);
        }

        var entity = new OpenIddictApplication(_guidGenerator.Create())
        {
            ClientId = clientId,
            DisplayName = CreateInput.DisplayName?.Trim(),
            ClientType = CreateInput.ClientType,
            ConsentType = CreateInput.ConsentType,
            ApplicationType = CreateInput.ApplicationType,
            ClientSecret = CreateInput.ClientSecret?.Trim(),
            RedirectUris = ToJsonArray(CreateInput.RedirectUris),
            PostLogoutRedirectUris = ToJsonArray(CreateInput.PostLogoutRedirectUris),
            Permissions = ToJsonArray(CreateInput.Permissions)
        };

        await _repository.InsertAsync(entity, autoSave: true);
        return RedirectToPage("/OpenIddict/Applications/Index");
    }

    public async Task<IActionResult> OnPostUpdateAsync(Guid id)
    {
        if (!await _authorizationService.IsGrantedAsync(LeadsPermissions.OpenIddictApplications.Edit))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            await LoadPermissionsAsync();
            await LoadListAsync();
            EditId = id;
            return Page();
        }

        var entity = await _repository.GetAsync(id);

        var clientId = EditInput.ClientId.Trim();
        var existing = await _repository.FindByClientIdAsync(clientId);
        if (existing != null && existing.Id != id)
        {
            throw new UserFriendlyException(L["OpenIddictClientIdExists", clientId]);
        }

        entity.ClientId = clientId;
        entity.DisplayName = EditInput.DisplayName?.Trim();
        entity.ClientType = EditInput.ClientType;
        entity.ConsentType = EditInput.ConsentType;
        entity.ApplicationType = EditInput.ApplicationType;
        entity.ClientSecret = EditInput.ClientSecret?.Trim();
        entity.RedirectUris = ToJsonArray(EditInput.RedirectUris);
        entity.PostLogoutRedirectUris = ToJsonArray(EditInput.PostLogoutRedirectUris);
        entity.Permissions = ToJsonArray(EditInput.Permissions);

        await _repository.UpdateAsync(entity, autoSave: true);
        return RedirectToPage("/OpenIddict/Applications/Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        if (!await _authorizationService.IsGrantedAsync(LeadsPermissions.OpenIddictApplications.Delete))
        {
            return Forbid();
        }

        await _repository.DeleteAsync(id, autoSave: true);
        return RedirectToPage("/OpenIddict/Applications/Index");
    }

    private async Task LoadPermissionsAsync()
    {
        CanCreate = await _authorizationService.IsGrantedAsync(LeadsPermissions.OpenIddictApplications.Create);
        CanEdit = await _authorizationService.IsGrantedAsync(LeadsPermissions.OpenIddictApplications.Edit);
        CanDelete = await _authorizationService.IsGrantedAsync(LeadsPermissions.OpenIddictApplications.Delete);
    }

    private async Task LoadListAsync()
    {
        var items = await _repository.GetListAsync(
            sorting: nameof(OpenIddictApplication.ClientId),
            skipCount: 0,
            maxResultCount: 200,
            filter: Filter?.Trim());

        Applications = items.Select(x => new ApplicationItemViewModel
        {
            Id = x.Id,
            ClientId = x.ClientId,
            DisplayName = x.DisplayName,
            ClientType = x.ClientType,
            ConsentType = x.ConsentType,
            ApplicationType = x.ApplicationType
        }).ToList();
    }

    private async Task LoadEditInputAsync()
    {
        if (!EditId.HasValue)
        {
            return;
        }

        var entity = await _repository.GetAsync(EditId.Value);
        EditInput = new EditApplicationViewModel
        {
            ClientId = entity.ClientId ?? string.Empty,
            DisplayName = entity.DisplayName,
            ClientType = entity.ClientType,
            ConsentType = entity.ConsentType,
            ApplicationType = entity.ApplicationType,
            ClientSecret = entity.ClientSecret,
            RedirectUris = FromJsonArray(entity.RedirectUris),
            PostLogoutRedirectUris = FromJsonArray(entity.PostLogoutRedirectUris),
            Permissions = FromJsonArray(entity.Permissions)
        };
    }

    private static string? ToJsonArray(string? text)
    {
        var values = (text ?? string.Empty)
            .Split(['\r', '\n', ','], StringSplitOptions.RemoveEmptyEntries)
            .Select(x => x.Trim())
            .Where(x => !x.IsNullOrWhiteSpace())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        return values.Length == 0 ? null : JsonSerializer.Serialize(values);
    }

    private static string? FromJsonArray(string? json)
    {
        if (json.IsNullOrWhiteSpace())
        {
            return null;
        }

        try
        {
            var values = JsonSerializer.Deserialize<string[]>(json!);
            return values == null ? null : string.Join(Environment.NewLine, values);
        }
        catch
        {
            return json;
        }
    }

    public class ApplicationItemViewModel
    {
        public Guid Id { get; set; }

        public string? ClientId { get; set; }

        public string? DisplayName { get; set; }

        public string? ClientType { get; set; }

        public string? ConsentType { get; set; }

        public string? ApplicationType { get; set; }
    }

    public class EditApplicationViewModel
    {
        [Required]
        [StringLength(100)]
        public string ClientId { get; set; } = string.Empty;

        [StringLength(200)]
        public string? DisplayName { get; set; }

        [StringLength(50)]
        public string? ClientType { get; set; }

        [StringLength(50)]
        public string? ConsentType { get; set; }

        [StringLength(50)]
        public string? ApplicationType { get; set; }

        [StringLength(200)]
        public string? ClientSecret { get; set; }

        public string? RedirectUris { get; set; }

        public string? PostLogoutRedirectUris { get; set; }

        public string? Permissions { get; set; }

        public static EditApplicationViewModel CreateDefault()
        {
            return new EditApplicationViewModel
            {
                ClientType = "confidential",
                ConsentType = "explicit",
                ApplicationType = "web"
            };
        }
    }
}
