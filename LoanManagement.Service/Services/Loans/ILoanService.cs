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
        Task ApproveLoanAsync(int loanId, int approvedByUserId);

        Task RejectLoanAsync(int loanId);

        Task<List<LoanViewModel>> GetActiveLoansByUserAsync(int customerId);

        Task CalculateInterestAsync(int loanId);

        Task<List<LoanViewModel>> GetAllLoansAsync();

        Task<Loan> ApplyForLoanAsync(LoanCreateModel loanCreateModel);
    }
}
