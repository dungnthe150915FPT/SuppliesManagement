using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;
using System.Linq;

namespace SuppliesManagement.Pages.Common
{
    public class ProfileModel : PageModel
    {
        private readonly SuppliesManagementProjectContext _dbContext;

        public ProfileModel(SuppliesManagementProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty]
        public Account User { get; set; }

        public IActionResult OnGet()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Common/SignIn");
            }

            /*            User = _dbContext.Accounts.FirstOrDefault(a => a.Id == userId);*/
            User = _dbContext.Accounts.Include(u => u.Role).FirstOrDefault(a => a.Id == userId);
            if (User == null)
            {
                return RedirectToPage("/Error");
            }

            return Page();
        }

        public IActionResult OnPost(
            string Fullname,
            bool? Gender,
            DateTime? DateOfBirth,
            string? Phone,
            string? Address,
            string? Email
        )
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Common/SignIn");
            }

            var user = _dbContext.Accounts.Include(u => u.Role).FirstOrDefault(a => a.Id == userId);
            if (user == null)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            // Cập nhật thông tin
            user.Fullname = Fullname;
            user.Gender = Gender;
            user.DateOfBirth = DateOfBirth;
            user.Phone = Phone;
            user.Address = Address;
            user.Email = Email;

            _dbContext.Accounts.Update(user);
            _dbContext.SaveChanges();

            TempData["SuccessMessage"] = "Thông tin cá nhân đã được cập nhật thành công!";
            return RedirectToPage();
        }
    }
}