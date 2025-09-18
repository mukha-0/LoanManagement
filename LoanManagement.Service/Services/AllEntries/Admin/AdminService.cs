using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.DataAccess.Repositories;
using LoanManagement.Domain.Entities;
using LoanManagement.Service.Services.AllEntries.Admin;

namespace LoanManagement.Service.Services.AllEntries
{
    public class AdminService: IAdminModel
    {
        private readonly IRepository<Adminn> adminRepository;
        private readonly IRepository<User> userRepository;
        private readonly IRepository<LoanOfficerr> officerRepository;
        public AdminService()
        {
            adminRepository = new Repository<Adminn>();
            userRepository = new Repository<User>();
            officerRepository = new Repository<LoanOfficerr>();
        }

        public async Task DeleteOfficerAsync(int id)
        {
            var officer = await officerRepository.SelectAsync(id);
            if (officer != null)
            {
                await officerRepository.DeleteAsync(officer);
            }
            else
            {
                throw new Exception("Officer not found");
            }
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await userRepository.SelectAsync(id);
            if (user != null)
            {
                await userRepository.DeleteAsync(user);
            }
            else
            {
                throw new Exception("User not found");
            }
        }

        public async Task<IEnumerable<object>> GetAllOfficersAsync()
        {
            return officerRepository.SelectAllAsQueryable().ToList();
        }

        public async Task<IEnumerable<object>> GetAllUsersAsync()
        {
            return userRepository.SelectAllAsQueryable().ToList();
        }
        public async Task AddNewAdmin(Adminn admin)
        {
            var existingAdmin = await adminRepository.SelectAsync(admin.AdminId);
            if (existingAdmin != null)
            {
                throw new Exception("Admin with the same ID already exists.");
            }
            await adminRepository.InsertAsync(admin);
        }
    }
}
