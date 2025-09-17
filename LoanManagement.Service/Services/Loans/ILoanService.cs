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
        Task ApplyForLoan(LoanCreateModel loanCreate);

        Task ApproveLoan(int loanId, int approvedByUserId);

        Task RejectLoan(int loanId);

        Task<List<LoanViewModel>> GetActiveLoansByCustomer(int customerId);

        Task CalculateInterest(int loanId);

        Task<List<LoanViewModel>> GetAllLoans();
    }
}
