using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.DataAccess.Repositories;
using LoanManagement.Domain.Entities;
using LoanManagement.Service.Exceptions;
using LoanManagement.Service.Services.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace LoanManagement.Service.Services.Users
{
    internal class UserService : IUserService
    {
        private readonly IRepository<User> userRepositoy;
        public UserService()
        {
            userRepositoy = new Repository<User>();
        }
        public async Task ChangePassword(UserUpdateModel updateUser, int id)
        {
            var user = await userRepositoy.SelectAsync(id)
                ?? throw new NotFoundException("User not found"); 

            user.PasswordHash = updateUser.NewPassword;
            _ = userRepositoy.UpdateAsync(user);
        }

        public async Task Login(UserLoginModel loginUser, int id)
        {
            var user = await userRepositoy.SelectAsync(id)
                ?? throw new NotFoundException("User not found");

            if (user.PasswordHash != loginUser.Password || user.UserName != loginUser.Username)
            {
                throw new BadRequestException("Invalid credentials");
            }
        }

        public async Task RegisterUser(UserCreateModel user)
        {
            _ = await userRepositoy.InsertAsync(new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PasswordHash = user.Password,
                Role = user.Role
            });
        }
    }
}
