using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Enums;

namespace LoanManagement.Domain.Entities
{
    public class Adminn
    {
        public string AdminId { get; set; }
        public string AdminUserName { get; set; }
        public string AdminPassword { get; set; }
        public Role Role { get; set; } = Role.Admin;
    }
}
