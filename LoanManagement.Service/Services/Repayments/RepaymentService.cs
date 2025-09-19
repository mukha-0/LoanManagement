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
    public class RepaymentService : IRepaymentService
    {
        private readonly IRepository<Repayment> repaymentRepository;
        private readonly IRepository<Loan> loanRepository;
        private IRepaymentService repaymentService;

        public RepaymentService(DataAccess.Context.AppDBContext db)
        {
            repaymentRepository = new Repository<Repayment>();
            loanRepository = new Repository<Loan>();
        }

        public RepaymentService(IRepaymentService repaymentService)
        {
            this.repaymentService = repaymentService;
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
            if (loanId <= 0)
            {
                throw new ArgumentException("Invalid loan ID.");
            }

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

        public async Task MakeRepaymentAsync(RepaymentCreateModel repaymentCreateModel)
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
            if (repaymentCreateModel.Amount > leftbalance)
            {
                throw new InvalidOperationException("Repayment amount exceeds outstanding balance.");
            }
            if (leftbalance - repaymentCreateModel.Amount == 0)
            {
                loan.Result.Status = Domain.Enums.LoanStatus.Closed;
                await loanRepository.UpdateAsync(loan.Result);
            }
            if(repaymentCreateModel.PaymentDate >= loan.Result.EndDate.AddDays(5))
            {
                loan.Result.Status = Domain.Enums.LoanStatus.Overdue;
                loan.Result.Amount += leftbalance * 0.05m;
                await loanRepository.UpdateAsync(loan.Result);

                throw new InvalidOperationException("Loan is overdue. A late 5% fee has been applied to the repayment amount.");
            }
            else if(repaymentCreateModel.PaymentDate >= loan.Result.EndDate.AddDays(10))
            {
                loan.Result.Status = Domain.Enums.LoanStatus.Overdue;
                loan.Result.Amount += leftbalance * 0.10m;
                await loanRepository.UpdateAsync(loan.Result);
                throw new InvalidOperationException("Loan is overdue. A late 10% fee has been applied to the repayment amount.");
            }
            else if (repaymentCreateModel.PaymentDate >= loan.Result.EndDate.AddDays(15))
            {
                loan.Result.Status = Domain.Enums.LoanStatus.Overdue;
                loan.Result.Amount += leftbalance * 0.15m;
                await loanRepository.UpdateAsync(loan.Result);
                throw new InvalidOperationException("Loan is overdue. A late 15% fee has been applied to the repayment amount.");
            }
            else if(repaymentCreateModel.PaymentDate >= loan.Result.EndDate.AddDays(20))
            {
                loan.Result.Status = Domain.Enums.LoanStatus.Suspended;
                await loanRepository.UpdateAsync(loan.Result);
                throw new InvalidOperationException("Your loan repayment has been suspended, " +
                    "we need you to come to the bank to make a repayment or else we would " +
                    "have to sue you to the court!!!");
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
