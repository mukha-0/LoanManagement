using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Service.Services.Users.Models;

namespace LoanManagement.Service.Services.Users
{
    public interface IUserService
    {
        Task RegisterUser(UserCreateModel user);
        Task Login(UserLoginModel loginUser, int id);
        Task ChangePassword(UserUpdateModel updateUser, int id);
    }
}
