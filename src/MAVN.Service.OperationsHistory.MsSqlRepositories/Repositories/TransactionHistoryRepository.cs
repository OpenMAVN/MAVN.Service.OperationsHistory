﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MAVN.Persistence.PostgreSQL.Legacy;
using MAVN.Service.OperationsHistory.Domain.Models;
using MAVN.Service.OperationsHistory.Domain.Repositories;
using Microsoft.EntityFrameworkCore;
using MoreLinq;

namespace MAVN.Service.OperationsHistory.MsSqlRepositories.Repositories
{
    public class TransactionHistoryRepository : ITransactionHistoryRepository
    {
        private readonly PostgreSQLContextFactory<OperationsHistoryContext> _contextFactory;

        private static readonly string[] CustomerIsActiveOperationTypes =
        {
            OperationType.PaymentTransferTokensReserved.ToString(),
            OperationType.BonusCashIn.ToString(),
            OperationType.PartnersPaymentTokensReserved.ToString(),
            OperationType.ReferralStakeTokensReserved.ToString(),
            OperationType.VoucherPurchasePayment.ToString()
        };

        public TransactionHistoryRepository(PostgreSQLContextFactory<OperationsHistoryContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<PaginatedCustomerOperationsModel> GetByCustomerIdPaginatedAsync(string customerId, int skip, int take)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var transactions = context.TransactionHistories
                    .Where(t => t.CustomerId == customerId);

                var totalCount = await transactions.CountAsync();

                var transactionsQuery = transactions
                    .OrderByDescending(t => t.Timestamp)
                    .Skip(skip)
                    .Take(take);

                var bonuses = await context.BonusCashIns
                    .Where(b => transactionsQuery.Any(x => x.TransactionId == b.TransactionId))
                    .Include(b => b.Campaign)
                    .Select(b => new BonusCashInDto
                    {
                        Amount = b.Amount,
                        CampaignName = b.Campaign.CampaignName,
                        CampaignId = b.CampaignId,
                        AssetSymbol = b.AssetSymbol,
                        TransactionId = b.TransactionId,
                        Timestamp = b.Timestamp,
                        CustomerId = b.CustomerId,
                        BonusType = b.BonusType,
                        PartnerId = b.PartnerId,
                        LocationId = b.LocationId,
                        ExternalOperationId = b.ExternalOperationId,
                        ConditionName = b.ConditionName
                    })
                    .ToArrayAsync();

                var transfers = await context.Transfers
                    .Where(t => transactionsQuery.Any(x => x.TransactionId == t.TransactionId))
                    .Select(t => new Transfer
                    {
                        Amount = t.Amount,
                        Timestamp = t.Timestamp,
                        AssetSymbol = t.AssetSymbol,
                        TransactionId = t.TransactionId,
                        ReceiverCustomerId = t.ReceiverCustomerId,
                        SenderCustomerId = t.SenderCustomerId,
                        ExternalOperationId = t.ExternalOperationId,
                        SenderWalletAddress = t.SenderWalletAddress,
                        ReceiverWalletAddress = t.ReceiverWalletAddress,
                    })
                    .ToArrayAsync();

                var partnersPayments = await context.PartnersPayments
                    .Where(p => transactionsQuery.Any(x =>
                        x.TransactionId == p.PaymentRequestId &&
                        x.Type == OperationType.PartnersPaymentTokensReserved.ToString()))
                    .ToArrayAsync();

                var refundedPartnersPayments = await context.RefundedPartnersPayments
                    .Where(p => transactionsQuery.Any(x =>
                        x.TransactionId == p.PaymentRequestId &&
                        x.Type == OperationType.PartnersPaymentRefunded.ToString()))
                    .ToArrayAsync();

                var referralStakes = await context.ReferralStakes
                    .Where(p => transactionsQuery.Any(x =>
                        x.TransactionId == p.ReferralId &&
                        x.Type == OperationType.ReferralStakeTokensReserved.ToString()))
                    .Select(r => new ReferralStakeDto
                    {
                        ReferralId = r.ReferralId,
                        CampaignId = r.CampaignId,
                        CustomerId = r.CustomerId,
                        AssetSymbol = r.AssetSymbol,
                        Timestamp = r.Timestamp,
                        Amount = r.Amount,
                        CampaignName = r.Campaign.CampaignName
                    })
                    .ToArrayAsync();

                var releasedReferralStakes = await context.ReleasedReferralStakes
                    .Where(p => transactionsQuery.Any(x =>
                        x.TransactionId == p.ReferralId &&
                        x.Type == OperationType.ReferralStakeTokensReleased.ToString()))
                    .Select(r => new ReferralStakeDto
                    {
                        ReferralId = r.ReferralId,
                        CampaignId = r.CampaignId,
                        CustomerId = r.CustomerId,
                        AssetSymbol = r.AssetSymbol,
                        Timestamp = r.Timestamp,
                        Amount = r.Amount,
                        CampaignName = r.Campaign.CampaignName
                    })
                    .ToArrayAsync();

                var linkedWalletTransfers = await context.LinkedWalletTransfers
                    .Where(t => transactionsQuery.Any(x => x.TransactionId == t.Id))
                    .Select(x => new LinkedWalletTransferDto
                    {
                        Amount = x.Amount,
                        Direction = x.Direction,
                        AssetSymbol = x.AssetSymbol,
                        Timestamp = x.Timestamp,
                        CustomerId = x.CustomerId,
                        OperationId = x.Id,
                        PrivateAddress = x.PrivateAddress,
                        PublicAddress = x.PublicAddress
                    })
                    .ToArrayAsync();

                var feeCollectedOperations = await context.FeeCollectedOperations
                    .Where(t => transactionsQuery.Any(x => x.TransactionId == t.Id))
                    .Select(x => new FeeCollectedOperationDto
                    {
                        CustomerId = x.CustomerId,
                        Timestamp = x.Timestamp,
                        OperationId = x.Id,
                        Reason = x.Reason,
                        Fee = x.Fee,
                        AssetSymbol = x.AssetSymbol,
                    })
                    .ToArrayAsync();

                var voucherPurchasePayments = await context.VoucherPurchasePayments
                    .Where(t => transactionsQuery.Any(x => x.TransactionId == t.TransferId.ToString()))
                    .Select(x => new VoucherPurchasePaymentDto
                    {
                        TransferId = x.TransferId,
                        CustomerId = x.CustomerId,
                        SpendRuleId = x.SpendRuleId,
                        VoucherId = x.VoucherId,
                        Amount = x.Amount,
                        AssetSymbol = x.AssetSymbol,
                        Timestamp = x.Timestamp
                    })
                    .ToListAsync();

                var smartVoucherPayments = await context.SmartVoucherPayments
                    .Include(x => x.Campaign)
                    .Where(t => transactionsQuery.Any(x => x.TransactionId == t.PaymentRequestId))
                    .Select(x => new SmartVoucherPaymentDto
                    {
                        PaymentRequestId = x.PaymentRequestId,
                        Timestamp = x.Timestamp,
                        CustomerId = x.CustomerId,
                        PartnerName = x.PartnerName,
                        Vertical = x.Vertical,
                        PartnerId = x.PartnerId,
                        CampaignId = x.CampaignId,
                        AssetSymbol = x.AssetSymbol,
                        CampaignName = x.Campaign.CampaignName,
                        ShortCode = x.ShortCode,
                        Amount = x.Amount,
                    })
                    .ToListAsync();

                var smartVoucherUses = await context.SmartVoucherUses
                    .Include(x => x.Campaign)
                    .Where(t => transactionsQuery.Any(x => x.TransactionId == t.Id))
                    .Select(x => new SmartVoucherUseDto
                    {
                        Id = x.Id,
                        Timestamp = x.Timestamp,
                        CustomerId = x.CustomerId,
                        PartnerName = x.PartnerName,
                        Vertical = x.Vertical,
                        PartnerId = x.PartnerId,
                        CampaignId = x.CampaignId,
                        AssetSymbol = x.AssetSymbol,
                        CampaignName = x.Campaign.CampaignName,
                        Amount = x.Amount,
                        LinkedCustomerId = x.LinkedCustomerId,
                        LocationId = x.LocationId,
                    })
                    .ToListAsync();

                var smartVoucherTransfers = await context.SmartVoucherTransfers
                    .Include(x => x.Campaign)
                    .Where(t => transactionsQuery.Any(x => x.TransactionId == t.Id))
                    .Select(x => new SmartVoucherTransferDto
                    {
                        Id = x.Id,
                        Timestamp = x.Timestamp,
                        PartnerName = x.PartnerName,
                        Vertical = x.Vertical,
                        PartnerId = x.PartnerId,
                        CampaignId = x.CampaignId,
                        AssetSymbol = x.AssetSymbol,
                        CampaignName = x.Campaign.CampaignName,
                        Amount = x.Amount,
                        NewCustomerId = x.NewCustomerId,
                        ShortCode = x.ShortCode,
                        OldCustomerId = x.OldCustomerId,
                    })
                    .ToListAsync();

                return new PaginatedCustomerOperationsModel
                {
                    Transfers = transfers,
                    BonusCashIns = bonuses,
                    TotalCount = totalCount,
                    PartnersPayments = partnersPayments,
                    RefundedPartnersPayments = refundedPartnersPayments,
                    ReferralStakes = referralStakes,
                    ReleasedReferralStakes = releasedReferralStakes,
                    LinkedWalletTransfers = linkedWalletTransfers,
                    FeeCollectedOperations = feeCollectedOperations,
                    VoucherPurchasePayments = voucherPurchasePayments,
                    SmartVoucherPayments = smartVoucherPayments,
                    SmartVoucherUses = smartVoucherUses,
                    SmartVoucherTransfers = smartVoucherTransfers,
                };
            }
        }

        public async Task<PaginatedTransactionHistory> GetByDatePaginatedAsync(DateTime dateFrom, DateTime dateTo, int skip, int take)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var transactions = context.TransactionHistories
                    .Where(t => t.Timestamp >= dateFrom && t.Timestamp < dateTo);

                var totalCount = await transactions.CountAsync();

                var result = await transactions
                    .OrderByDescending(t => t.Timestamp)
                    .Skip(skip)
                    .Take(take)
                    .ToArrayAsync();

                return new PaginatedTransactionHistory
                {
                    TransactionsHistory = result,
                    TotalCount = totalCount
                };
            }
        }

        public async Task<int> GetActiveCustomersCountAsync(DateTime dateFrom, DateTime dateTo)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var result = await context.TransactionHistories
                    .Where(b => b.Timestamp >= dateFrom
                                && b.Timestamp <= dateTo
                                && CustomerIsActiveOperationTypes.Contains(b.Type))
                    .Select(b => b.CustomerId)
                    .Distinct()
                    .CountAsync();

                return result;
            }
        }

        public async Task<CustomersStatisticListModel> GetActiveCustomersStatisticAsync(DateTime fromDate, DateTime toDate)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var query = await context.TransactionHistories
                    .Where(b => b.Timestamp.Date >= fromDate.Date
                                && b.Timestamp.Date <= toDate.Date
                                && CustomerIsActiveOperationTypes.Contains(b.Type))
                    .Select(c => new
                    {
                        c.Timestamp,
                        c.CustomerId
                    })
                    .ToListAsync();

                var statisticDictionary = query.GroupBy(c => c.Timestamp.Date)
                    .ToDictionary(k => k.Key, v => v.DistinctBy(c => c.CustomerId).Count());

                var listStatistics = new List<CustomerStatisticModel>();

                foreach (var day in EachDay(fromDate, toDate))
                {
                    var statistic = new CustomerStatisticModel
                    {
                        Day = day.Date,
                        Count = 0
                    };

                    if (statisticDictionary.ContainsKey(day.Date))
                    {
                        statistic.Count = statisticDictionary[day.Date];
                    }

                    listStatistics.Add(statistic);
                }

                return new CustomersStatisticListModel
                {
                    ActiveCustomers = listStatistics,
                    TotalActiveCustomers = query.DistinctBy(c => c.CustomerId).Count()
                };
            }
        }

        private static IEnumerable<DateTime> EachDay(DateTime fromDate, DateTime toDate)
        {
            for (var day = fromDate.Date; day.Date <= toDate.Date; day = day.AddDays(1))
                yield return day;
        }

        public async Task<IEnumerable<IBonusCashIn>> GetBonusCashInsAsync(string customerId, string campaignId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var bonuses = await context.BonusCashIns
                    .Where(b => b.CustomerId == customerId && b.CampaignId == campaignId)
                    .Include(b => b.Campaign)
                    .ToArrayAsync();

                return bonuses;
            }
        }

        public async Task<IEnumerable<IBonusCashIn>> GetBonusCashInsByReferralAsync(string customerId, string referralId)
        {
            using (var context = _contextFactory.CreateDataContext())
            {
                var bonuses = await context.BonusCashIns
                    .Where(b => b.CustomerId == customerId && b.ReferralId == referralId)
                    .Include(b => b.Campaign)
                    .ToArrayAsync();

                return bonuses;
            }
        }
    }
}
