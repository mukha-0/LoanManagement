using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.DataAccess.Repositories;
using LoanManagement.Domain.Entities;
using LoanManagement.Service.Services.Loans.Models;

namespace LoanManagement.Service.Services.Loans
{
    internal class LoanService : ILoanService
    {
        private readonly IRepository<Loan> loanRepository;
        private readonly IRepository<Repayment> repaymentRepository = new Repository<Repayment>();
        private readonly IRepository<Transaction> transactionRepository = new Repository<Transaction>();
        public LoanService()
        {
            loanRepository = new Repository<Loan>();
        }

        public async Task<Loan> ApplyForLoanAsync(LoanCreateModel loanCreateModel)
        {
            if (loanCreateModel.Amount <= 0) throw new ArgumentException("Loan amount must be greater than 0");
            if (loanCreateModel.TermInMonths <= 0) throw new ArgumentException("Term must be valid");

            var loan = new Loan
            {
                CustomerId = loanCreateModel.CustomerId,
                Amount = loanCreateModel.Amount,
                InterestRate = loanCreateModel.InterestRate,
                DurationMonths = loanCreateModel.TermInMonths,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(loanCreateModel.TermInMonths),
                Status = Domain.Enums.LoanStatus.Pending,
            };

            await loanRepository.InsertAsync(loan);


            var transaction = new Transaction
            {
                LoanId = loan.LoanId,
                Amount = loan.Amount,
                TransactionDate = DateTime.UtcNow,
                Description = "Loan Approved"
            };
            await transactionRepository.InsertAsync(transaction);

            return loan;
        }

        public async Task ApproveLoanAsync(int loanId, int approvedByUserId)
        {
            var loan = await loanRepository.SelectAsync(loanId);
            if (loan == null)
            {
                throw new Exception("Loan not found");
            }

            if (loan.Status != Domain.Enums.LoanStatus.Pending)
            {
                throw new Exception("Only pending loans can be approved");
            }

            loan.Status = Domain.Enums.LoanStatus.Active;
            loan.EndDate = loan.StartDate.AddMonths(loan.DurationMonths);
            await loanRepository.UpdateAsync(loan);
        }

        public async Task CalculateInterestAsync(int loanId)
        {
            var loan = await loanRepository.SelectAsync(loanId);
            if (loan == null)
            {
                throw new Exception("Loan not found");
            }
            if (loan.Status != Domain.Enums.LoanStatus.Active)
            {
                throw new Exception("Interest can only be calculated for active loans");
            }
            var monthsElapsed = ((DateTime.UtcNow.Year - loan.StartDate.Year) * 12) + DateTime.UtcNow.Month - loan.StartDate.Month;
            if (monthsElapsed <= 0)
            {
                throw new Exception("No interest accrued yet");
            }
            var interest = loan.Amount * (loan.InterestRate / 100) * monthsElapsed;
        }

        public async Task<List<LoanViewModel>> GetActiveLoansByCustomerAsync(int customerId)
        {
            var loans = loanRepository
                .SelectAllAsQueryable()
                .Where(l => l.CustomerId == customerId && l.Status == Domain.Enums.LoanStatus.Active)
                .ToList();
            if (loans == null || !loans.Any())
            {
                throw new Exception("No active loans found for the customer");
            }
            var loanViewModels = loans.Select(l => new LoanViewModel
            {
                Id = l.LoanId,
                CustomerId = l.CustomerId,
                Amount = l.Amount,
                InterestRate = l.InterestRate,
                TermInMonths = l.DurationMonths,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Status = l.Status.ToString()
            }).ToList();

            return loanViewModels;
        }

        public async Task<List<LoanViewModel>> GetAllLoansAsync()
        {
            var loans = loanRepository.SelectAllAsQueryable().ToList();
            if (loans == null || !loans.Any())
            {
                throw new Exception("No loans found");
            }
            var loanViewModels = loans.Select(l => new LoanViewModel
            {
                Id = l.LoanId,
                CustomerId = l.CustomerId,
                Amount = l.Amount,
                InterestRate = l.InterestRate,
                TermInMonths = l.DurationMonths,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Status = l.Status.ToString()
            }).ToList();

            return loanViewModels;
        }

        public async Task<decimal> MakeRepaymentAsync(int loanId, decimal amount, DateTime paymentDate)
        {
            var loan = await loanRepository.SelectAsync(loanId);
            if (loan == null) throw new Exception("Loan not found");

            decimal finalAmount = amount;
            if (paymentDate < loan.EndDate)
            {
                finalAmount = amount * 0.95m;
            }

            var repayment = new Repayment
            {
                LoanId = loanId,
                RemainingBalance = finalAmount,
                PaymentDate = paymentDate
            };

            await repaymentRepository.InsertAsync(repayment);

            var transaction = new Transaction
            {
                LoanId = loanId,
                Amount = finalAmount,
                TransactionDate = DateTime.UtcNow,
                Description = paymentDate < loan.EndDate
                    ? "Early Repayment with Discount"
                    : "Repayment"
            };
            await transactionRepository.InsertAsync(transaction);

            return finalAmount;
        }

        public async Task RejectLoanAsync(int loanId)
        {
            var loan = await loanRepository.SelectAsync(loanId);
            if (loan == null)
            {
                throw new Exception("Loan not found");
            }
            if (loan.Status != Domain.Enums.LoanStatus.Pending)
            {
                throw new Exception("Only pending loans can be rejected");
            }
            loan.Status = Domain.Enums.LoanStatus.Rejected;
            await loanRepository.UpdateAsync(loan);
        }
    }
}
