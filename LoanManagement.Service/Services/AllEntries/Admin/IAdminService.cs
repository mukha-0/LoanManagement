using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Entities;

namespace LoanManagement.Service.Services.AllEntries.Admin
{
    public interface IAdminService
    {
        Task<IEnumerable<object>> GetAllUsersAsync();
        Task<IEnumerable<object>> GetAllOfficersAsync();
        Task DeleteUserAsync(int id);
        Task DeleteOfficerAsync(int id);
        Task AddNewAdmin(Adminn admin);
        Task GiveDiscountToTopBorrower(int topN);
    }
}
