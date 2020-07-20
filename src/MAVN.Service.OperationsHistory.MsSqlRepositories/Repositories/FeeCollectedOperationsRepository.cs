﻿using System.Threading.Tasks;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.OperationsHistory.Domain.Models;
using MAVN.Service.OperationsHistory.Domain.Repositories;
using MAVN.Service.OperationsHistory.MsSqlRepositories.Entities;

namespace MAVN.Service.OperationsHistory.MsSqlRepositories.Repositories
{
    public class FeeCollectedOperationsRepository : IFeeCollectedOperationsRepository
    {
        private readonly PostgreSQLContextFactory<OperationsHistoryContext> _contextFactory;

        public FeeCollectedOperationsRepository(PostgreSQLContextFactory<OperationsHistoryContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddAsync(FeeCollectedOperationDto operation)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = FeeCollectedOperationEntity.Create(operation);

                var historyEntity = TransactionHistoryEntity.Create(operation);

                context.FeeCollectedOperations.Add(entity);

                context.TransactionHistories.Add(historyEntity);

                await context.SaveChangesAsync();
            }
        }
    }
}
