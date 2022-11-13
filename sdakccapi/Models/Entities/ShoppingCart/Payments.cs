using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace sdakccapi.Models.Entities
{
    public class Payments
    {
        [Key]        
        public long PaymentID { get; set; }
        public int PaymentModeID { get; set; }
        public long SaleID { get; set; }
        public decimal Amount { get; set; }
        public long CustomerID { get; set; }
        public string CardRef { get; set; }
        public string ChequeNo { get; set; }
        public string NameOnCheque { get; set; }
        public int BankID { get; set; }
        public DateTime BankingDate { get; set; }
        public int CustomerDepositID { get; set; }
        public int SupplierPaymentID { get; set; }
        public int SupplierID { get; set; }
        public int ExpenseID { get; set; }
        public string PaymentEmail { get; set; }
        public string PaymentDetailJson { get; set; }
    }
}
