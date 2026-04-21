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
using Volo.Abp.OpenIddict.Scopes;

namespace Leads.Web.Pages.OpenIddict.Scopes;

[Authorize(LeadsPermissions.OpenIddictScopes.Default)]
public class IndexModel : LeadsPageModel
{
    private readonly IOpenIddictScopeRepository _repository;
    private readonly IAuthorizationService _authorizationService;
    private readonly IGuidGenerator _guidGenerator;

    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? EditId { get; set; }

    [BindProperty]
    public EditScopeViewModel CreateInput { get; set; } = new();

    [BindProperty]
    public EditScopeViewModel EditInput { get; set; } = new();

    public IReadOnlyList<ScopeItemViewModel> Scopes { get; private set; } = [];

    public bool CanCreate { get; private set; }

    public bool CanEdit { get; private set; }

    public bool CanDelete { get; private set; }

    public IndexModel(
        IOpenIddictScopeRepository repository,
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
        if (!await _authorizationService.IsGrantedAsync(LeadsPermissions.OpenIddictScopes.Create))
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            await LoadPermissionsAsync();
            await LoadListAsync();
            return Page();
        }

        var name = CreateInput.Name.Trim();
        if (await _repository.FindByNameAsync(name) != null)
        {
            throw new UserFriendlyException(L["OpenIddictScopeNameExists", name]);
        }

        var entity = new OpenIddictScope(_guidGenerator.Create())
        {
            Name = name,
            DisplayName = CreateInput.DisplayName?.Trim(),
            Description = CreateInput.Description?.Trim(),
            Resources = ToJsonArray(CreateInput.Resources)
        };

        await _repository.InsertAsync(entity, autoSave: true);
        return RedirectToPage("/OpenIddict/Scopes/Index");
    }

    public async Task<IActionResult> OnPostUpdateAsync(Guid id)
    {
        if (!await _authorizationService.IsGrantedAsync(LeadsPermissions.OpenIddictScopes.Edit))
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
        var name = EditInput.Name.Trim();

        var existing = await _repository.FindByNameAsync(name);
        if (existing != null && existing.Id != id)
        {
            throw new UserFriendlyException(L["OpenIddictScopeNameExists", name]);
        }

        entity.Name = name;
        entity.DisplayName = EditInput.DisplayName?.Trim();
        entity.Description = EditInput.Description?.Trim();
        entity.Resources = ToJsonArray(EditInput.Resources);

        await _repository.UpdateAsync(entity, autoSave: true);
        return RedirectToPage("/OpenIddict/Scopes/Index");
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        if (!await _authorizationService.IsGrantedAsync(LeadsPermissions.OpenIddictScopes.Delete))
        {
            return Forbid();
        }

        await _repository.DeleteAsync(id, autoSave: true);
        return RedirectToPage("/OpenIddict/Scopes/Index");
    }

    private async Task LoadPermissionsAsync()
    {
        CanCreate = await _authorizationService.IsGrantedAsync(LeadsPermissions.OpenIddictScopes.Create);
        CanEdit = await _authorizationService.IsGrantedAsync(LeadsPermissions.OpenIddictScopes.Edit);
        CanDelete = await _authorizationService.IsGrantedAsync(LeadsPermissions.OpenIddictScopes.Delete);
    }

    private async Task LoadListAsync()
    {
        var items = await _repository.GetListAsync(
            sorting: nameof(OpenIddictScope.Name),
            skipCount: 0,
            maxResultCount: 200,
            filter: Filter?.Trim());

        Scopes = items.Select(x => new ScopeItemViewModel
        {
            Id = x.Id,
            Name = x.Name,
            DisplayName = x.DisplayName,
            Description = x.Description
        }).ToList();
    }

    private async Task LoadEditInputAsync()
    {
        if (!EditId.HasValue)
        {
            return;
        }

        var entity = await _repository.GetAsync(EditId.Value);
        EditInput = new EditScopeViewModel
        {
            Name = entity.Name ?? string.Empty,
            DisplayName = entity.DisplayName,
            Description = entity.Description,
            Resources = FromJsonArray(entity.Resources)
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

    public class ScopeItemViewModel
    {
        public Guid Id { get; set; }

        public string? Name { get; set; }

        public string? DisplayName { get; set; }

        public string? Description { get; set; }
    }

    public class EditScopeViewModel
    {
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? DisplayName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public string? Resources { get; set; }
    }
}
