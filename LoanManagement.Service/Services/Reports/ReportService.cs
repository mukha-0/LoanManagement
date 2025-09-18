using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.DataAccess.Repositories;
using LoanManagement.Domain.Entities;
using LoanManagement.Domain.Enums;
using LoanManagement.Service.Exceptions;
using LoanManagement.Service.Services.Reports.Models;

namespace LoanManagement.Service.Services.Reports
{
    public class ReportService : IReportService
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

        public async Task<object> GetByUserIdAsync(int userId)
        {
            var user = await reportServiceRepository.SelectAsync(userId)
                ?? throw new NotFoundException("User is not found");
            return user;
        }

        public async Task<object> GetLoansByStatusAsync(LoanStatus status)
        {
            var loans = loanRepository.SelectAllAsQueryable()
                .Where(l => l.Status == status)
                .ToList();
            return loans;
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
