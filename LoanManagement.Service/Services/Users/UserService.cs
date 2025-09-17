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
        public async Task ChangePasswordAsync(UserUpdateModel updateUser, int id)
        {
            var user = await userRepositoy.SelectAsync(id)
                ?? throw new NotFoundException("User not found"); 

            user.PasswordHash = updateUser.NewPassword;
            _ = userRepositoy.UpdateAsync(user);
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await userRepositoy.SelectAsync(id);
        }

        public async Task<User> LoginAsync(UserLoginModel loginUser, int id)
        {
            var user = await userRepositoy.SelectAsync(id)
                ?? throw new NotFoundException("User not found");
            if (user.PasswordHash != loginUser.Password)
            {
                throw new UnauthorizedAccessException("Invalid credentials");
            }
            return user;
        }

        public async Task RegisterUserAsync(User user)
        {
            if (string.IsNullOrWhiteSpace(user.UserName))
                throw new ArgumentException("Username is required");
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                throw new ArgumentException("Password is required");
            var existingUser = userRepositoy
                .SelectAllAsQueryable()
                .FirstOrDefault(u => u.UserName == user.UserName);
            if (existingUser == null)
                throw new Exception("Username already exists");

            _ = await userRepositoy.InsertAsync(new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                PasswordHash = user.PasswordHash,
                Role = user.Role
            });
        }
    }
}
