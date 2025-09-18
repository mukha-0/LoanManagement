using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Entities;
using LoanManagement.Domain.Enums;

namespace LoanManagement
{
    public class LoanOfficerr
    {
        public int OfficerId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; }
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Role Role { get; set; } = Role.LoanOfficer;

        public ICollection<Loan> LoansApproved { get; set; } = new List<Loan>();
    }
}
