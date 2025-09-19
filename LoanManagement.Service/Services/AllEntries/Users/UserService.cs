using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Bots.AdminBot;
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
        private IUserService userService;

        public UserService(DataAccess.Context.AppDBContext db)
        {
            userRepositoy = new Repository<User>();
        }

        public UserService(IUserService userService)
        {
            this.userService = userService;
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
        public async Task RegisterUserAsync(UserRegisterModel userModel)
        {
            var existingUser = await userRepositoy
                .SelectAllAsQueryable()
                .FirstOrDefaultAsync(u => u.UserName == userModel.Username);
            if (existingUser != null)
            {
                throw new Exception("Username already exists");
            }
            var newUser = new User
            {
                FirstName = userModel.FullName,
                Email = userModel.Email,
                PhoneNumber = userModel.PhoneNumber,
                UserName = userModel.Username,
                Password = userModel.Password,
                CreatedAt = DateTime.UtcNow,
                Role = Domain.Enums.Role.User,
            };
            await userRepositoy.InsertAsync(newUser);
        }
    }
}
