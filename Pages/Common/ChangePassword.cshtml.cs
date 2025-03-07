using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages.Common
{
    public class ChangePasswordModel : PageModel
    {
        private readonly SuppliesManagementProjectContext _dbContext;

        public ChangePasswordModel(SuppliesManagementProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        [BindProperty]
        public string CurrentPassword { get; set; }

        [BindProperty]
        public string NewPassword { get; set; }

        [BindProperty]
        public string ConfirmNewPassword { get; set; }

        public IActionResult OnGet()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out var userId))
            {
                return RedirectToPage("/Common/SignIn");
            }

            // var user = _dbContext.Accounts.Include(u => u.Role).FirstOrDefault(a => a.Id == userId);
            // if (user == null)
            // {
            //     return RedirectToPage("/Error/AccessDenied");
            // }
            return Page();
        }

        public IActionResult OnPost()
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

            if (NewPassword == CurrentPassword)
            {
                TempData["ErrorChangePassword"] =
                    "Mật khẩu hiện tại và mật khẩu mới không được giống nhau.";
                return Page();
            }

            if (user.Password != CurrentPassword)
            {
                TempData["ErrorChangePassword"] = "Mật khẩu hiện tại không chính xác.";
                return Page();
            }

            if (NewPassword != ConfirmNewPassword)
            {
                TempData["ErrorChangePassword"] = "Mật khẩu mới và xác nhận mật khẩu không khớp.";
                return Page();
            }

            //https://stackoverflow.com/questions/34715501/validating-password-using-regex-c-sharp
            var hasNumber = new Regex(@"[0-9]+");
            var hasUpperChar = new Regex(@"[A-Z]+");
            var hasMiniMaxChars = new Regex(@".{8,15}");
            var hasLowerChar = new Regex(@"[a-z]+");
            var hasSymbols = new Regex(@"[!@#$%^&*()_+=\[{\]};:<>|./?,-]");

            if (!hasLowerChar.IsMatch(NewPassword))
            {
                TempData["ErrorChangePassword"] = "Mật khẩu mới phải chứa ít nhất một chữ cái viết thường";
                return Page();
            }
            else if (!hasUpperChar.IsMatch(NewPassword))
            {
                TempData["ErrorChangePassword"] = "Mật khẩu mới phải chứa ít nhất một chữ cái viết hoa";
                return Page();
            }
            else if (!hasMiniMaxChars.IsMatch(NewPassword))
            {
                TempData["ErrorChangePassword"] = "Mật khẩu mới phải nằm trong khoảng 8-15 ký tự";
                return Page();
            }
            else if (!hasNumber.IsMatch(NewPassword))
            {
                TempData["ErrorChangePassword"] = "Mật khẩu mới phải chứa ít nhất một giá trị số";
                return Page();
            }
            else if (!hasSymbols.IsMatch(NewPassword))
            {
                TempData["ErrorChangePassword"] =
                    "Mật khẩu mới phải chứa ít nhất một ký tự đặc biệt (!@#$%^&*...)";
                return Page();
            }
            else
            {
                user.Password = NewPassword;
                TempData["SuccessChangePassword"] = "Mật khẩu đã được thay đổi thành công.";
                _dbContext.SaveChanges();
            }

            return RedirectToPage("/Common/ChangePassword");
        }
    }
}
