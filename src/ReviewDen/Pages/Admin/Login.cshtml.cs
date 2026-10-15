using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ReviewDen.Pages.Admin;

public class LoginModel(IConfiguration config) : PageModel
{
    public string? Error { get; private set; }

    public async Task<IActionResult> OnPostAsync(string password)
    {
        var expected = config["AdminPassword"] ?? "";
        if (expected.Length < 8 || !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(password ?? ""), Encoding.UTF8.GetBytes(expected)))
        {
            Error = expected.Length < 8 ? "AdminPassword is not set (min 8 chars)." : "Wrong password.";
            return Page();
        }
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, "admin")], CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(identity));
        return Redirect("/admin");
    }
}
