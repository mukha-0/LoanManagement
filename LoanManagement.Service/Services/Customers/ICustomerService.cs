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
        Task AddCustomer(CustomerCreateModel customerCreate);
        Task<List<Customer>> GetAllCustomers();
        Task<CustomerViewModel> GetCustomerById(int id);
        Task UpdateCustomer(CustomerUpdateModel customerUpdateModel, int id);
        Task DeleteCustomer(int id);
    }
}
