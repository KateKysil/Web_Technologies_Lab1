using DocumentFormat.OpenXml.Wordprocessing;
using LibraryDomain.Model;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Security.Policy;

public class AccountController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AccountController(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet("login-google")]
    public IActionResult LoginWithGoogle()
    {
        var redirectUrl = Url.Action("GoogleResponse", "Account");
        var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
        properties.Parameters["prompt"] = "select_account";
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-response")]
    public async Task<IActionResult> GoogleResponse()
    {
        var info = await HttpContext.AuthenticateAsync(IdentityConstants.ExternalScheme);
        if (!info.Succeeded)
            return RedirectToAction("Login", "Account");

        var email = info.Principal.FindFirst(ClaimTypes.Email)?.Value;
        var name = info.Principal.FindFirst(ClaimTypes.Name)?.Value;

        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            user = new User
            {
                UserName = email,
                Email = email,
                FirstName = name ?? "Unknown",
                LastName = "",
                EmailConfirmed = true 
            };
            var createResult = await _userManager.CreateAsync(user);
            if (!createResult.Succeeded)
            {
                return BadRequest(createResult.Errors);
            }
            await _userManager.AddToRoleAsync(user, "User");
        }
        await _signInManager.SignInAsync(user, isPersistent: false);
        return RedirectToAction("Index", "Home");
    }
}
