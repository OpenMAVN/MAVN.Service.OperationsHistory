﻿using System;

namespace MAVN.Service.OperationsHistory.Client.Models.Responses
{
    /// <summary>
    /// Response model for smart voucher use
    /// </summary>
    public class SmartVoucherUseResponse
    {
        /// <summary>
        /// Id of the operation
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Timestamp
        /// </summary>
        public DateTime Timestamp { get; set; }
        /// <summary>
        /// id of the customer
        /// </summary>
        public Guid CustomerId { get; set; }
        /// <summary>
        /// Id of the linked customer 
        /// </summary>
        public Guid? LinkedCustomerId { get; set; }
        /// <summary>
        /// Id of the partner
        /// </summary>
        public Guid PartnerId { get; set; }
        /// <summary>
        /// Id of the location
        /// </summary>
        public Guid? LocationId { get; set; }
        /// <summary>
        /// Id of the campaign
        /// </summary>
        public string CampaignId { get; set; }
        /// <summary>
        /// amount of the voucher
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Asset symbol (currency)
        /// </summary>
        public string AssetSymbol { get; set; }
        /// <summary>
        /// Name of the campaign
        /// </summary>
        public string CampaignName { get; set; }
        /// <summary>
        /// Name of the partner
        /// </summary>
        public string PartnerName { get; set; }
        /// <summary>
        /// Vertical of the partner
        /// </summary>
        public string Vertical { get; set; }
    }
}
