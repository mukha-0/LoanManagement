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
        public int LoanId { get; set; }
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
