using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagement.Domain.Entities
{
    public class Transaction
    {
        public int TransactionId { get; set; }
        public int UserId { get; set; }
        public int LoanId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = null!;
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;


        public User User { get; set; } = null!;
        public Loan Loan { get; set; }
    }
}
