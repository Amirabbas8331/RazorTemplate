using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorTemplate.Model;

namespace RazorTemplate.Pages;

public class RegisterModel : PageModel
{
    private readonly IHttpClientFactory _clientFactory;

    public RegisterModel(IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    [BindProperty]
    public RegisterRequest? Input { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        var client = _clientFactory.CreateClient();
        var response = await client.PostAsJsonAsync("https://localhost:7141/users/register", Input);

        if (response.IsSuccessStatusCode)
        {
            var createdUser = await response.Content.ReadFromJsonAsync<User>();

            return RedirectToPage("/Index");
        }

        var error = await response.Content.ReadAsStringAsync();
        ModelState.AddModelError(string.Empty, error);
        return Page();
    }
}
