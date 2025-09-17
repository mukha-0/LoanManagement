using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LoanManagement.Domain.Entities
{
    public class Customer
    {
        public int CustomerId { get; set; }
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Address { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime DateOfBirth { get; set; }
        public decimal AnnualIncome { get; set; }
        public string IdentificationNumber { get; set; }


        public ICollection<Loan> Loans { get; set; } = new List<Loan>();
    }
}
