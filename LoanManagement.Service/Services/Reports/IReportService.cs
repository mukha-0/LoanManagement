using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Enums;
using LoanManagement.Service.Services.Reports.Models;

namespace LoanManagement.Service.Services.Reports
{
    public interface IReportService
    {
        Task GetTotalUnconfirmedLoansAsync();
        Task GetTotalRepaidAmountAsync();
        Task<object> GetTopBorrowersAsync(int topN);
        Task<object> GetLoansByStatusAsync(LoanStatus status);
        Task<object> GetByUserIdAsync(int userId);
        Task<List<ReportsViewModel>> GetAllAsync();
    }
}
