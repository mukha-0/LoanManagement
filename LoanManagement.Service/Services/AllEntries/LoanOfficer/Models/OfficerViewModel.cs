using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Entities;

namespace LoanManagement.Service.Services.AllEntries.LoanOfficer.Models
{
    public class OfficerViewModel
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Firstname { get; set; }
        public string Password { get; set; }
        public string Lastname { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int ApprovedLoansCount { get; set; }
        public ICollection<Loan> ApprovedLoans { get; set; } = new List<Loan>();
    }
}
