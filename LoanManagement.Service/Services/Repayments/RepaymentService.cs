using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.DataAccess.Repositories;
using LoanManagement.Domain.Entities;
using LoanManagement.Service.Exceptions;
using LoanManagement.Service.Services.Repayments.Models;

namespace LoanManagement.Service.Services.Repayments
{
    internal class RepaymentService : IRepaymentService
    {
        private readonly IRepository<Repayment> repaymentRepository;
        private readonly IRepository<Loan> loanRepository;
        public RepaymentService()
        {
            repaymentRepository = new Repository<Repayment>();
            loanRepository = new Repository<Loan>();
        }
        public async Task<decimal> CheckOutstandingBalance(int loanId)
        {
            var repayments = await repaymentRepository.SelectAsync(loanId);
            if (repayments == null)
            {
                throw new Exception("No repayments found for the given loan ID.");
            }
            return repayments.RemainingBalance;
        }

        public async Task<IEnumerable<Repayment>> GetAllRepayments()
        {
            return repaymentRepository.SelectAllAsQueryable().ToList();
        }

        public async Task<RepaymentViewModel> GetRepaymentsByUserId(int userId)
        {
            var repayments = repaymentRepository
                .SelectAllAsQueryable()
                .Where(r => r.Loan.CustomerId == userId)
                .ToList();
            if (repayments == null || !repayments.Any())
            {
                throw new NotFoundException("Not found");
            }
            var repaymentViewModel = new RepaymentViewModel
            {
                Id = userId,
                LoanId = repayments.First().LoanId,
                Amount = repayments.Sum(r => r.AmountPaid),
                Date = repayments.Max(r => r.PaymentDate)
            };
            return repaymentViewModel;
        }

        public async Task<RepaymentViewModel> GetRepaymentSchedule(int loanId)
        {
            var repayments = repaymentRepository
                .SelectAllAsQueryable()
                .Where(r => r.LoanId == loanId)
                .ToList();
            if (repayments == null || !repayments.Any())
            {
                throw new NotFoundException("Not found");
            }
            var repaymentViewModel = new RepaymentViewModel
            {
                Id = loanId,
                LoanId = loanId,
                Amount = repayments.Sum(r => r.AmountPaid),
                Date = repayments.Max(r => r.PaymentDate)
            };
            return repaymentViewModel;
        }

        public async Task MakeRepayment(RepaymentCreateModel repaymentCreateModel)
        {
            var loan = loanRepository.SelectAsync(repaymentCreateModel.LoanId);
            if (loan == null)
            {
                throw new NotFoundException("Loan not found");
            }
            if (loan.Result.Status != Domain.Enums.LoanStatus.Approved)
            {
                throw new InvalidOperationException("Cannot make a repayment on a loan that is not approved.");
            }
            if (repaymentCreateModel.Amount <= 0)
            {
                throw new ArgumentException("Repayment amount must be greater than zero.");
            }

            var leftbalance = await CheckOutstandingBalance(repaymentCreateModel.LoanId);

            if (repaymentCreateModel.PaymentDate.Month - loan.Result.EndDate.Month < loan.Result.DurationMonths)
            {
                repaymentCreateModel.Amount *= 1.05m;
            }
            var repayment = new Repayment
            {
                LoanId = repaymentCreateModel.LoanId,
                AmountPaid = repaymentCreateModel.Amount,
                PaymentDate = DateTime.UtcNow,
                RemainingBalance = leftbalance - repaymentCreateModel.Amount
            };
            await repaymentRepository.InsertAsync(repayment);
        }
    }
}
