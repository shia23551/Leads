using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using Volo.Abp.Identity;

namespace Leads.Web.Pages.Users;

[Authorize(IdentityPermissions.Users.Default)]
public class IndexModel : LeadsPageModel
{
    private readonly IIdentityUserAppService _identityUserAppService;
    private readonly IAuthorizationService _authorizationService;

    [BindProperty(SupportsGet = true)]
    public string? Filter { get; set; }

    [BindProperty]
    public CreateUserViewModel CreateInput { get; set; } = new();

    public IReadOnlyList<IdentityUserDto> Users { get; private set; } = [];

    public bool CanCreate { get; private set; }

    public IndexModel(
        IIdentityUserAppService identityUserAppService,
        IAuthorizationService authorizationService)
    {
        _identityUserAppService = identityUserAppService;
        _authorizationService = authorizationService;
    }

    public async Task OnGetAsync()
    {
        await LoadPermissionsAsync();
        await LoadUsersAsync();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        await LoadPermissionsAsync();

        if (!CanCreate)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            await LoadUsersAsync();
            return Page();
        }

        if (CreateInput.Password != CreateInput.ConfirmPassword)
        {
            throw new UserFriendlyException(L["PasswordNotMatch"]);
        }

        var input = new IdentityUserCreateDto
        {
            UserName = CreateInput.UserName.Trim(),
            Name = CreateInput.Name?.Trim(),
            Surname = CreateInput.Surname?.Trim(),
            Email = CreateInput.Email.Trim(),
            PhoneNumber = CreateInput.PhoneNumber?.Trim(),
            IsActive = true,
            LockoutEnabled = true,
            Password = CreateInput.Password,
            RoleNames = []
        };

        await _identityUserAppService.CreateAsync(input);

        return RedirectToPage("/Users/Index");
    }

    private async Task LoadPermissionsAsync()
    {
        CanCreate = await _authorizationService.IsGrantedAsync(IdentityPermissions.Users.Create);
    }

    private async Task LoadUsersAsync()
    {
        var result = await _identityUserAppService.GetListAsync(new GetIdentityUsersInput
        {
            Filter = Filter?.Trim(),
            Sorting = nameof(IdentityUserDto.UserName),
            SkipCount = 0,
            MaxResultCount = 100
        });

        Users = result.Items;
    }

    public class CreateUserViewModel
    {
        [Required]
        [StringLength(256)]
        public string UserName { get; set; } = string.Empty;

        [StringLength(64)]
        public string? Name { get; set; }

        [StringLength(64)]
        public string? Surname { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public string Email { get; set; } = string.Empty;

        [StringLength(16)]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(128)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [StringLength(128)]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
