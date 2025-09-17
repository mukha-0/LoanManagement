using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Entities;
using LoanManagement.Service.Services.Customers.Models;
using LoanManagement.Service.Services.Users.Models;

namespace LoanManagement.Service.Services.Users
{
    public interface IUserService
    {
        Task RegisterUserAsync(User user);
        Task<User> LoginAsync(UserLoginModel loginUser, int id);
        Task ChangePasswordAsync(UserUpdateModel updateUser, int id);
        Task<User?> GetUserByIdAsync(int id);
    }
}
