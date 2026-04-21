using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace Leads.Web.Pages.Passkeys;

[AllowAnonymous]
public class LoginModel : LeadsPageModel
{
    private readonly UserManager<Volo.Abp.Identity.IdentityUser> _userManager;
    private readonly SignInManager<Volo.Abp.Identity.IdentityUser> _signInManager;

    [BindProperty(SupportsGet = true)]
    public string? ReturnUrl { get; set; }

    [BindProperty]
    public InputViewModel Input { get; set; } = new();

    public LoginModel(
        UserManager<Volo.Abp.Identity.IdentityUser> userManager,
        SignInManager<Volo.Abp.Identity.IdentityUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostRequestOptionsAsync([FromBody] PasskeyOptionsRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserNameOrEmail))
        {
            return BadRequest(new { message = L["UserNameRequired"] });
        }

        var user = await _userManager.FindByNameAsync(request.UserNameOrEmail.Trim())
                   ?? await _userManager.FindByEmailAsync(request.UserNameOrEmail.Trim());

        if (user == null)
        {
            return BadRequest(new { message = L["InvalidUserNameOrPassword"] });
        }

        var requestOptions = await _signInManager.MakePasskeyRequestOptionsAsync(user);
        return new JsonResult(new { optionsJson = requestOptions });
    }

    public async Task<IActionResult> OnPostSignInAsync([FromBody] PasskeySignInRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.CredentialJson))
        {
            return BadRequest(new { message = L["PasskeyCredentialRequired"] });
        }

        var signInResult = await _signInManager.PasskeySignInAsync(request.CredentialJson);
        if (!signInResult.Succeeded)
        {
            return BadRequest(new { message = L["PasskeySignInFailed"] });
        }

        return new JsonResult(new
        {
            succeeded = true,
            redirectUrl = string.IsNullOrWhiteSpace(ReturnUrl) ? "~/" : ReturnUrl
        });
    }

    public class InputViewModel
    {
        [Required]
        [StringLength(256)]
        public string UserNameOrEmail { get; set; } = string.Empty;
    }

    public class PasskeyOptionsRequest
    {
        public string UserNameOrEmail { get; set; } = string.Empty;
    }

    public class PasskeySignInRequest
    {
        public string CredentialJson { get; set; } = string.Empty;
    }
}
