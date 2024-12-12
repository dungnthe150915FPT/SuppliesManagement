using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SuppliesManagement.Pages.Common
{
    public class SignOutModel : PageModel
    {
        public IActionResult OnGet()
        {
            // Xóa toàn bộ session
            HttpContext.Session.Clear();
            return RedirectToPage("/Common/SignIn");
        }
    }
}
