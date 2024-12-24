using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages
{
    public class DanhSachTaiKhoanModel : PageModel
    {
        private readonly SuppliesManagementProjectContext context;

        public DanhSachTaiKhoanModel(SuppliesManagementProjectContext context)
        {
            this.context = context;
        }

        public List<Account> Accounts { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }
            Accounts = context.Accounts
                .Where(a => a.RoleId != 1)
                .Include(a => a.Role)
                .OrderBy(a => a.RoleId)
                .ToList();
            return Page();
        }
    }
}
