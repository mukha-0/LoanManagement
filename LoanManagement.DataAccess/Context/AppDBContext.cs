using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanManagement.DataAccess.Context
{
    public class AppDBContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder
                .UseSqlServer(
                "Server=ROOT;Database=LoanManagement;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
                );
        }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Repayment> Repayments { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

    }
}
