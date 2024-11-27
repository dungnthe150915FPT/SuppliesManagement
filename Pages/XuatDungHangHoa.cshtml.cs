using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages
{
    public class XuatDungHangHoaModel : PageModel
    {
        private readonly SuppliesManagementProjectContext context;

        public XuatDungHangHoaModel(SuppliesManagementProjectContext context)
        {
            this.context = context;
        }
        public List<HangHoa> HangHoas { get; set; }
        public List<KhoHang> KhoHangs { get; set; }
        public List<Account> Accounts { get; set; }
        public List<NhomHang> NhomHangs { get; set; }
        public List<DonViTinh> DonViTinhs { get; set; }
        public async void OnGet()
        {
            HangHoas = await context.HangHoas.ToListAsync();
            KhoHangs = await context.KhoHangs.ToListAsync();
            Accounts = await context.Accounts.Include(a => a.Role).ToListAsync();
            NhomHangs = await context.NhomHangs.ToListAsync();
            DonViTinhs = await context.DonViTinhs.ToListAsync();
        }
    }
}
