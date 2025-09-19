using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagement.UI.Bots.LoanOfficer
{
    public class LoanOfficerTempData
    {
        public int TempLoanId { get; set; }
        public string? TempRejectReason { get; set; }
        public string? TempUsername { get; set; }
        public string? TempPassword { get; set; }
        public string LoanDecision { get; internal set; }
        public string FullName { get; internal set; }
        public string Email { get; internal set; }
        public string PhoneNumber { get; internal set; }
        public int OfficerId { get; internal set; }
    }
}
