using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Entities;
using LoanManagement.Service.Services.Repayments.Models;

namespace LoanManagement.Service.Services.Repayments
{
    public interface IRepaymentService
    {
        Task MakeRepaymentAsync(RepaymentCreateModel repaymentCreate);

        Task<RepaymentViewModel> GetRepaymentSchedule(int loanId);

        Task<RepaymentViewModel> GetRepaymentsByUserId(int userId);

        Task<decimal> CheckOutstandingBalance(int loanId);
        Task<IEnumerable<Repayment>> GetAllRepayments();
    }
}
