﻿using System;
using System.Collections.Generic;

namespace SuppliesManagement.Models
{
    public partial class Account
    {
        public Account()
        {
            HoaDonXuats = new HashSet<HoaDonXuat>();
        }

        public Guid Id { get; set; }
        public string Username { get; set; } = null!;
        public string Password { get; set; } = null!;
        public int RoleId { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual ICollection<HoaDonXuat> HoaDonXuats { get; set; }
    }
}