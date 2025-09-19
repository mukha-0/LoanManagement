using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagement.UI.Bots.User
{
    public class UserTempData
    {
        // Loan application
        public decimal LoanAmount { get; set; }
        public int LoanTermMonths { get; set; }
        public decimal LoanInterestRate { get; set; }

        // Repayment
        public int TempLoanId { get; set; }

        // Profile update
        public string? TempUsername { get; set; }
        public string? TempPassword { get; set; }
        public int LoanTerm { get; internal set; }
        public string FullName { get; internal set; }
        public string Email { get; internal set; }
        public string PhoneNumber { get; internal set; }
    }
}
