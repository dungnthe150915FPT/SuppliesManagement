using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;

namespace SuppliesManagement.Pages.SuppliesManager
{
    public class ChiTietHangModel : PageModel
    {
        private readonly SuppliesManagementProjectContext _dbContext;

        public ChiTietHangModel(SuppliesManagementProjectContext dbContext)
        {
            _dbContext = dbContext;
        }

        public HangHoa HangHoa { get; set; }

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            HangHoa = await _dbContext.HangHoas
                .Include(h => h.NhomHang)
                .Include(h => h.DonViTinh)
                .FirstOrDefaultAsync(h => h.Id == id);

            if (HangHoa == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
