using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Entities;

namespace LoanManagement.Service.Services.Reports.Models
{
    public class ReportsCreateModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public IEnumerable<Loan> Loans { get; set; }
        public IEnumerable<Repayment> Payments { get; set; }
        public decimal TotalLoanAmount { get; set; }
        public decimal TotalPaymentAmount { get; set; }
        public decimal OutstandingBalance { get; set; }
        public int TotalLoans { get; set; }
        public int TotalPayments { get; set; }
        public double AverageLoanAmount { get; set; }
        public double AveragePaymentAmount { get; set; }
        public DateTime ReportGeneratedOn { get; set; }
        public IEnumerable<Loan> TopBorrowers { get; set; }
        public decimal TotalApprovedLoans { get; set; }
    }
}
