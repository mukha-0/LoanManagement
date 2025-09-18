using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Service.Services.AllEntries.LoanOfficer.Models;
using LoanManagement.Service.Services.Loans.Models;

namespace LoanManagement.Service.Services.AllEntries.LoanOfficer
{
    public interface ILoanOfficerService
    {
        Task ApproveLoanApplication(int applicationId);
        Task RejectLoanApplication(int applicationId, string officerUsername, string reason);
        Task<IEnumerable<LoanViewModel>> GetPendingLoanApplications();
        Task<LoanViewModel> GetLoanApplicationDetails(int applicationId);
        Task<OfficerViewModel> GetOfficerById(int officerId);
        Task<IEnumerable<OfficerViewModel>> GetAllOfficers();
        Task ApplyForLoanOfficer(OfficerCreateModel officerCreateModel);
        Task UpdateLoanOfficer(int officerId, OfficerUpdateModel officerUpdateModel);
        Task CreateNewLoanOfficer(OfficerCreateModel officerCreateModel);
    }
}
