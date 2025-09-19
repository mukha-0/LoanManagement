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
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

namespace LoanManagement.Service.Services.Reports
{
    public class ReportService : IReportService
    {
        private readonly IRepository<ReportService> reportServiceRepository;
        private readonly IRepository<Loan> loanRepository;
        private readonly IRepository<User> userRepository;
        private IReportService reportService;

        public ReportService(DataAccess.Context.AppDBContext db)
        {
            reportServiceRepository = new Repository<ReportService>();
            userRepository = new Repository<User>();
        }

        public ReportService(IReportService reportService)
        {
            this.reportService = reportService;
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

        public async Task<decimal> GetTotalApprovedLoansAsync()
        {
            var totalApproved = loanRepository.SelectAllAsQueryable()
                .Where(l => l.Status == Domain.Enums.LoanStatus.Approved)
                .Sum(l => l.Amount);
            return totalApproved;
        }

        public async Task<decimal> GetTotalOutstandingLoansAsync()
        {
            var totalOutstanding = loanRepository.SelectAllAsQueryable()
                .Where(l => l.Status == Domain.Enums.LoanStatus.Approved || l.Status == Domain.Enums.LoanStatus.Pending)
                .Sum(l => l.Amount);
            return totalOutstanding;
        }

        public async Task<decimal> GetTotalRejectedLoansAsync()
        {
            var totalRejected = loanRepository.SelectAllAsQueryable()
                .Where(l => l.Status == Domain.Enums.LoanStatus.Rejected)
                .Sum(l => l.Amount);
            return totalRejected;
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
        public async Task GenerateReport(TelegramBotClient telegramBotClient, long chatId, ReportsCreateModel reportsCreateModel)
        {
            var reportMessage = new StringBuilder();
            reportMessage.AppendLine("Loan Management Report");
            reportMessage.AppendLine("----------------------");
            reportMessage.AppendLine($"Total Approved Loans: {await GetTotalApprovedLoansAsync()}");
            reportMessage.AppendLine($"Total Outstanding Loans: {await GetTotalOutstandingLoansAsync()}");
            reportMessage.AppendLine($"Total Rejected Loans: {await GetTotalRejectedLoansAsync()}");
            reportMessage.AppendLine("----------------------");
            reportMessage.AppendLine("Top Borrowers:");
            var topBorrowers = await GetTopBorrowersAsync(5) as List<Loan>;
            if (topBorrowers != null)
            {
                foreach (var borrower in topBorrowers)
                {
                    var user = await userRepository.SelectAsync(borrower.CustomerId);
                    if (user != null)
                    {
                        reportMessage.AppendLine($"User ID: {user.UserId}, Name: {user.FirstName} {user.LastName}, Amount: {borrower.Amount}");
                    }
                }
            }
            await telegramBotClient.SendTextMessageAsync(chatId, reportMessage.ToString());
        }
    }
}
