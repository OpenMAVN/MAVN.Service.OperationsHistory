﻿using System.Threading.Tasks;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.OperationsHistory.Domain.Models;
using MAVN.Service.OperationsHistory.Domain.Repositories;
using MAVN.Service.OperationsHistory.MsSqlRepositories.Entities;

namespace MAVN.Service.OperationsHistory.MsSqlRepositories.Repositories
{
    public class LinkedWalletTransfersRepository : ILinkedWalletTransfersRepository
    {
        private readonly PostgreSQLContextFactory<OperationsHistoryContext> _contextFactory;

        public LinkedWalletTransfersRepository(PostgreSQLContextFactory<OperationsHistoryContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task AddAsync(LinkedWalletTransferDto transfer)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var entity = LinkedWalletTransferEntity.Create(transfer);

                var historyEntity = TransactionHistoryEntity.CreateForLinkedWalletTransfer(transfer);
                
                context.LinkedWalletTransfers.Add(entity);
                
                context.TransactionHistories.Add(historyEntity);

                await context.SaveChangesAsync();
            }
        }
    }
}
