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
                "Server=ROOT;Database=LoanManagement2;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
                );
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Loan> Loans { get; set; }
        public DbSet<Repayment> Repayments { get; set; }
        public DbSet<LoanOfficerr> LoanOfficers { get; set; }
        public DbSet<Adminn> Admins { get; set; }
    }
}
