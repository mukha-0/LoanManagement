using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Entities;
using LoanManagement.Service.Services.Customers.Models;
using Microsoft.Win32;

namespace LoanManagement.Service.Services.Customers
{
    public interface ICustomerService
    {
        Task AddCustomerAsync(CustomerCreateModel customerCreate);
        Task<List<Customer>> GetAllCustomersAsync();
        Task<Customer> GetCustomerByIdAsync(int id);
        Task UpdateCustomerAsync(CustomerUpdateModel customerUpdateModel, int id);
        Task DeleteCustomerAsync(int id);
    }
}
