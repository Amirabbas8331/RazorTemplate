using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTemplate.Model;

public class LoginModel : PageModel
{
    private readonly IHttpClientFactory _clientFactory;

    public LoginModel(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    [BindProperty]
    public LoginRequest? Input { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var client = _clientFactory.CreateClient();
        var response = await client.PostAsJsonAsync("https://localhost:7141/users/login", Input);

        if (response.IsSuccessStatusCode)
        {
            var token = await response.Content.ReadFromJsonAsync<TokenResponse>();

            // Store token in cookie (HttpOnly for security)
            Response.Cookies.Append("AuthToken", token!.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddSeconds(token.ExpiresIn)
            });

            return RedirectToPage("/Index");
        }

        ModelState.AddModelError(string.Empty, "Invalid username or password");
        return Page();
    }
}



