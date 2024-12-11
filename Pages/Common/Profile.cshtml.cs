using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages.Common
{
    public class ProfileModel : PageModel
    {
        private readonly SuppliesManagementProjectContext _dbContext;

        public ProfileModel(SuppliesManagementProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Account User { get; set; }

        public IActionResult OnGet()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Common/SignIn");
            }
            User = _dbContext.Accounts.Include(u => u.Role).FirstOrDefault(a => a.Id == userId);
            if (User == null)
            {
                return RedirectToPage("/Error");
            }

            return Page();
        }
    }
}
