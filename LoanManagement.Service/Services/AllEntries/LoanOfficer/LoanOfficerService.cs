using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.DataAccess.Repositories;
using LoanManagement.Domain.Entities;
using LoanManagement.Service.Services.AllEntries.LoanOfficer.Models;
using LoanManagement.Service.Services.Loans.Models;

namespace LoanManagement.Service.Services.AllEntries.LoanOfficer
{
    public class LoanOfficerService : ILoanOfficerService
    {
        private readonly IRepository<LoanOfficerr> officerRepository;
        private readonly IRepository<Loan> loanRepository;

        public LoanOfficerService()
        {
            officerRepository = new Repository<LoanOfficerr>();
            loanRepository = new Repository<Loan>();
        }

        public async Task ApplyForLoanOfficer(OfficerCreateModel officerCreateModel)
        {
            var existingOfficer = await officerRepository.SelectAsync(officerCreateModel.OfficerId);
            var newOfficer = new LoanOfficerr
            {
                FirstName = officerCreateModel.FirstName,
                LastName = officerCreateModel.LastName,
                Email = officerCreateModel.Email,
                PhoneNumber = officerCreateModel.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                Role = Domain.Enums.Role.LoanOfficer,
            };
            await officerRepository.InsertAsync(newOfficer);
        }

        public async Task ApproveLoanApplication(int applicationId)
        {
            var loan = await loanRepository.SelectAsync(applicationId);
            if (loan == null)
            {
                throw new Exception("Loan application not found.");
            }
            if (loan.Status != Domain.Enums.LoanStatus.Pending)
            {
                throw new Exception("Only pending loan applications can be approved.");
            }
            loan.Status = Domain.Enums.LoanStatus.Approved;
            await loanRepository.UpdateAsync(loan);
        }

        public async Task CreateNewLoanOfficer(OfficerCreateModel officerCreateModel)
        {
            var existingOfficer = await officerRepository.SelectAsync(officerCreateModel.OfficerId);
            if (existingOfficer != null)
            {
                throw new Exception("Officer with the same ID already exists.");
            }
            var newOfficer = new LoanOfficerr
            {
                FirstName = officerCreateModel.FirstName,
                LastName = officerCreateModel.LastName,
                Email = officerCreateModel.Email,
                PhoneNumber = officerCreateModel.PhoneNumber,
                CreatedAt = DateTime.UtcNow,
                Role = Domain.Enums.Role.LoanOfficer,
            };
            await officerRepository.InsertAsync(newOfficer);
        }

        public async Task<IEnumerable<OfficerViewModel>> GetAllOfficers()
        {
            var officers = officerRepository.SelectAllAsQueryable();
            return officers.Select(o => new OfficerViewModel
            {
                Id = o.OfficerId,
                Firstname = o.FirstName,
                Lastname = o.LastName,
                Password = o.Password,
                Email = o.Email,
                PhoneNumber = o.PhoneNumber,
                
                ApprovedLoansCount = o.LoansApproved.Count
            }).ToList();
        }

        public async Task<LoanViewModel> GetLoanApplicationDetails(int applicationId)
        {
            var loan = await loanRepository.SelectAsync(applicationId);
            if (loan == null)
            {
                throw new Exception("Loan application not found.");
            }
            return new LoanViewModel
            {
                Id = loan.LoanId,
                CustomerId = loan.CustomerId,
                Amount = loan.Amount,
                InterestRate = loan.InterestRate,
                TermInMonths = loan.DurationMonths,
                StartDate = loan.StartDate,
                EndDate = loan.EndDate,
                Status = loan.Status.ToString()
            };
        }

        public async Task<OfficerViewModel> GetOfficerById(int officerId)
        {
            var officer = await officerRepository.SelectAsync(officerId)
                ?? throw new Exception("Officer not found");
            return new OfficerViewModel
            {
                Id = officer.OfficerId,
                Firstname = officer.FirstName,
                Lastname = officer.LastName,
                Email = officer.Email,
                PhoneNumber = officer.PhoneNumber,
                ApprovedLoans = officer.LoansApproved
            };
        }

        public async Task<IEnumerable<LoanViewModel>> GetPendingLoanApplications()
        {
            var pendingLoans = loanRepository
                .SelectAllAsQueryable()
                .Where(l => l.Status == Domain.Enums.LoanStatus.Pending)
                .ToList();
            return pendingLoans.Select(l => new LoanViewModel
            {
                Id = l.LoanId,
                CustomerId = l.CustomerId,
                Amount = l.Amount,
                InterestRate = l.InterestRate,
                TermInMonths = l.DurationMonths,
                StartDate = l.StartDate,
                EndDate = l.EndDate,
                Status = l.Status.ToString()
            }).ToList();
        }

        public async Task RejectLoanApplication(int applicationId, string officerUsername, string reason)
        {
            var loan = await loanRepository.SelectAsync(applicationId);
            if (loan == null)
            {
                throw new Exception("Loan application not found.");
            }
            if (loan.Status != Domain.Enums.LoanStatus.Pending)
            {
                throw new Exception("Only pending loan applications can be rejected.");
            }
            loan.Status = Domain.Enums.LoanStatus.Rejected;
            await loanRepository.UpdateAsync(loan);
        }

        public async Task UpdateLoanOfficer(int officerId, OfficerUpdateModel officerUpdateModel)
        {
            var officer = await officerRepository.SelectAsync(officerId);
            if (officer == null)
            {
                throw new Exception("Officer not found");
            }
            officer.FirstName = officerUpdateModel.Firstname;
            officer.LastName = officerUpdateModel.Lastname;
            officer.Email = officerUpdateModel.Email;
            officer.PhoneNumber = officerUpdateModel.PhoneNumber;
            await officerRepository.UpdateAsync(officer);
        }
    }
}
