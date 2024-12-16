using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;
using SuppliesManagement.Models.ViewModels;

namespace SuppliesManagement.Pages.Admin
{
    public class UserDetailModel : PageModel
    {
        private readonly SuppliesManagementProjectContext context;

        public UserDetailModel(SuppliesManagementProjectContext context)
        {
            this.context = context;
        }

        public AccountViewModel Account { get; set; }
        public List<Role> Roles { get; set; }

        public IActionResult OnGet(Guid id)
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }
            var roles = context.Roles.ToList();
            var account = context.Accounts.Include(a => a.Role).FirstOrDefault(a => a.Id == id);
            if (account == null)
            {
                return NotFound();
            }
            else
            {
                Account = new AccountViewModel
                {
                    Id = account.Id,
                    Username = account.Username,
                    Password = account.Password,
                    Address = account.Address,
                    DateOfBirth = account.DateOfBirth,
                    Email = account.Email,
                    Fullname = account.Fullname,
                    Gender = account.Gender,
                    Phone = account.Phone,
                    RoleId = account.RoleId,
                    Rolename = account.Role.Name
                };
            }
            return Page();
        }
    }
}
