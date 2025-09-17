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
        public RepaymentService()
        {
            repaymentRepository = new Repository<Repayment>();
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

        public async Task<RepaymentViewModel> GetRepaymentsByCustomer(int customerId)
        {
            var repayments = repaymentRepository
                .SelectAllAsQueryable()
                .Where(r => r.Loan.CustomerId == customerId)
                .ToList();
            if (repayments == null || !repayments.Any())
            {
                throw new NotFoundException("Not found");
            }
            var repaymentViewModel = new RepaymentViewModel
            {
                Id = customerId,
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
            var repayment = new Repayment
            {
                LoanId = repaymentCreateModel.LoanId,
                PaymentDate = DateTime.UtcNow,
                AmountPaid = repaymentCreateModel.Amount
            };
            await repaymentRepository.InsertAsync(repayment);
        }
    }
}
