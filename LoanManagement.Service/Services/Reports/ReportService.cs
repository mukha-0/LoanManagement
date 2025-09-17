using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.DataAccess.Repositories;
using LoanManagement.Domain.Entities;
using LoanManagement.Service.Exceptions;
using LoanManagement.Service.Services.Reports.Models;

namespace LoanManagement.Service.Services.Reports
{
    internal class ReportService : IReportService
    {
        private readonly IRepository<ReportService> reportServiceRepository;
        private readonly IRepository<Loan> loanRepository;
        public ReportService()
        {
            reportServiceRepository = new Repository<ReportService>();
        }

        public Task<List<ReportsViewModel>> GetAllAsync()
        {
            return (Task<List<ReportsViewModel>>)reportServiceRepository.SelectAllAsQueryable();
        }

        public async Task<object> GetByUserAsync(int userId, ReportsViewModel userr)
        {
            var user = await reportServiceRepository.SelectAsync(userId)
                ?? throw new NotFoundException("User is not found");
            return user;
        }

        public async Task<object> GetLoansByStatusAsync(string status)
        {
            var loan = await loanRepository.SelectAsync(status)
                ?? throw new NotFoundException("No loans found with the specified status");
            return loan;
        }

        public async Task<object> GetTopBorrowersAsync(int topN)
        {
            var loans = loanRepository.SelectAllAsQueryable()
                .OrderByDescending(l => l.Amount)
                .Take(topN)
                .ToList();
            return loans;
        }

        public async Task GetTotalRepaidAmountAsync()
        {
            var totalRepaid = loanRepository.SelectAllAsQueryable()
                .Where(l => l.Status == Domain.Enums.LoanStatus.Closed)
                .Sum(l => l.Amount);
        }

        public async Task GetTotalUnconfirmedLoansAsync()
        {
            var totalUnconfirmed = loanRepository.SelectAllAsQueryable()
                .Where(l => l.Status == Domain.Enums.LoanStatus.Pending)
                .Sum(l => l.Amount);
        }
    }
}
