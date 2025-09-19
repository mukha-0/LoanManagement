using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Enums;

namespace LoanManagement.Domain.Entities
{
    public class User
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
        public Role Role { get; set; } = Role.User;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public ICollection<Repayment> Repayments { get; set; } = new List<Repayment>();
        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
