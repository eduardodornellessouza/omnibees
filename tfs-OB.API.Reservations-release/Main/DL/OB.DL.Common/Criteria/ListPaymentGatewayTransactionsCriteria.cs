﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OB.DL.Common.Criteria
{
    public class ListPaymentGatewayTransactionsCriteria
    {

        /// <summary>
        /// List of Paymentgatewaytransaction Ids to look for.
        /// </summary>
        public List<long> Ids { get; set; }

        /// <summary>
        /// List of PaymentGateway Names to look for.
        /// </summary>
        public List<string> PaymentGatewayNames { get; set; }

        /// <summary>
        /// List of Property Ids to look for.
        /// </summary>
        public List<long> PropertyIds { get; set; }

        /// <summary>
        /// List of PaymentGatewayOrder Ids to look for.
        /// </summary>
        public List<string> PaymentGatewayOrderIds { get; set; }

        /// <summary>
        /// List of PaymentGatewayAutoGenerated Ids to look for.
        /// </summary>
        public List<string> PaymentGatewayAutoGeneratedIds { get; set; }

        /// <summary>
        /// List of Transaction Ids to look for.
        /// </summary>
        public List<string> TransactionIds { get; set; }

        /// <summary>
        /// List of Transaction Codes to look for.
        /// </summary>
        public List<string> TransactionCodes { get; set; }

        /// <summary>
        /// List of Transaction Status to look for.
        /// </summary>
        public List<string> TransactionStatus { get; set; }

        /// <summary>
        /// List of Transaction Types to look for.
        /// </summary>
        public List<string> TransactionTypes { get; set; }

        /// <summary>
        /// List of RefundTransaction Ids to look for.
        /// </summary>
        public List<string> RefundTransactionIds { get; set; }

        /// <summary>
        /// List of PaypalProfile Ids to look for.
        /// </summary>
        public List<string> PaypalProfileIds { get; set; }

        /// <summary>
        /// List of TransactionRequests to look for.
        /// </summary>
        public List<string> TransactionRequests { get; set; }
        
    }
}