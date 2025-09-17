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
        public LoanService()
        {
            loanRepository = new Repository<Loan>();
        }
        public async Task ApplyForLoan(LoanCreateModel loanCreateModel)
        {
            var loan = new Loan
            {
                Amount = loanCreateModel.Amount,
                DurationMonths = loanCreateModel.TermInMonths,
                InterestRate = loanCreateModel.InterestRate,
                StartDate = loanCreateModel.StartDate,
                CustomerId = loanCreateModel.CustomerId,
                Status = Domain.Enums.LoanStatus.Pending
            };
            await loanRepository.InsertAsync(loan);
        }

        public async Task ApproveLoan(int loanId, int approvedByUserId)
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

        public async Task CalculateInterest(int loanId)
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

        public async Task<List<LoanViewModel>> GetActiveLoansByCustomer(int customerId)
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

        public async Task<List<LoanViewModel>> GetAllLoans()
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

        public async Task RejectLoan(int loanId)
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
