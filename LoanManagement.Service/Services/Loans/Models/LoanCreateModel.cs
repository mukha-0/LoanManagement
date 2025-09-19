using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Entities;

namespace LoanManagement.Service.Services.Loans.Models
{
    public class LoanCreateModel
    {
        public decimal Amount { get; set; }
        public int TermInMonths { get; set; }
        public decimal InterestRate { get; set; }
        public DateTime StartDate { get; set; }
        public int CustomerId
        {
            get; set;
        }
        public string? Purpose { get; set; }
    }
}
