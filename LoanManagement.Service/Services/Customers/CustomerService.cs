using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.DataAccess.Repositories;
using LoanManagement.Domain.Entities;
using LoanManagement.Service.Exceptions;
using LoanManagement.Service.Services.Customers.Models;

namespace LoanManagement.Service.Services.Customers
{
    public class CustomerService : ICustomerService
    {
        private readonly IRepository<Customer> customerRepository;

        public CustomerService()
        {
            customerRepository = new Repository<Customer>();
        }
        public async Task AddCustomerAsync(CustomerCreateModel customerCreate)
        {
            var customer = new Customer
            {
                FirstName = customerCreate.FirstName,
                LastName = customerCreate.LastName,
                Email = customerCreate.Email,
                PhoneNumber = customerCreate.PhoneNumber,
                Address = customerCreate.Address,
                AnnualIncome = customerCreate.AnnualIncome,
                DateOfBirth = customerCreate.DateOfBirth,
                IdentificationNumber = customerCreate.IdentificationNumber,
                CreatedAt = DateTime.UtcNow
            };
            await customerRepository.InsertAsync(customer);
        }

        public async Task DeleteCustomerAsync(int id)
        {
            var customer = await customerRepository.SelectAsync(id)
               ?? throw new NotFoundException("User is not found");

            await customerRepository.DeleteAsync(customer);
        }

        public async Task<List<Customer>> GetAllCustomersAsync()
        {
            var customers = customerRepository.SelectAllAsQueryable().ToList();

            if (customers == null || !customers.Any())
            {
                throw new NotFoundException("No customers found");
            }

            return await Task.FromResult(customers);
        }

        public async Task<Customer> GetCustomerByIdAsync(int id)
        {
            var customer = await customerRepository.SelectAsync(id)
                ?? throw new NotFoundException("Customer not found");
            return customer;
        }

        public async Task UpdateCustomerAsync(CustomerUpdateModel customerUpdateModel, int customerId)
        {
            var customer = await customerRepository.SelectAsync(customerId)
                ?? throw new NotFoundException("Customer not found");

            customer.FirstName = customerUpdateModel.FirstName;
            customer.LastName = customerUpdateModel.LastName;
            customer.Email = customerUpdateModel.Email;
            customer.PhoneNumber = customerUpdateModel.PhoneNumber;
            customer.Address = customerUpdateModel.Address;
            customer.DateOfBirth = customerUpdateModel.DateOfBirth;
            customer.AnnualIncome = customerUpdateModel.AnnualIncome;

            await customerRepository.UpdateAsync(customer);
        }
    }
}
