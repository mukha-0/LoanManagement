using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.DataAccess.Repositories;
using LoanManagement.Domain.Entities;
using LoanManagement.Service.Exceptions;
using LoanManagement.Service.Services.AllEntries.Users.Models;
using Microsoft.EntityFrameworkCore;

namespace LoanManagement.Service.Services.AllEntries.Users
{
    public class UserService : IUserService
    {
        private readonly IRepository<User> userRepositoy;
        public UserService()
        {
            userRepositoy = new Repository<User>();
        }
        public async Task ChangePasswordAsync(UserUpdateModel updateUser, int id)
        {
            var user = userRepositoy.SelectAsync(id)
                ?? throw new NotFoundException("User not found");
            if (string.IsNullOrWhiteSpace(updateUser.NewPassword))
            {
                throw new ArgumentException("New password is required");
            }
            user.Result.Password = updateUser.NewPassword;
            await userRepositoy.UpdateAsync(user.Result);
        }
        public async Task<IEnumerable<object>> GetAllUsersAsync()
        {
            return userRepositoy.SelectAllAsQueryable().ToList();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await userRepositoy.SelectAsync(id);
        }

        public async Task<User> LoginAsync(UserLoginModel loginUser)
        {
            var user = await userRepositoy
                .SelectAllAsQueryable()
                .FirstOrDefaultAsync(u => u.UserName == loginUser.Username && u.Password == loginUser.Password);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid username or password");
            return user;
        }
        public async Task RegisterUserAsync(UserRegisterModel user)
        {
            if (string.IsNullOrWhiteSpace(user.UserName))
                throw new ArgumentException("Username is required");
            if (string.IsNullOrWhiteSpace(user.PasswordHash))
                throw new ArgumentException("Password is required");
            var existingUser = userRepositoy
                .SelectAllAsQueryable()
                .FirstOrDefault(u => u.UserName == user.UserName);
            if (existingUser != null)
                throw new Exception("Username already exists");

            _ = await userRepositoy.InsertAsync(new User
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Password = user.PasswordHash,
                CreatedAt = DateTime.UtcNow,
                Email = user.Email,
                Role = user.Role,
            });
        }
    }
}
