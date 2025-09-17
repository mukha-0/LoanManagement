using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Service.Services.Repayments.Models;

namespace LoanManagement.Service.Services.Repayments
{
    public interface IRepaymentService
    {
        Task MakeRepayment(RepaymentCreateModel repaymentCreate);

        Task<RepaymentViewModel> GetRepaymentSchedule(int loanId);

        Task<RepaymentViewModel> GetRepaymentsByCustomer(int customerId);

        Task<decimal> CheckOutstandingBalance(int loanId);
    }
}
