using MedicationService.Application.Interfaces;
using MedicationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace MedicationService.Infrastructure
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MedicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(MedicationDbContext context)
        {
            _context = context;
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}

