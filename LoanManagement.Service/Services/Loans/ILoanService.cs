using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Entities;
using LoanManagement.Service.Services.Loans.Models;

namespace LoanManagement.Service.Services.Loans
{
    public interface ILoanService
    {
        Task ApproveLoanAsync(int loanId);

        Task RejectLoanAsync(int loanId);

        Task<List<LoanViewModel>> GetActiveLoansByUserAsync(string customerId);

        Task<decimal> CalculateInterestAsync(int loanId);

        Task<List<LoanViewModel>> GetAllLoansAsync();
        IEnumerable<object> GetLoansWaitingForOfficer();
        Task<Loan> ApplyForLoanAsync(LoanCreateModel loanCreateModel);
        Task<List<Loan>> GetPendingLoans();
        Task<IQueryable<Loan>> GetLoanHistoryOfUserAsync(string userId);
    }
}
