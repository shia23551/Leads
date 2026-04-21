using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Volo.Abp;
using Volo.Abp.Users;

namespace Leads.Web.Pages.Passkeys;

[Authorize]
public class ManageModel : LeadsPageModel
{
    private readonly UserManager<Volo.Abp.Identity.IdentityUser> _userManager;
    private readonly SignInManager<Volo.Abp.Identity.IdentityUser> _signInManager;

    public IReadOnlyList<PasskeyItemViewModel> Passkeys { get; private set; } = [];

    public bool IsPasskeySupported { get; private set; }

    public ManageModel(
        UserManager<Volo.Abp.Identity.IdentityUser> userManager,
        SignInManager<Volo.Abp.Identity.IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task OnGetAsync()
    {
        var user = await GetCurrentUserAsync();
        IsPasskeySupported = _userManager.SupportsUserPasskey;

        if (!IsPasskeySupported)
        {
            Passkeys = [];
            return;
        }

        var passkeys = await _userManager.GetPasskeysAsync(user);
        Passkeys = passkeys.Select(x => new PasskeyItemViewModel
        {
            Name = x.Name ?? string.Empty,
            CreatedAt = x.CreatedAt,
            CredentialId = WebEncoders.Base64UrlEncode(x.CredentialId)
        }).ToList();
    }

    public async Task<IActionResult> OnPostCreateOptionsAsync([FromBody] CreatePasskeyOptionsRequest request)
    {
        if (!_userManager.SupportsUserPasskey)
        {
            return BadRequest(new { message = L["PasskeyNotSupported"] });
        }

        var user = await GetCurrentUserAsync();
        var displayName = string.IsNullOrWhiteSpace(request.DisplayName)
            ? (user.UserName ?? user.Email ?? user.Id.ToString())
            : request.DisplayName.Trim();

        var options = await _signInManager.MakePasskeyCreationOptionsAsync(new PasskeyUserEntity
        {
            Id = user.Id.ToString(),
            Name = user.UserName ?? user.Email ?? user.Id.ToString(),
            DisplayName = displayName
        });

        return new JsonResult(new { optionsJson = options });
    }

    public async Task<IActionResult> OnPostRegisterAsync([FromBody] RegisterPasskeyRequest request)
    {
        if (!_userManager.SupportsUserPasskey)
        {
            return BadRequest(new { message = L["PasskeyNotSupported"] });
        }

        if (string.IsNullOrWhiteSpace(request.CredentialJson))
        {
            return BadRequest(new { message = L["PasskeyCredentialRequired"] });
        }

        var user = await GetCurrentUserAsync();
        var attestation = await _signInManager.PerformPasskeyAttestationAsync(request.CredentialJson);

        if (!attestation.Succeeded)
        {
            return BadRequest(new { message = attestation.Failure?.Message ?? L["PasskeyRegistrationFailed"].Value });
        }

        var passkeyInfo = attestation.Passkey;
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            passkeyInfo.Name = request.Name.Trim();
        }

        var result = await _userManager.AddOrUpdatePasskeyAsync(user, passkeyInfo);
        if (!result.Succeeded)
        {
            return BadRequest(new { message = string.Join("; ", result.Errors.Select(x => x.Description)) });
        }

        return new JsonResult(new { succeeded = true });
    }

    public async Task<IActionResult> OnPostDeleteAsync(string credentialId)
    {
        if (!_userManager.SupportsUserPasskey)
        {
            return BadRequest(new { message = L["PasskeyNotSupported"] });
        }

        if (string.IsNullOrWhiteSpace(credentialId))
        {
            throw new UserFriendlyException(L["PasskeyCredentialRequired"]);
        }

        var user = await GetCurrentUserAsync();
        var credentialIdBytes = WebEncoders.Base64UrlDecode(credentialId);
        var result = await _userManager.RemovePasskeyAsync(user, credentialIdBytes);

        if (!result.Succeeded)
        {
            throw new UserFriendlyException(string.Join("; ", result.Errors.Select(x => x.Description)));
        }

        return RedirectToPage("/Passkeys/Manage");
    }

    private async Task<Volo.Abp.Identity.IdentityUser> GetCurrentUserAsync()
    {
        var id = CurrentUser.GetId();
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null)
        {
            throw new UserFriendlyException(L["UserNotFound"]);
        }

        return user;
    }

    public class CreatePasskeyOptionsRequest
    {
        public string? DisplayName { get; set; }
    }

    public class RegisterPasskeyRequest
    {
        [Required]
        public string CredentialJson { get; set; } = string.Empty;

        public string? Name { get; set; }
    }

    public class PasskeyItemViewModel
    {
        public string Name { get; set; } = string.Empty;

        public DateTimeOffset CreatedAt { get; set; }

        public string CredentialId { get; set; } = string.Empty;
    }
}
