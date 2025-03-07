using System.Net;
using System.Net.Mail;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using MimeKit;
using MimeKit.Text;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages.Admin
{
    public class AddUserModel : PageModel
    {
        private readonly SuppliesManagementProjectContext _context;

        public AddUserModel(SuppliesManagementProjectContext context)
        {
            _context = context;
        }

        [BindProperty]
        public string Username { get; set; }

        [BindProperty]
        public string Fullname { get; set; }

        [BindProperty]
        public DateTime DateOfBirth { get; set; }

        [BindProperty]
        public bool Gender { get; set; }

        [BindProperty]
        public string Phone { get; set; }

        [BindProperty]
        public string Address { get; set; }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public int Role { get; set; }

        public List<Role> Roles { get; set; }

        public IActionResult OnGet()
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }
            Roles = _context.Roles.Where(r => r.Id != 1).ToList();
            return Page();
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            Roles = _context.Roles.Where(r => r.Id != 1).ToList();
            // Tạo mật khẩu ngẫu nhiên
            string password = GenerateRandomPassword(8);

            if (_context.Accounts.Include(a => a.Role).Any(a => a.Username == Username))
            {
                ModelState.AddModelError(string.Empty, "Username này đã tồn tại.");
                TempData["Error"] = "Username này đã tồn tại";
                return Page();
            }

            // Tạo tài khoản mới
            var newAccount = new Account
            {
                Id = Guid.NewGuid(),
                Username = Username,
                Password = password,
                RoleId = Role,
                Fullname = Fullname,
                DateOfBirth = DateOfBirth,
                Gender = Gender,
                Phone = Phone,
                Address = Address,
                Email = Email
            };

            // Lưu tài khoản vào DB
            _context.Accounts.Add(newAccount);
            _context.SaveChanges();

            // Gửi email chứa mật khẩu
            var receiver = newAccount.Email;
            var subject = "Đăng kí tài khoản mới";
            var message = "";
            if (Role == 2)
            {
                message =
                    $"Xin chào {newAccount.Fullname},\n\n"
                    + $"Tài khoản của bạn đã được tạo thành công với thông tin sau:\n"
                    + $"Tên đăng nhập: {Username}\n"
                    + $"Mật khẩu: {password}\n\n"
                    + $"Quyền: Thủ Kho\n\n"
                    + $"Vui lòng đổi mật khẩu sau khi đăng nhập.\n"
                    + $"Trân trọng,\nPhần Mềm Quản Lý Kho";
            }
            else
            {
                message =
                    $"Xin chào {newAccount.Fullname},\n\n"
                    + $"Tài khoản của bạn đã được tạo thành công với thông tin sau:\n"
                    + $"Tên đăng nhập: {Username}\n"
                    + $"Mật khẩu: {password}\n\n"
                    + $"Quyền: Người Nhận Hàng\n\n"
                    + $"Vui lòng đổi mật khẩu sau khi đăng nhập.\n"
                    + $"Trân trọng,\nPhần Mềm Quản Lý Kho";
            }

            SendPasswordEmail(receiver, subject, message);

            TempData["SuccessMessage"] =
                "Tài khoản đã được thêm mới. Mật khẩu đã gửi qua email. Kiểm tra gmail: "
                + newAccount.Email
                + " để lấy mật khẩu";
            return Page();
        }

        private string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            var random = new Random();
            return new string(
                Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray()
            );
        }

        private async Task SendPasswordEmail(string email, string subject, string message)
        {
            if (string.IsNullOrWhiteSpace(subject))
                throw new ArgumentException("Subject cannot be null or empty", nameof(subject));

            try
            {
                var mail = "dungdev224@gmail.com";
                var appPassword = "tnjgrgrmsvbvtvko";

                // Cấu hình SMTP
                using var smtp = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true, // Bật SSL
                    Credentials = new NetworkCredential(mail, appPassword) // Xác thực với email và mật khẩu ứng dụng
                };

                // Cấu hình email
                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(mail, "Supplies Management"), // Người gửi
                    Subject = subject.Trim(), // Chủ đề email
                    Body = message, // Nội dung email
                    IsBodyHtml = false // Nếu không cần gửi HTML
                };

                mailMessage.To.Add(email); // Người nhận
                await smtp.SendMailAsync(mailMessage); // Gửi email

                Console.WriteLine("Email sent successfully.");
                TempData["SuccessAddUser"] = "Email sent successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                TempData["ErrorAddUser"] = $"Failed to send email: {ex.Message}";
                throw;
            }
        }
    }
}
