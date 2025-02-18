using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SuppliesManagement.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SuppliesManagement.Pages
{
    public class DanhSachTaiKhoanModel : PageModel
    {
        private readonly SuppliesManagementProjectContext context;

        public DanhSachTaiKhoanModel(SuppliesManagementProjectContext context)
        {
            this.context = context;
        }

        public List<Account> Accounts { get; set; }
        public List<SelectListItem> Roles { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public const int PageSize = 10;

        [BindProperty(SupportsGet = true)]
        public string SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Fullname { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Phone { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Email { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool? Gender { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? RoleId { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }

        public IActionResult OnGet(int pageNumber = 1)
        {
            var role = HttpContext.Session.GetInt32("RoleId");
            if (role != 1)
            {
                return RedirectToPage("/Error/AccessDenied");
            }

            Roles = context.Roles
                .Where(r => r.Id != 1)
                .Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name })
                .ToList();

            IQueryable<Account> query = context.Accounts
                .Where(a => a.RoleId != 1)
                .Include(a => a.Role);

            // Filtering
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(a => a.Username.Contains(SearchTerm));
            }

            if (!string.IsNullOrEmpty(Fullname))
            {
                query = query.Where(a => a.Fullname.Contains(Fullname));
            }

            if (!string.IsNullOrEmpty(Phone))
            {
                query = query.Where(a => a.Phone.Contains(Phone));
            }

            if (!string.IsNullOrEmpty(Email))
            {
                query = query.Where(a => a.Email.Contains(Email));
            }

            if (Gender.HasValue)
            {
                query = query.Where(a => a.Gender == Gender.Value);
            }

            if (RoleId.HasValue)
            {
                query = query.Where(a => a.RoleId == RoleId.Value);
            }

            if (StartDate.HasValue)
            {
                query = query.Where(a => a.DateOfBirth >= StartDate.Value);
            }

            if (EndDate.HasValue)
            {
                query = query.Where(a => a.DateOfBirth <= EndDate.Value);
            }

            // Pagination
            int totalItems = query.Count();
            TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
            CurrentPage = pageNumber;

            Accounts = query.Skip((pageNumber - 1) * PageSize).Take(PageSize).ToList();

            return Page();
        }
    }
}
