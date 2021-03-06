﻿using System;

namespace MAVN.Service.OperationsHistory.Domain.Models
{
    public class SmartVoucherUseDto
    {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public Guid CustomerId { get; set; }
        public Guid? LinkedCustomerId { get; set; }
        public Guid PartnerId { get; set; }
        public Guid? LocationId { get; set; }
        public string CampaignId { get; set; }
        public decimal Amount { get; set; }
        public string AssetSymbol { get; set; }
        public string CampaignName { get; set; }
        public string PartnerName { get; set; }
        public string Vertical { get; set; }
    }
}
