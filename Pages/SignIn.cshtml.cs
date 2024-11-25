using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SuppliesManagement.DBContext;

namespace SuppliesManagement.Pages
{
    public class SignInModel : PageModel
    {
        private readonly SuppliesManagementDBContext context;

        public SignInModel(SuppliesManagementDBContext context)
        {
            this.context = context;
        }
        public void OnGet()
        {
        }
        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public IActionResult OnPost()
        {
            var user = context.Accounts.FirstOrDefault(
                u => u.Username == Username && u.Password == Password);
            if (user != null && user.RoleID == 2)
            {
                HttpContext.Session.SetString("Username", user.Username);
                return RedirectToPage("/DanhSachHang");
            }
            else if (user != null && user.RoleID == 1)
            {
                HttpContext.Session.SetString("Username", user.Username);
                return RedirectToPage("/DanhSachTaiKhoan");
            }
            else
            {
                ViewData["Error"] = "Đăng nhập thất bại, tên tài khoản hoặc mật khẩu không đúng!";
                return null;
            }
        }
    }
}
