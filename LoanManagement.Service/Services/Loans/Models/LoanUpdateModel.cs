using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagement.Service.Services.Loans.Models
{
    public class LoanUpdateModel
    {
        public decimal Amount { get; set; }
        public double InterestRate { get; set; }
        public int TermInMonths { get; set; }
        public DateTime StartDate { get; set; }
    }
}
