namespace SuppliesManagement.Models.ViewModels
{
    public class AccountViewModel
    {
        public AccountViewModel() { }

        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int RoleId { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? Email { get; set; }
        public string? Fullname { get; set; }
        public bool? Gender { get; set; }
        public string Rolename { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public virtual Role Role { get; set; } = null!;
    }
}
