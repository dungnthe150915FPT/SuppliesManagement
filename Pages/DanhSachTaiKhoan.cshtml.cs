using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.DBContext;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages
{
    public class DanhSachTaiKhoanModel : PageModel
    {
        private readonly SuppliesManagementDBContext context;

        public DanhSachTaiKhoanModel(SuppliesManagementDBContext context)
        {
            this.context = context;
        }
        public List<Account> Accounts { get; set; }
        public void OnGet()
        {
            Accounts = context.Accounts.Include(a => a.Role).OrderBy(a => a.RoleID).ToList();
        }
    }
}
