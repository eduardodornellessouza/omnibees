namespace OB.Reservation.BL.Contracts.Data
{
    public partial class PaymentResultReportMessage
    {
        public PaymentResultReportMessage()
        {
        }

        //[Key]
        public string OrderID { get; set; }

        public string ApprovalCode { get; set; }

        public string CreditCardType { get; set; }

        public string TransactionAmount { get; set; }

        public string TransactionDate { get; set; }

        public string TransactionID { get; set; }

        public string TransactionStatus { get; set; }

        public string TransactionType { get; set; }

        public string PaymentType { get; set; }

        public string ProcessorID { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorMessage { get; set; }

        public string ReferenceNumber { get; set; }

        public string Comments { get; set; }
    }
}