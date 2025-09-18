using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.Domain.Entities;

namespace LoanManagement.DataAccess.Repositories
{
    public interface IRepository<TEntity> where TEntity : class
    {
        Task<TEntity> InsertAsync(TEntity entity);
        Task UpdateAsync(TEntity entity);
        Task DeleteAsync(TEntity entity);
        Task<TEntity> SelectAsync(int id);
        IQueryable<TEntity> SelectAllAsQueryable();
        Task<TEntity> SelectAsync(string status);
    }
}
