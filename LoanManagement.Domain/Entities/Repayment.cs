using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Enums;

namespace LoanManagement.Domain.Entities
{
    public class Repayment
    {
        public int RepaymentId { get; set; }
        public int LoanId { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
        public decimal AmountPaid { get; set; }
        public decimal RemainingBalance { get; set; }
        public RepaymentStatus Status { get; set; } = RepaymentStatus.Pending;


        public Loan Loan { get; set; } = null!;
    }
}
