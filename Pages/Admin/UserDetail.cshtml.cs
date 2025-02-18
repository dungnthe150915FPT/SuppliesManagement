using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
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

        [BindProperty]
        public AccountViewModel Account { get; set; }
        public List<SelectListItem> RoleList { get; set; }

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
            RoleList = context.Roles
                .Where(r => r.Id != 1)
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name })
                .ToList();
            return Page();
        }

        public IActionResult OnPost()
        {
/*            if (!ModelState.IsValid)
            {
                RoleList = context.Roles
                    .Where(r => r.Id != 1)
                    .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name })
                    .ToList();
                return Page();
            }*/

            var account = context.Accounts.FirstOrDefault(a => a.Id == Account.Id);
            if (account == null)
            {
                return RedirectToPage("/Error/PageNotFound");
            }
            account.Username = Account.Username;
            account.Fullname = Account.Fullname;
            account.DateOfBirth = Account.DateOfBirth;
            account.Gender = Account.Gender;
            account.Phone = Account.Phone;
            account.Address = Account.Address;
            account.Email = Account.Email;
            account.RoleId = Account.RoleId;
            account.Password = Account.Password;

            context.SaveChanges();

            TempData["SuccessMessage"] = "Thông tin người dùng đã được cập nhật thành công.";
            return RedirectToPage(new { id = Account.Id });
        }
    }
}
