using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages.Common
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly SuppliesManagementProjectContext context;

        public ForgotPasswordModel(SuppliesManagementProjectContext context)
        {
            this.context = context;
        }

        // public string Username { get; set; }
        // public string Email { get; set; }

        public void OnGet() { }

        public IActionResult OnPost(string Username, string Email)
        {
            var user = context.Accounts.FirstOrDefault(
                u => u.Username == Username && u.Email == Email
            );
            if (user == null)
            {
                TempData["ErrorMessage"] =
                    "Hãy nhập đúng tên đăng nhập và gmail liên kết với tài khoản";
                return Page();
            }
            else
            {
                string New_password = GenerateRandomPassword(8);
                var receiver = Email;
                var subject = "Reset mật khẩu mới";
                var message =
                    $"Xin chào {Username},\n\n"
                    + $"Tài khoản của bạn đã được reset mật khẩu thành công:\n"
                    + $"Mật khẩu mới: {New_password}\n\n"
                    + $"Vui lòng đổi mật khẩu sau khi đăng nhập.\n"
                    + $"Trân trọng,\nSupplies Management";
                SendPasswordEmail(receiver, subject, message);
                TempData["SuccessMessage"] =
                    "Reset mật khẩu thành công, kiểm tra email "
                    + Email
                    + " để lấy mật khẩu cho tài khoản "
                    + Username;
                user.Password = New_password;
                context.SaveChanges();
            }
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
                TempData["SuccessMessage"] = "Email sent successfully.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                TempData["SuccessMessage"] = $"Failed to send email: {ex.Message}";
                throw;
            }
        }
    }
}
