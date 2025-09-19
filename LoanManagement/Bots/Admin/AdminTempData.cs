using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagement.UI.Bots.Admin
{
    public class AdminTempData
    {
        // User registration
        public string? FullName { get; set; }
        public string? Username { get; set; }
        public string? Password { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }

        // Officer registration
        public string? OfficerFullName { get; set; }
        public string? OfficerUsername { get; set; }
        public string? OfficerPassword { get; set; }
        public string? OfficerEmail { get; set; }
        public string? OfficerPhoneNumber { get; set; }

        // Loan rejection
        public int TempLoanId { get; set; }
        public string? TempRejectReason { get; set; }
    }
}
