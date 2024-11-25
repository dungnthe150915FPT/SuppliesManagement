namespace SuppliesManagement.Models
{
    public class Account
    {
        public Guid ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int RoleID { get; set; }
        public Role Role { get; set; }
    }
}
