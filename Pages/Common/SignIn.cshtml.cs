using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SuppliesManagement.Models;
using System.Linq;
using System;
using DNTCaptcha.Core;
using Microsoft.Extensions.Options;
using System.Runtime.InteropServices;

namespace SuppliesManagement.Pages
{
    public class SignInModel : PageModel
    {
        private readonly SuppliesManagementProjectContext context;

        public SignInModel(
            SuppliesManagementProjectContext context
        )
        {
            this.context = context;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Password { get; set; }

        [BindProperty]
        public string CaptchaInput { get; set; } // Captcha người dùng nhập

        public string CaptchaGenerated { get; private set; } // Captcha được sinh ra

        private char[] chars = {
            '1', 'A', 'a', 'B', 'b', 'C', 'c', '2', 'D', 'd', 'E', 'e', 'F', 'f', '3',
            'G', 'g', 'H', 'h', 'I', 'i', 'J', 'j', 'K', 'k', 'L', 'l', '4', 'M', 'm',
            'N', 'n', 'O', 'o', '5', 'P', 'p', 'Q', 'q', 'R', 'r', 'S', 's', 'T', 't',
            '6', '7', 'U', 'u', 'V', 'v', 'U', 'u', 'W', 'w', '8', 'X', 'x', 'Y', 'y',
            'Z', 'z', '9'
        };
        public void OnGet()
        {
            var roleId = HttpContext.Session.GetInt32("RoleId");
            ViewData["RoleId"] = roleId;
            if (HttpContext.Session.GetInt32("FailedAttempts") == null)
            {
                HttpContext.Session.SetInt32("FailedAttempts", 0);
            }
        }

        public IActionResult OnPost()
        {
            int failedAttempts = HttpContext.Session.GetInt32("FailedAttempts") ?? 0;

            if (failedAttempts >= 3)
            {
                // Lấy captcha đã được lưu trong session
                string? storedCaptcha = HttpContext.Session.GetString("CaptchaGenerated");
                // Kiểm tra captcha
                if (string.IsNullOrEmpty(CaptchaInput) || CaptchaInput != storedCaptcha)
                {
                    ViewData["Error"] = "Mã captcha không đúng, vui lòng thử lại.";
                    CaptchaGenerated = storedCaptcha;
                    return Page();
                }
            }

            // check acc
            var user = context.Accounts.FirstOrDefault(
                u => u.Username == Username && u.Password == Password);

            if (user != null)
            {
                /*                HttpContext.Session.SetString("Username", user.Username);
                                HttpContext.Session.SetInt32("RoleId", user.RoleId);*/
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("UserId", user.Id.ToString());
                HttpContext.Session.SetInt32("RoleId", user.RoleId);
                HttpContext.Session.SetInt32("FailedAttempts", 0); // Reset số lần thất bại
                HttpContext.Session.Remove("CaptchaGenerated"); // Xóa captcha
                if (user.RoleId == 2)
                {
                    return RedirectToPage("/User/Dashboard");
                }
                else if (user.RoleId == 1)
                {
                    return RedirectToPage("/User/Dashboard");
                }
                else if (user.RoleId == 3)
                {
                    return RedirectToPage("/User/DanhSachHangNhan");
                }
            }

            // login failed
            failedAttempts++;
            HttpContext.Session.SetInt32("FailedAttempts", failedAttempts);

            if (failedAttempts >= 3)
            {
                CaptchaGenerated = GenerateCaptcha();
                HttpContext.Session.SetString("CaptchaGenerated", CaptchaGenerated);
            }

            ViewData["Error"] = "Tên tài khoản hoặc mật khẩu không đúng!";
            return Page();
        }


        private string GenerateCaptcha()
        {
            var random = new Random();
            return new string(Enumerable.Repeat(chars, 6)
                                        .Select(s => s[random.Next(s.Length)])
                                        .ToArray());
        }
    }
}
