using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Enums;

namespace LoanManagement.Domain.Entities
{
    public class Loan
    {
        public int LoanId { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
        public decimal InterestRate { get; set; }
        public int DurationMonths { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public LoanStatus Status { get; set; } = LoanStatus.Pending;


        public ICollection<Repayment> Repayments { get; set; } = new List<Repayment>();
    }
}
