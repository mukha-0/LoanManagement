using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Bots.AdminBot;
using LoanManagement.Domain.Entities;
using LoanManagement.Service.Services.AllEntries.Users.Models;

namespace LoanManagement.Service.Services.AllEntries.Users
{
    public interface IUserService
    {
        Task<User> LoginAsync(UserLoginModel loginUser);
        Task ChangePasswordAsync(UserUpdateModel updateUser, int id);
        Task<User?> GetUserByIdAsync(int id);
        Task<IEnumerable<object>> GetAllUsersAsync();
        Task RegisterUserAsync(UserRegisterModel userModel);
    }
}
