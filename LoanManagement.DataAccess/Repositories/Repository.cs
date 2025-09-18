using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LoanManagement.DataAccess.Context;
using LoanManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LoanManagement.DataAccess.Repositories
{
    public class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly AppDBContext context;
        public Repository()
        {
            context = new AppDBContext();
            context.Set<TEntity>();
        }

        public async Task<TEntity> InsertAsync(TEntity entity)
        {
            var createdEntity = (await context.AddAsync(entity)).Entity;
            await context.SaveChangesAsync();
            return createdEntity;
        }

        public async Task UpdateAsync(TEntity entity)
        {
            context.Update(entity);
            await context.SaveChangesAsync();
        }

        public async Task DeleteAsync(TEntity entity)
        {
            context.Remove(entity);
            await context.SaveChangesAsync();
        }

        public async Task<TEntity?> SelectAsync(int id)
        {
            return await context.Set<TEntity>().FindAsync(id);
        }

        public IQueryable<TEntity> SelectAllAsQueryable()
        {
            return context.Set<TEntity>().AsQueryable();
        }


        public async Task<TEntity?> SelectAsync(string status)
        {

            return await context.Set<TEntity>().FirstOrDefaultAsync(e => EF.Property<string>(e, "Status") == status);
        }

    }
}
